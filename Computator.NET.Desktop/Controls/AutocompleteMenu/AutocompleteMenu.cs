//
//  THIS CODE AND INFORMATION IS PROVIDED "AS IS" WITHOUT WARRANTY OF ANY
//  KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE
//  IMPLIED WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A PARTICULAR
//  PURPOSE.
//
//  License: GNU Lesser General Public License (LGPLv3)
//
//  Email: pavel_torgashov@mail.ru.
//
//  Copyright (C) Pavel Torgashov, 2012. 

//MERGED WITH MASTER ON 13.12.2015
//#define USE_TEXT_WIDTH

using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using Computator.NET.Core.Autocompletion;
using Computator.NET.Core.Model;
using Computator.NET.Core.Properties;
using Computator.NET.DataTypes.Text;
using Computator.NET.Desktop.Controls.AutocompleteMenu.Wrappers;
using Computator.NET.Desktop.Services;

namespace Computator.NET.Desktop.Controls.AutocompleteMenu
{
    [ProvideProperty("AutocompleteMenu", typeof(Control))]
    public class AutocompleteMenu : Component, IExtenderProvider
    {
        private static readonly Dictionary<Control, AutocompleteMenu>
            AutocompleteMenuByControls =
                new Dictionary<Control, AutocompleteMenu>();

        private static readonly Dictionary<Control, ITextBoxWrapper>
            WrapperByControls =
                new Dictionary<Control, ITextBoxWrapper>();

        private readonly Timer _timer = new Timer();
        private bool _forcedOpened;
        private Size _maximumSize;
        private Form _myForm;

        private IEnumerable<AutocompleteItem> _sourceItems =
            new List<AutocompleteItem>();

        private ITextBoxWrapper _targetControlWrapper;

        public AutocompleteMenu(ISharedViewState sharedViewState)
        {
            _sharedViewState = sharedViewState;
            Host = new AutocompleteMenuHost(this);
            Host.ListView.ItemSelected += ListView_ItemSelected;
            Host.ListView.ItemHovered += ListView_ItemHovered;
            VisibleItems = new List<AutocompleteItem>();
            Enabled = true;
            AppearInterval = 500;
            _timer.Tick += Timer_Tick;
#if __MonoCS__
            // On Mono a 2,000-pixel completion menu obscures the editor and
            // leaves no room for its function reference. Size to the items.
            var preferred = new Size(580, 360).DpiScale();
#else
            var preferred = new Size(2000, 420).DpiScale();
#endif
            var available = Screen.FromControl(Host).WorkingArea.Size;
            MaximumSize = new Size(Math.Min(preferred.Width, Math.Max(1, available.Width - 4)),
                Math.Min(preferred.Height, Math.Max(1, available.Height - 4)));
            AutoPopup = true;

            SearchPattern = @"[\w\.]";
            MinFragmentLength = 2;

            SetupAutocomplete();
        }

        [Browsable(false)]
        public IList<AutocompleteItem> VisibleItems
        {
            get => Host.ListView.VisibleItems;
            private set => Host.ListView.VisibleItems = value;
        }

        /// <summary>
        ///     Duration (ms) of tooltip showing
        /// </summary>
        [Description("Duration (ms) of tooltip showing")]
        [DefaultValue(3000)]
        public int ToolTipDuration
        {
            get => Host.ListView.ToolTipDuration;
            set => Host.ListView.ToolTipDuration = value;
        }

        [Browsable(false)]
        [System.ComponentModel.DesignerSerializationVisibility(System.ComponentModel.DesignerSerializationVisibility.Hidden)]
        public int SelectedItemIndex
        {
            get => Host.ListView.SelectedItemIndex;
            internal set => Host.ListView.SelectedItemIndex = value;
        }

        [System.ComponentModel.DesignerSerializationVisibility(System.ComponentModel.DesignerSerializationVisibility.Hidden)]
        internal AutocompleteMenuHost Host { get; set; }

