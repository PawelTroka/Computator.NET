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
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using Computator.NET.Config;
using Computator.NET.DataTypes;
using Computator.NET.Properties;
using Computator.NET.UI.CodeEditors;
using Computator.NET.UI.Controls;

namespace AutocompleteMenuNS
{
    [ProvideProperty("AutocompleteMenu", typeof (Control))]
    public class AutocompleteMenu : Component, IExtenderProvider
    {
        private static readonly Dictionary<Control, AutocompleteMenu>
            AutocompleteMenuByControls =
                new Dictionary<Control, AutocompleteMenu>();

        private static readonly Dictionary<Control, ITextBoxWrapper>
            WrapperByControls =
                new Dictionary<Control, ITextBoxWrapper>();

        private readonly Timer timer = new Timer();
        private bool forcedOpened;
        private Size maximumSize;
        private Form myForm;

        private IEnumerable<AutocompleteItem> sourceItems =
            new List<AutocompleteItem>();

        private ITextBoxWrapper targetControlWrapper;

        public AutocompleteMenu()
        {
            Host = new AutocompleteMenuHost(this);
            Host.ListView.ItemSelected += ListView_ItemSelected;
            Host.ListView.ItemHovered += ListView_ItemHovered;
            VisibleItems = new List<AutocompleteItem>();
            Enabled = true;
            AppearInterval = 500;
            timer.Tick += timer_Tick;
            MaximumSize = new Size(2000, 200);
            AutoPopup = true;

            SearchPattern = @"[\w\.]";
            MinFragmentLength = 2;

            setupAutocomplete();

        }

        [Browsable(false)]
        public IList<AutocompleteItem> VisibleItems
        {
            get { return Host.ListView.VisibleItems; }
            private set { Host.ListView.VisibleItems = value; }
        }

        /// <summary>
        ///     Duration (ms) of tooltip showing
        /// </summary>
        [Description("Duration (ms) of tooltip showing")]
        [DefaultValue(3000)]
        public int ToolTipDuration
        {
            get { return Host.ListView.ToolTipDuration; }
            set { Host.ListView.ToolTipDuration = value; }
        }

        [Browsable(false)]
        public int SelectedItemIndex
        {
            get { return Host.ListView.SelectedItemIndex; }
            internal set
            {
                Host.ListView.SelectedItemIndex = value;


            }
        }

        internal AutocompleteMenuHost Host { get; set; }