        /// <summary>
        ///     Current target control wrapper
        /// </summary>
        [Browsable(false)]
        [System.ComponentModel.DesignerSerializationVisibility(System.ComponentModel.DesignerSerializationVisibility.Hidden)]
        public ITextBoxWrapper TargetControlWrapper
        {
            get => _targetControlWrapper;
            set
            {
                _targetControlWrapper = value;
                if (value != null && !WrapperByControls.ContainsKey(value.TargetControl))
                {
                    WrapperByControls[value.TargetControl] = value;
                    SetAutocompleteMenu(value.TargetControl, this);
                }
            }
        }

        /// <summary>
        ///     Maximum size of popup menu
        /// </summary>
        [DefaultValue(typeof(Size), "180, 200")]
        [Description("Maximum size of popup menu")]
        public Size MaximumSize
        {
            get { return _maximumSize; }
            set
            {
                _maximumSize = value;
                (Host.ListView as Control).MaximumSize = _maximumSize;
                (Host.ListView as Control).Size = _maximumSize;
                Host.CalcSize();
            }
        }

        /// <summary>
        ///     Font
        /// </summary>
        [System.ComponentModel.DesignerSerializationVisibility(System.ComponentModel.DesignerSerializationVisibility.Visible)]
        public Font Font
        {
            get => (Host.ListView as Control).Font;
            set => (Host.ListView as Control).Font = CustomFonts.GetFontFromFont(value);
        }

        /// <summary>
        ///     Left padding of text
        /// </summary>
        [DefaultValue(18)]
        [Description("Left padding of text")]
        public int LeftPadding
        {
            get
            {
                if (Host.ListView is AutocompleteListView)
                    return (Host.ListView as AutocompleteListView).LeftPadding;
                return 0;
            }
            set
            {
                if (Host.ListView is AutocompleteListView)
                    (Host.ListView as AutocompleteListView).LeftPadding = value;
            }
        }

        /// <summary>
        ///     Colors of foreground and background
        /// </summary>
        [Browsable(true)]
        [Description("Colors of foreground and background.")]
        [TypeConverter(typeof(ExpandableObjectConverter))]
        [System.ComponentModel.DesignerSerializationVisibility(System.ComponentModel.DesignerSerializationVisibility.Visible)]
        public Colors Colors
        {
            get => Host.ListView.Colors;
            set => Host.ListView.Colors = value;
        }

        /// <summary>
        ///     AutocompleteMenu will popup automatically (when user writes text). Otherwise it will popup only programmatically or
        ///     by Ctrl-Space.
        /// </summary>
        [DefaultValue(true)]
        [Description(
            "AutocompleteMenu will popup automatically (when user writes text). Otherwise it will popup only programmatically or by Ctrl-Space."
            )]
        public bool AutoPopup { get; set; }

        /// <summary>
        ///     AutocompleteMenu will capture focus when opening.
        /// </summary>
        [DefaultValue(false)]
        [Description("AutocompleteMenu will capture focus when opening.")]
        public bool CaptureFocus { get; set; }

        /// <summary>
        ///     Indicates whether the component should draw right-to-left for RTL languages.
        /// </summary>
        [DefaultValue(typeof(RightToLeft), "No")]
        [Description(
            "Indicates whether the component should draw right-to-left for RTL languages.")]
        public RightToLeft RightToLeft
        {
            get { return Host.RightToLeft; }
            set { Host.RightToLeft = value; }
        }

        /// <summary>
        ///     Image list
        /// </summary>
        [System.ComponentModel.DesignerSerializationVisibility(System.ComponentModel.DesignerSerializationVisibility.Visible)]
        public ImageList ImageList
        {
            get { return Host.ListView.ImageList; }
            set { Host.ListView.ImageList = value; }
        }

        /// <summary>
        ///     Fragment
        /// </summary>
        [Browsable(false)]
        [System.ComponentModel.DesignerSerializationVisibility(System.ComponentModel.DesignerSerializationVisibility.Hidden)]
        public Range Fragment { get; internal set; }

        /// <summary>
        ///     Regex pattern for serach fragment around caret
        /// </summary>
        [Description("Regex pattern for serach fragment around caret")]
        [DefaultValue(@"[\w\.]")]
        public string SearchPattern { get; set; }

        /// <summary>
        ///     Minimum fragment length for popup
        /// </summary>
        [Description("Minimum fragment length for popup")]
        [DefaultValue(2)]
        public int MinFragmentLength { get; set; }

        /// <summary>
        ///     Allows TAB for select menu item
        /// </summary>
        [Description("Allows TAB for select menu item")]
        [DefaultValue(false)]
        public bool AllowsTabKey { get; set; }

        /// <summary>
        ///     Interval of menu appear (ms)
        /// </summary>
        [Description("Interval of menu appear (ms)")]
        [DefaultValue(500)]
        public int AppearInterval { get; set; }

        [DefaultValue(null)]
        public string[] Items
        {
            get
            {
                if (_sourceItems == null)
                    return null;
                var list = new List<string>();
                foreach (var item in _sourceItems)
                    list.Add(item.ToString());
                return list.ToArray();
            }
            set => SetAutocompleteItems(value);
        }

        /// <summary>
        ///     The control for menu displaying.
        ///     Set to null for restore default ListView (AutocompleteListView).
        /// </summary>
        [Browsable(false)]
        [System.ComponentModel.DesignerSerializationVisibility(System.ComponentModel.DesignerSerializationVisibility.Hidden)]
        public IAutocompleteListView ListView
        {
            get => Host.ListView;
            set
            {
                if (ListView != null)
                {
                    var ctrl = value as Control;
                    value.ImageList = ImageList;
                    ctrl.RightToLeft = RightToLeft;
                    ctrl.Font = Font;
                    ctrl.MaximumSize = ctrl.MaximumSize;
                }
                Host.ListView = value;
                Host.ListView.ItemSelected += ListView_ItemSelected;
                Host.ListView.ItemHovered += ListView_ItemHovered;
            }
        }

        [DefaultValue(true)]
        public bool Enabled { get; set; }

        protected new bool DesignMode
        {
            get
            {
                if (base.DesignMode)
                    return true;

                return LicenseManager.UsageMode == LicenseUsageMode.Designtime;
            }
        }


        /// <summary>
        ///     Menu is visible
        /// </summary>
        public bool Visible
        {
            get { return Host != null && Host.Visible; }
        }


        private void SetupAutocomplete()
        {
            if (!DesignMode)
                Font = CustomFonts.GetMathFont(18);
            //GlobalConfig.mathFont;
            ImageList = null;
            TargetControlWrapper = null;
            //AllowsTabKey = true;
            //CaptureFocus = true;
            ToolTipDuration = 4000;
            MinFragmentLength = 1;
            ImageList = new ImageList {TransparentColor = Color.Transparent, ImageSize = Resources.Real.Size.DpiScale()};
            ImageList.Images.Add(Resources.Real);
            ImageList.Images.Add(Resources.Complex);
            ImageList.Images.Add(Resources.Natural);
            ImageList.Images.Add(Resources.Integer);
            ImageList.Images.Add(Resources.Rational);
            ImageList.Images.Add(Resources.Matrix);
            ImageList.Images.SetKeyName(0, "Real.png");
            ImageList.Images.SetKeyName(1, "Complex.png");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                UnsubscribeForm();
                _timer.Dispose();
                Host.Dispose();
            }
            base.Dispose(disposing);
        }

        private void ListView_ItemSelected(object sender, EventArgs e)
        {
            OnSelecting();
        }

        private void ListView_ItemHovered(object sender, HoveredEventArgs e)
        {
            OnHovered(e);
        }

        public void OnHovered(HoveredEventArgs e)
        {
            Hovered?.Invoke(this, e);
        }

        /// <summary>
        ///     Called when user selected the control and needed wrapper over it.
        ///     You can assign own Wrapper for target control.
        /// </summary>
        [Description(
            "Called when user selected the control and needed wrapper over it. You can assign own Wrapper for target control."
            )]
        public event EventHandler<WrapperNeededEventArgs> WrapperNeeded;

        protected void OnWrapperNeeded(WrapperNeededEventArgs args)
        {
            WrapperNeeded?.Invoke(this, args);
            if (args.Wrapper == null)
                args.Wrapper = TextBoxWrapper.Create(args.TargetControl);
        }