        /// <summary>
        ///     Current target control wrapper
        /// </summary>
        [Browsable(false)]
        public ITextBoxWrapper TargetControlWrapper
        {
            get { return targetControlWrapper; }
            set
            {
                targetControlWrapper = value;
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
        [DefaultValue(typeof (Size), "180, 200")]
        [Description("Maximum size of popup menu")]
        public Size MaximumSize
        {
            get { return maximumSize; }
            set
            {
                maximumSize = value;
                (Host.ListView as Control).MaximumSize = maximumSize;
                (Host.ListView as Control).Size = maximumSize;
                Host.CalcSize();
            }
        }

        /// <summary>
        ///     Font
        /// </summary>
        public Font Font
        {
            get { return (Host.ListView as Control).Font; }
            set { (Host.ListView as Control).Font = value; }
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
        /// Colors of foreground and background
        /// </summary>
        [Browsable(true)]
        [Description("Colors of foreground and background.")]
        [TypeConverter(typeof(ExpandableObjectConverter))]
        public Colors Colors
        {
            get { return (Host.ListView as IAutocompleteListView).Colors; }
            set { (Host.ListView as IAutocompleteListView).Colors = value; }
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
        [DefaultValue(typeof (RightToLeft), "No")]
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
        public ImageList ImageList
        {
            get { return Host.ListView.ImageList; }
            set { Host.ListView.ImageList = value; }
        }

        /// <summary>
        ///     Fragment
        /// </summary>
        [Browsable(false)]
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
                if (sourceItems == null)
                    return null;
                var list = new List<string>();
                foreach (var item in sourceItems)
                    list.Add(item.ToString());
                return list.ToArray();
            }
            set { SetAutocompleteItems(value); }
        }

        /// <summary>
        ///     The control for menu displaying.
        ///     Set to null for restore default ListView (AutocompleteListView).
        /// </summary>
        [Browsable(false)]
        public IAutocompleteListView ListView
        {
            get { return Host.ListView; }
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

        private void setupAutocomplete()
        {
            Font = CustomFonts.GetMathFont(18);//new Font("Cambria", 18.0F, GraphicsUnit.Point);
            //GlobalConfig.mathFont;
            ImageList = null;
            TargetControlWrapper = null;
            //AllowsTabKey = true;
            //CaptureFocus = true;
            ToolTipDuration = 4000;
            MinFragmentLength = 1;
            ImageList = new ImageList {TransparentColor = Color.Transparent};
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
                timer.Dispose();
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


            if (Hovered != null)
                Hovered(this, e);
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
            if (WrapperNeeded != null)
                WrapperNeeded(this, args);
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

        private void timer_Tick(object sender, EventArgs e)
        {
            timer.Stop();
            if (TargetControlWrapper != null)
                ShowAutocomplete(false);
        }

        private void SubscribeForm(ITextBoxWrapper wrapper)
        {
            if (wrapper == null) return;
            var form = wrapper.TargetControl.FindForm();
            if (form == null) return;
            if (myForm != null)
            {
                if (myForm == form)
                    return;
                UnsubscribeForm(wrapper);
            }

            myForm = form;

            form.LocationChanged += form_LocationChanged;
            form.ResizeBegin += form_LocationChanged;
            form.FormClosing += form_FormClosing;
            form.LostFocus += form_LocationChanged;
        }

        private void UnsubscribeForm(ITextBoxWrapper wrapper)
        {
            if (wrapper == null) return;
            var form = wrapper.TargetControl.FindForm();
            if (form == null) return;

            form.LocationChanged -= form_LocationChanged;
            form.ResizeBegin -= form_LocationChanged;
            form.FormClosing -= form_FormClosing;
            form.LostFocus -= form_LocationChanged;
        }

        private void form_FormClosing(object sender, FormClosingEventArgs e)
        {
            Close();
        }

        private void form_LocationChanged(object sender, EventArgs e)
        {
            Close();
        }

        private void control_MouseDown(object sender, MouseEventArgs e)
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

        private void control_KeyDown(object sender, KeyEventArgs e)
        {
            TargetControlWrapper = FindWrapper(sender as Control);

            bool backspaceORdel = e.KeyCode == Keys.Back || e.KeyCode == Keys.Delete;




            if (Host.Visible)
            {
                if (ProcessKey((char)e.KeyCode, Control.ModifierKeys))
                    e.SuppressKeyPress = true;


                else

                    if (!backspaceORdel)
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
                            timer.Stop();
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
                timer.Interval = AppearInterval;
            else
                timer.Interval = interval;
            timer.Stop();
            timer.Start();
        }

        private void control_Scroll(object sender, ScrollEventArgs e)
        {
            Close();
        }

        private void control_LostFocus(object sender, EventArgs e)
        {
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
            if (forced)
                forcedOpened = true;

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

            if (!forcedOpened && !AutoPopup)
            {
                Close();
                return;
            }

            //build list
            BuildAutocompleteList(forcedOpened);

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
                    Host.Show(TargetControlWrapper.TargetControl, point);
                    if (CaptureFocus)
                    {
                        (Host.ListView as Control).Focus();
                        //ProcessKey((char) Keys.Down, Keys.None);
                    }
                }
            }
            else
                (Host.ListView as Control).Invalidate();
        }

        private void BuildAutocompleteList(bool forced)
        {
            var visibleItems = new List<AutocompleteItem>();

            var foundSelected = false;
            var selectedIndex = -1;
            //get fragment around caret
            var fragment = GetFragment(SearchPattern);
            var text = fragment.Text;
            //
            if (sourceItems != null)
                if (forced || (text.Length >= MinFragmentLength /* && tb.Selection.Start == tb.Selection.End*/))
                {
                    Fragment = fragment;
                    //build popup menu
                    foreach (var item in sourceItems)
                    {
                        item.Parent = this;
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
            Host.CalcSize();
        }

        internal void OnOpening(CancelEventArgs args)
        {
            if (Opening != null)
                Opening(this, args);
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
            while (i > 0 && (i - 1) < text.Length)
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
            (Host.ListView as AutocompleteListView).closeToolTip();
            Host.Close();
            forcedOpened = false;
        }

        public void SetAutocompleteItems(IEnumerable<string> items)
        {
            var list = new List<AutocompleteItem>();
            if (items == null)
            {
                sourceItems = null;
                return;
            }
            foreach (var item in items)
                list.Add(new AutocompleteItem(item));
            SetAutocompleteItems(list);
        }

        public void SetAutocompleteItems(IEnumerable<AutocompleteItem> items)
        {
            sourceItems = items;
        }

        public void AddItem(string item)
        {
            AddItem(new AutocompleteItem(item));
        }

        public void AddItem(AutocompleteItem item)
        {
            if (sourceItems == null)
                sourceItems = new List<AutocompleteItem>();

            if (sourceItems is IList)
                (sourceItems as IList).Add(item);
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

            var expressionTextBox = (TargetControlWrapper.TargetControl as ExpressionTextBox);
            var scintillaEditor =
                (TargetControlWrapper.TargetControl as ScintillaCodeEditorControl);

            var isExponent = false;

            if (expressionTextBox != null)
                isExponent = expressionTextBox.ExponentMode;
            else if (scintillaEditor != null)
                isExponent = scintillaEditor.ExponentMode;


            fragment.Text = isExponent ? SpecialSymbols.AsciiToSuperscript(newText) : newText;
            fragment.TargetWrapper.TargetControl.Focus();
        }

        internal void OnSelecting(SelectingEventArgs args)
        {
            if (Selecting != null)
                Selecting(this, args);
        }

        public void OnSelected(SelectedEventArgs args)
        {
            if (Selected != null)
                Selected(this, args);
        }

        public void SelectNext(int shift)
        {
            SelectedItemIndex = Math.Max(0, Math.Min(SelectedItemIndex + shift, VisibleItems.Count - 1));
            //
            (Host.ListView as Control).Invalidate();
        }

        public bool ProcessKey(char c, Keys keyModifiers)
        {
            var page = Host.Height/(Font.Height + 4);
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

        /// <summary>
        /// Menu is visible
        /// </summary>
        public bool Visible
        {
            get { return Host != null && Host.Visible; }
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