        private ITextBoxWrapper CreateWrapper(Control control)
        {
            if (WrapperByControls.ContainsKey(control))
                return WrapperByControls[control];

            var args = new WrapperNeededEventArgs(control);
            OnWrapperNeeded(args);
            if (args.Wrapper != null)
                WrapperByControls[control] = args.Wrapper;

            return args.Wrapper;
        }

        /// <summary>
        ///     Updates size of the menu
        /// </summary>
        public void Update()
        {
            Host.CalcSize();
        }

        /// <summary>
        ///     Returns rectangle of item
        /// </summary>
        public Rectangle GetItemRectangle(int itemIndex)
        {
            return Host.ListView.GetItemRectangle(itemIndex);
        }

        /// <summary>
        ///     User selects item
        /// </summary>
        [Description("Occurs when user selects item.")]
        public event EventHandler<SelectingEventArgs> Selecting;

        /// <summary>
        ///     It fires after item was inserting
        /// </summary>
        [Description("Occurs after user selected item.")]
        public event EventHandler<SelectedEventArgs> Selected;

        /// <summary>
        ///     It fires when item was hovered
        /// </summary>
        [Description("Occurs when user hovered item.")]
        public event EventHandler<HoveredEventArgs> Hovered;

        /// <summary>
        ///     Occurs when popup menu is opening
        /// </summary>
        public event EventHandler<CancelEventArgs> Opening;

        private void Timer_Tick(object sender, EventArgs e)
        {
            _timer.Stop();
            if (TargetControlWrapper != null)
                ShowAutocomplete(false);
        }

        private void SubscribeForm(ITextBoxWrapper wrapper)
        {
            var form = wrapper?.TargetControl?.FindForm();
            if (form == null) return;
            if (_myForm != null)
            {
                if (_myForm == form)
                    return;
                UnsubscribeForm();
            }

            _myForm = form;

            form.LocationChanged += Form_LocationChanged;
            form.ResizeBegin += Form_LocationChanged;
            form.FormClosing += Form_FormClosing;
            form.LostFocus += Form_LocationChanged;
        }

        private void UnsubscribeForm()
        {
            var form = _myForm;
            if (form == null) return;

            form.LocationChanged -= Form_LocationChanged;
            form.ResizeBegin -= Form_LocationChanged;
            form.FormClosing -= Form_FormClosing;
            form.LostFocus -= Form_LocationChanged;
            _myForm = null;
        }

        private void Form_FormClosing(object sender, FormClosingEventArgs e)
        {
            Close();
        }

        private void Form_LocationChanged(object sender, EventArgs e)
        {
            Close();
        }

        private void Control_MouseDown(object sender, MouseEventArgs e)
        {
            Close();
        }

        private ITextBoxWrapper FindWrapper(Control sender)
        {
            while (sender != null)
            {
                if (WrapperByControls.ContainsKey(sender))
                    return WrapperByControls[sender];

                sender = sender.Parent;
            }

            return null;
        }

        private void Control_KeyDown(object sender, KeyEventArgs e)
        {
            TargetControlWrapper = FindWrapper(sender as Control);
            // Editors can bind before their containing window has been created.
            SubscribeForm(TargetControlWrapper);

            var backspaceORdel = e.KeyCode == Keys.Back || e.KeyCode == Keys.Delete;


            if (Host.Visible)
            {
                if (ProcessKey((char) e.KeyCode, Control.ModifierKeys))
                    e.SuppressKeyPress = true;


                else if (!backspaceORdel)
                    ResetTimer(1);
                else
                    ResetTimer();

                return;
            }

            if (!Host.Visible)
            {
                switch (e.KeyCode)
                {
                    case Keys.Up:
                    case Keys.Down:
                    case Keys.PageUp:
                    case Keys.PageDown:
                    case Keys.Left:
                    case Keys.Right:
                    case Keys.End:
                    case Keys.Home:
                    case Keys.ControlKey:
                    {
                        _timer.Stop();
                        return;
                    }
                }

                if (Control.ModifierKeys == Keys.Control && e.KeyCode == Keys.Space)


                {
                    ShowAutocomplete(true);
                    e.SuppressKeyPress = true;
                    return;
                }
            }

            ResetTimer();
        }

        private void ResetTimer()
        {
            ResetTimer(-1);
        }

        private void ResetTimer(int interval)
        {
            if (interval <= 0)
                _timer.Interval = AppearInterval;
            else
                _timer.Interval = interval;
            _timer.Stop();
            _timer.Start();
        }

        private void Control_Scroll(object sender, ScrollEventArgs e)
        {
            Close();
        }

        private void Control_LostFocus(object sender, EventArgs e)
        {
#if __MonoCS__
            // Mono raises TextBox.LostFocus before the clicked dropdown child
            // receives its mouse event. Keep the pending click alive.
            if (Host.Visible && (Host.ContainsFocus || Host.Bounds.Contains(Control.MousePosition)))
                return;
#endif
            if (!Host.Focused) Close();
        }

        public AutocompleteMenu GetAutocompleteMenu(Control control)
        {
            if (AutocompleteMenuByControls.ContainsKey(control))
                return AutocompleteMenuByControls[control];
            return null;
        }

        internal void ShowAutocomplete(bool forced)
        {
            SubscribeForm(TargetControlWrapper);
            var target = TargetControlWrapper?.TargetControl;
            // A delayed timer can run inside a modal message loop after the
            // editor lost focus. It must not reopen a topmost popup/help window.
            if (target == null || target.IsDisposed || !target.Visible || !target.Enabled ||
                (!target.ContainsFocus && !Host.ContainsFocus))
            {
                Close();
                return;
            }

            if (forced)
                _forcedOpened = true;

            if (TargetControlWrapper != null && TargetControlWrapper.Readonly)
            {
                Close();
                return;
            }

            if (!Enabled)
            {
                Close();
                return;
            }

            if (!_forcedOpened && !AutoPopup)
            {
                Close();
                return;
            }

            //build list
            BuildAutocompleteList(_forcedOpened);

            //show popup menu
            if (VisibleItems.Count > 0)
            {
#if USE_TEXT_WIDTH
                MaximumSize = new Size(VisibleItems.Max(vi => vi.TextWidth), MaximumSize.Height);
#endif
                if (forced && VisibleItems.Count == 1 && Host.ListView.SelectedItemIndex == 0)
                {
                    //do autocomplete if menu contains only one line and user press CTRL-SPACE
                    OnSelecting();
                    Close();
                }
                else
                    ShowMenu();
            }
            else
                Close();
        }

        private void ShowMenu()
        {
            if (!Host.Visible)
            {
                var args = new CancelEventArgs();
                OnOpening(args);
                if (!args.Cancel)
                {
                    //calc screen point for popup menu
                    var point = TargetControlWrapper.TargetControl.Location;
                    point.Offset(2, TargetControlWrapper.TargetControl.Height + 2);
                    point = TargetControlWrapper.GetPositionFromCharIndex(Fragment.Start);
                    point.Offset(2, TargetControlWrapper.TargetControl.Font.Height + 2);
                    //
#if __MonoCS__
                    point = GetMonoMenuPosition(point);
#endif
                    Host.Show(TargetControlWrapper.TargetControl, point);
#if __MonoCS__
                    ShowMonoSelectedReference();
#endif
                    if (CaptureFocus)
                    {
                        (Host.ListView as Control).Focus();
                        //ProcessKey((char) Keys.Down, Keys.None);
                    }
                }
            }
            else
            {
#if __MonoCS__
                // The number of matches, and hence the popup size, can change
                // on every keystroke. Clamp it again after each reflow.
                var point = TargetControlWrapper.GetPositionFromCharIndex(Fragment.Start);
                point.Offset(2, TargetControlWrapper.TargetControl.Font.Height + 2);
                Host.Location = TargetControlWrapper.TargetControl.PointToScreen(GetMonoMenuPosition(point));
                ShowMonoSelectedReference();
#endif
                (Host.ListView as Control).Invalidate();
            }
        }
#if __MonoCS__
        private void SizeMonoList()
        {
            var list = (AutocompleteListView)Host.ListView;
            var area = Screen.FromControl(TargetControlWrapper.TargetControl).WorkingArea;
            var maximumWidth = Math.Max(1, Math.Min(MaximumSize.Width, area.Width - 8));
            var measuredWidth = new Size(240, 0).DpiScale().Width;
            // Measure a bounded sample; measuring thousands of suggestions
            // for every character would make the editor feel unresponsive.
            var count = Math.Min(VisibleItems.Count, 64);
            for (var i = 0; i < count; i++)
                measuredWidth = Math.Max(measuredWidth,
                    TextRenderer.MeasureText(VisibleItems[i].MenuText ?? "", list.Font,
                        new Size(4000, list.ItemHeight), TextFormatFlags.SingleLine | TextFormatFlags.NoPrefix).Width
                    + LeftPadding + SystemInformation.VerticalScrollBarWidth + 8);
            if (SelectedItemIndex >= count && SelectedItemIndex < VisibleItems.Count)
                measuredWidth = Math.Max(measuredWidth,
                    TextRenderer.MeasureText(VisibleItems[SelectedItemIndex].MenuText ?? "", list.Font,
                        new Size(4000, list.ItemHeight), TextFormatFlags.SingleLine | TextFormatFlags.NoPrefix).Width
                    + LeftPadding + SystemInformation.VerticalScrollBarWidth + 8);
            var height = Math.Min(MaximumSize.Height, Math.Max(list.ItemHeight + 1,
                VisibleItems.Count * list.ItemHeight + 1));
            list.Size = new Size(Math.Min(maximumWidth, measuredWidth), height);
            list.AutoScrollMinSize = new Size(0, VisibleItems.Count * list.ItemHeight + 1);
        }

        private Point GetMonoMenuPosition(Point requestedClientPoint)
        {
            var control = TargetControlWrapper.TargetControl;
            var caret = control.PointToScreen(requestedClientPoint);
            var area = Screen.FromPoint(caret).WorkingArea;
            var x = Math.Max(area.Left + 2, Math.Min(caret.X, area.Right - Host.Width - 2));
            var above = caret.Y - control.Font.Height - Host.Height - 4;
            var y = caret.Y + Host.Height <= area.Bottom - 2
                ? caret.Y
                : above >= area.Top + 2 ? above : area.Bottom - Host.Height - 2;
            y = Math.Max(area.Top + 2, Math.Min(y, area.Bottom - Host.Height - 2));
            return control.PointToClient(new Point(x, y));
        }

        private void ShowMonoSelectedReference()
        {
            if (SelectedItemIndex < 0 || SelectedItemIndex >= VisibleItems.Count || !Host.Visible)
                return;
            ((AutocompleteListView)Host.ListView).ShowToolTip(VisibleItems[SelectedItemIndex]);
        }
#endif
        private readonly ISharedViewState _sharedViewState;

        private void BuildAutocompleteList(bool forced)
        {
            var visibleItems = new List<AutocompleteItem>();

            var foundSelected = false;
            var selectedIndex = -1;
            //get fragment around caret
            var fragment = GetFragment(SearchPattern);
            var text = fragment.Text;

            var index = text.IndexOfAny(SpecialSymbols.SuperscriptsWithoutSpace.ToCharArray());

            if (_sharedViewState.IsExponent && index == -1)
            {
                text = "";
            }
            //  else if (IsExponent)
            //    text = text.Substring(index);

            //
            if (_sourceItems != null)
                if (forced || (text.Length >= MinFragmentLength /* && tb.Selection.Start == tb.Selection.End*/))
                {
                    Fragment = fragment;
                    //build popup menu
                    foreach (var item in _sourceItems)
                    {
                       // item.Parent = this;
                        var res = item.Compare(text);
                        if (res != CompareResult.Hidden)
                            visibleItems.Add(item);
                        if (res == CompareResult.VisibleAndSelected && !foundSelected)
                        {
                            foundSelected = true;
                            selectedIndex = visibleItems.Count - 1;
                        }
                    }
                }

            VisibleItems = visibleItems;

            if (foundSelected)
                SelectedItemIndex = selectedIndex;
            else
                SelectedItemIndex = 0;

            Host.ListView.HighlightedItemIndex = -1;
#if __MonoCS__
            SizeMonoList();
#endif
            Host.CalcSize();
        }

        internal void OnOpening(CancelEventArgs args)
        {
            Opening?.Invoke(this, args);
        }

        private Range GetFragment(string searchPattern)
        {
            var tb = TargetControlWrapper;

            if (tb.SelectionLength > 0) return new Range(tb);

            var text = tb.Text;
            var regex = new Regex(searchPattern);
            var result = new Range(tb);

            var startPos = tb.SelectionStart;
            //go forward
            var i = startPos;
            while (i >= 0 && i < text.Length)
            {
                if (!regex.IsMatch(text[i].ToString()))
                    break;
                i++;
            }
            result.End = i;

            //go backward
            i = startPos;
            while (i > 0 && i - 1 < text.Length)
            {
                if (!regex.IsMatch(text[i - 1].ToString()))
                    break;
                i--;
            }
            result.Start = i;

            return result;
        }

        public void Close()
        {
            _timer.Stop();
            (Host.ListView as AutocompleteListView).CloseToolTip();
            Host.Close();
            _forcedOpened = false;
        }

        public void SetAutocompleteItems(IEnumerable<string> items)
        {
            var list = new List<AutocompleteItem>();
            if (items == null)
            {
                _sourceItems = null;
                return;
            }
            foreach (var item in items)
                list.Add(new AutocompleteItem(item));
            SetAutocompleteItems(list);
        }

        public void SetAutocompleteItems(IEnumerable<AutocompleteItem> items)
        {
            _sourceItems = items;
        }

        public void AddItem(string item)
        {
            AddItem(new AutocompleteItem(item));
        }

        public void AddItem(AutocompleteItem item)
        {
            if (_sourceItems == null)
                _sourceItems = new List<AutocompleteItem>();

            if (_sourceItems is IList)
                (_sourceItems as IList).Add(item);
            else
                throw new Exception("Current autocomplete items does not support adding");
        }

        /// <summary>
        ///     Shows popup menu immediately
        /// </summary>
        /// <param name="forced">If True - MinFragmentLength will be ignored</param>
        public void Show(Control control, bool forced)
        {
            SetAutocompleteMenu(control, this);
            TargetControlWrapper = FindWrapper(control);
            ShowAutocomplete(forced);
        }

        internal virtual void OnSelecting()
        {
            if (SelectedItemIndex < 0 || SelectedItemIndex >= VisibleItems.Count)
                return;


            var item = VisibleItems[SelectedItemIndex];
            var args = new SelectingEventArgs
            {
                Item = item,
                SelectedIndex = SelectedItemIndex
            };

            OnSelecting(args);

            if (args.Cancel)
            {
                SelectedItemIndex = args.SelectedIndex;
                (Host.ListView as Control).Invalidate(true);
                return;
            }

            if (!args.Handled)
            {
                var fragment = Fragment;
                ApplyAutocomplete(item, fragment);
            }

            Close();
            //
            var args2 = new SelectedEventArgs
            {
                Item = item,
                Control = TargetControlWrapper.TargetControl
            };
            item.OnSelected(args2);
            OnSelected(args2);
        }

        private void ApplyAutocomplete(AutocompleteItem item, Range fragment)
        {
            var newText = item.GetTextForReplace();
            //replace text of fragment


            var index = fragment.Text.IndexOfAny(SpecialSymbols.SuperscriptsWithoutSpace.ToCharArray());

            if (index == 0)
            {
                newText = SpecialSymbols.AsciiToSuperscript(newText);
            }
            else if (index > 0)
            {
                newText = fragment.Text.Substring(0, index) + SpecialSymbols.AsciiToSuperscript(newText);
            }
            else if (_sharedViewState.IsExponent)
                newText = fragment.Text + SpecialSymbols.AsciiToSuperscript(newText);


            fragment.Text = newText;
            fragment.TargetWrapper.TargetControl.Focus();
        }

        internal void OnSelecting(SelectingEventArgs args)
        {
            Selecting?.Invoke(this, args);
        }

        public void OnSelected(SelectedEventArgs args)
        {
            Selected?.Invoke(this, args);
        }

        public void SelectNext(int shift)
        {
            SelectedItemIndex = Math.Max(0, Math.Min(SelectedItemIndex + shift, VisibleItems.Count - 1));
            //
            (Host.ListView as Control).Invalidate();
        }

        public bool ProcessKey(char c, Keys keyModifiers)
        {
#if __MonoCS__
            var list = (AutocompleteListView)Host.ListView;
            var page = Math.Max(1, list.ClientSize.Height / Math.Max(1, list.ItemHeight));
#else
            var page = Host.Height/(Font.Height + 4);
#endif
            if (keyModifiers == Keys.None)
                switch ((Keys) c)
                {
                    case Keys.Down:
                        SelectNext(+1);
                        return true;
                    case Keys.PageDown:
                        SelectNext(+page);
                        return true;
                    case Keys.Up:
                        SelectNext(-1);
                        return true;
                    case Keys.PageUp:
                        SelectNext(-page);
                        return true;
                    case Keys.Enter:
                        OnSelecting();
                        return true;
                    case Keys.Tab:
                        if (!AllowsTabKey)
                            break;
                        OnSelecting();
                        return true;
                    case Keys.Left:
                    case Keys.Right:
                        Close();
                        return false;
                    case Keys.Escape:
                        Close();
                        return true;
                }

            return false;
        }

        #region IExtenderProvider Members

        bool IExtenderProvider.CanExtend(object extendee)
        {
            //find  AutocompleteMenu with lowest hashcode
            if (Container != null)
                foreach (var comp in Container.Components)
                    if (comp is AutocompleteMenu)
                        if (comp.GetHashCode() < GetHashCode())
                            return false;
            //we are main autocomplete menu on form ...
            //check extendee as TextBox
            if (!(extendee is Control))
                return false;
            var temp = TextBoxWrapper.Create(extendee as Control);
            return temp != null;
        }

        public void SetAutocompleteMenu(Control control, AutocompleteMenu menu)
        {
            if (menu != null)
            {
                var wrapper = menu.CreateWrapper(control);
                if (wrapper == null) return;
                //
                menu.SubscribeForm(wrapper);
                AutocompleteMenuByControls[control] = this;
                //
                wrapper.LostFocus += menu.Control_LostFocus;
                wrapper.Scroll += menu.Control_Scroll;
                wrapper.KeyDown += menu.Control_KeyDown;
                wrapper.MouseDown += menu.Control_MouseDown;
            }
            else
            {
                AutocompleteMenuByControls.TryGetValue(control, out menu);
                AutocompleteMenuByControls.Remove(control);
                WrapperByControls.TryGetValue(control, out ITextBoxWrapper wrapper);
                WrapperByControls.Remove(control);
                if (wrapper != null && menu != null)
                {
                    wrapper.LostFocus -= menu.Control_LostFocus;
                    wrapper.Scroll -= menu.Control_Scroll;
                    wrapper.KeyDown -= menu.Control_KeyDown;
                    wrapper.MouseDown -= menu.Control_MouseDown;
                }
            }
        }

        /*   
         public void SetAutocompleteMenu(Control control, AutocompleteMenu menu)
        {

            if (menu != null)
            {
                if (WrapperByControls.ContainsKey(control))
                    return;
                var wrapper = menu.CreateWrapper(control);
                if (wrapper == null) return;
                //
                if (control.IsHandleCreated)
                    menu.SubscribeForm(wrapper);
                else
                    control.HandleCreated += (o, e) => menu.SubscribeForm(wrapper);
                //
                AutocompleteMenuByControls[control] = this;
                //
                wrapper.LostFocus += menu.control_LostFocus;
                wrapper.Scroll += menu.control_Scroll;
                wrapper.KeyDown += menu.control_KeyDown;
                wrapper.MouseDown += menu.control_MouseDown;
            }
            else
            {
                AutocompleteMenuByControls.TryGetValue(control, out menu);
                AutocompleteMenuByControls.Remove(control);
                ITextBoxWrapper wrapper = null;
                WrapperByControls.TryGetValue(control, out wrapper);
                WrapperByControls.Remove(control);
                if (wrapper != null && menu != null)
                {
                    wrapper.LostFocus -= menu.control_LostFocus;
                    wrapper.Scroll -= menu.control_Scroll;
                    wrapper.KeyDown -= menu.control_KeyDown;
                    wrapper.MouseDown -= menu.control_MouseDown;
                }
            }
        }
        */

        #endregion
    }
}
