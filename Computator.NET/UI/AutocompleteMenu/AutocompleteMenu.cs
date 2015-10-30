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

namespace AutocompleteMenuNS
{
    [System.ComponentModel.ProvideProperty("AutocompleteMenu", typeof (System.Windows.Forms.Control))]
    public class AutocompleteMenu : System.ComponentModel.Component, System.ComponentModel.IExtenderProvider
    {
        private static readonly System.Collections.Generic.Dictionary<System.Windows.Forms.Control, AutocompleteMenu>
            AutocompleteMenuByControls =
                new System.Collections.Generic.Dictionary<System.Windows.Forms.Control, AutocompleteMenu>();

        private static readonly System.Collections.Generic.Dictionary<System.Windows.Forms.Control, ITextBoxWrapper>
            WrapperByControls =
                new System.Collections.Generic.Dictionary<System.Windows.Forms.Control, ITextBoxWrapper>();

        private readonly System.Windows.Forms.Timer timer = new System.Windows.Forms.Timer();
        private bool forcedOpened;
        private System.Drawing.Size maximumSize;
        private System.Windows.Forms.Form myForm;

        private System.Collections.Generic.IEnumerable<AutocompleteItem> sourceItems =
            new System.Collections.Generic.List<AutocompleteItem>();

        private ITextBoxWrapper targetControlWrapper;

        public AutocompleteMenu()
        {
            Host = new AutocompleteMenuHost(this);
            Host.ListView.ItemSelected += ListView_ItemSelected;
            Host.ListView.ItemHovered += ListView_ItemHovered;
            VisibleItems = new System.Collections.Generic.List<AutocompleteItem>();
            Enabled = true;
            AppearInterval = 500;
            timer.Tick += timer_Tick;
            MaximumSize = new System.Drawing.Size(180, 200);
            AutoPopup = true;

            SearchPattern = @"[\w\.]";
            MinFragmentLength = 2;

            setupAutocomplete();
        }

        [System.ComponentModel.Browsable(false)]
        public System.Collections.Generic.IList<AutocompleteItem> VisibleItems
        {
            get { return Host.ListView.VisibleItems; }
            private set { Host.ListView.VisibleItems = value; }
        }

        /// <summary>
        ///     Duration (ms) of tooltip showing
        /// </summary>
        [System.ComponentModel.Description("Duration (ms) of tooltip showing")]
        [System.ComponentModel.DefaultValue(3000)]
        public int ToolTipDuration
        {
            get { return Host.ListView.ToolTipDuration; }
            set { Host.ListView.ToolTipDuration = value; }
        }

        [System.ComponentModel.Browsable(false)]
        public int SelectedItemIndex
        {
            get { return Host.ListView.SelectedItemIndex; }
            internal set { Host.ListView.SelectedItemIndex = value; }
        }

        internal AutocompleteMenuHost Host { get; set; }

        /// <summary>
        ///     Current target control wrapper
        /// </summary>
        [System.ComponentModel.Browsable(false)]
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
        [System.ComponentModel.DefaultValue(typeof (System.Drawing.Size), "180, 200")]
        [System.ComponentModel.Description("Maximum size of popup menu")]
        public System.Drawing.Size MaximumSize
        {
            get { return maximumSize; }
            set
            {
                maximumSize = value;
                (Host.ListView as System.Windows.Forms.Control).MaximumSize = maximumSize;
                (Host.ListView as System.Windows.Forms.Control).Size = maximumSize;
                Host.CalcSize();
            }
        }

        /// <summary>
        ///     Font
        /// </summary>
        public System.Drawing.Font Font
        {
            get { return (Host.ListView as System.Windows.Forms.Control).Font; }
            set { (Host.ListView as System.Windows.Forms.Control).Font = value; }
        }

        /// <summary>
        ///     Left padding of text
        /// </summary>
        [System.ComponentModel.DefaultValue(18)]
        [System.ComponentModel.Description("Left padding of text")]
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
        ///     AutocompleteMenu will popup automatically (when user writes text). Otherwise it will popup only programmatically or
        ///     by Ctrl-Space.
        /// </summary>
        [System.ComponentModel.DefaultValue(true)]
        [System.ComponentModel.Description(
            "AutocompleteMenu will popup automatically (when user writes text). Otherwise it will popup only programmatically or by Ctrl-Space."
            )]
        public bool AutoPopup { get; set; }

        /// <summary>
        ///     AutocompleteMenu will capture focus when opening.
        /// </summary>
        [System.ComponentModel.DefaultValue(false)]
        [System.ComponentModel.Description("AutocompleteMenu will capture focus when opening.")]
        public bool CaptureFocus { get; set; }

        /// <summary>
        ///     Indicates whether the component should draw right-to-left for RTL languages.
        /// </summary>
        [System.ComponentModel.DefaultValue(typeof (System.Windows.Forms.RightToLeft), "No")]
        [System.ComponentModel.Description(
            "Indicates whether the component should draw right-to-left for RTL languages.")]
        public System.Windows.Forms.RightToLeft RightToLeft
        {
            get { return Host.RightToLeft; }
            set { Host.RightToLeft = value; }
        }

        /// <summary>
        ///     Image list
        /// </summary>
        public System.Windows.Forms.ImageList ImageList
        {
            get { return Host.ListView.ImageList; }
            set { Host.ListView.ImageList = value; }
        }

        /// <summary>
        ///     Fragment
        /// </summary>
        [System.ComponentModel.Browsable(false)]
        public Range Fragment { get; internal set; }

        /// <summary>
        ///     Regex pattern for serach fragment around caret
        /// </summary>
        [System.ComponentModel.Description("Regex pattern for serach fragment around caret")]
        [System.ComponentModel.DefaultValue(@"[\w\.]")]
        public string SearchPattern { get; set; }

        /// <summary>
        ///     Minimum fragment length for popup
        /// </summary>
        [System.ComponentModel.Description("Minimum fragment length for popup")]
        [System.ComponentModel.DefaultValue(2)]
        public int MinFragmentLength { get; set; }

        /// <summary>
        ///     Allows TAB for select menu item
        /// </summary>
        [System.ComponentModel.Description("Allows TAB for select menu item")]
        [System.ComponentModel.DefaultValue(false)]
        public bool AllowsTabKey { get; set; }

        /// <summary>
        ///     Interval of menu appear (ms)
        /// </summary>
        [System.ComponentModel.Description("Interval of menu appear (ms)")]
        [System.ComponentModel.DefaultValue(500)]
        public int AppearInterval { get; set; }

        [System.ComponentModel.DefaultValue(null)]
        public string[] Items
        {
            get
            {
                if (sourceItems == null)
                    return null;
                var list = new System.Collections.Generic.List<string>();
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
        [System.ComponentModel.Browsable(false)]
        public IAutocompleteListView ListView
        {
            get { return Host.ListView; }
            set
            {
                if (ListView != null)
                {
                    var ctrl = value as System.Windows.Forms.Control;
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

        [System.ComponentModel.DefaultValue(true)]
        public bool Enabled { get; set; }

        private void setupAutocomplete()
        {
            Font = new System.Drawing.Font("Cambria", 18.0F, System.Drawing.GraphicsUnit.Point);
                //GlobalConfig.mathFont;
            ImageList = null;
            TargetControlWrapper = null;
            AllowsTabKey = true;
            ToolTipDuration = 4000;
            MinFragmentLength = 1;
            ImageList = new System.Windows.Forms.ImageList();
            ImageList.TransparentColor = System.Drawing.Color.Transparent;
            ImageList.Images.Add(Computator.NET.Properties.Resources.Real);
            ImageList.Images.Add(Computator.NET.Properties.Resources.Complex);
            ImageList.Images.Add(Computator.NET.Properties.Resources.Natural);
            ImageList.Images.Add(Computator.NET.Properties.Resources.Integer);
            ImageList.Images.Add(Computator.NET.Properties.Resources.Rational);
            ImageList.Images.Add(Computator.NET.Properties.Resources.Matrix);
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

        private void ListView_ItemSelected(object sender, System.EventArgs e)
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
        [System.ComponentModel.Description(
            "Called when user selected the control and needed wrapper over it. You can assign own Wrapper for target control."
            )]
        public event System.EventHandler<WrapperNeededEventArgs> WrapperNeeded;

        protected void OnWrapperNeeded(WrapperNeededEventArgs args)
        {
            if (WrapperNeeded != null)
                WrapperNeeded(this, args);
            if (args.Wrapper == null)
                args.Wrapper = TextBoxWrapper.Create(args.TargetControl);
        }

        private ITextBoxWrapper CreateWrapper(System.Windows.Forms.Control control)
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
        public System.Drawing.Rectangle GetItemRectangle(int itemIndex)
        {
            return Host.ListView.GetItemRectangle(itemIndex);
        }

        /// <summary>
        ///     User selects item
        /// </summary>
        [System.ComponentModel.Description("Occurs when user selects item.")]
        public event System.EventHandler<SelectingEventArgs> Selecting;

        /// <summary>
        ///     It fires after item was inserting
        /// </summary>
        [System.ComponentModel.Description("Occurs after user selected item.")]
        public event System.EventHandler<SelectedEventArgs> Selected;

        /// <summary>
        ///     It fires when item was hovered
        /// </summary>
        [System.ComponentModel.Description("Occurs when user hovered item.")]
        public event System.EventHandler<HoveredEventArgs> Hovered;

        /// <summary>
        ///     Occurs when popup menu is opening
        /// </summary>
        public event System.EventHandler<System.ComponentModel.CancelEventArgs> Opening;

        private void timer_Tick(object sender, System.EventArgs e)
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

        private void form_FormClosing(object sender, System.Windows.Forms.FormClosingEventArgs e)
        {
            Close();
        }

        private void form_LocationChanged(object sender, System.EventArgs e)
        {
            Close();
        }

        private void control_MouseDown(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            Close();
        }

        private ITextBoxWrapper FindWrapper(System.Windows.Forms.Control sender)
        {
            while (sender != null)
            {
                if (WrapperByControls.ContainsKey(sender))
                    return WrapperByControls[sender];

                sender = sender.Parent;
            }

            return null;
        }

        private void control_KeyDown(object sender, System.Windows.Forms.KeyEventArgs e)
        {
            TargetControlWrapper = FindWrapper(sender as System.Windows.Forms.Control);

            var backspaceORdel = e.KeyCode == System.Windows.Forms.Keys.Back ||
                                 e.KeyCode == System.Windows.Forms.Keys.Delete;

            if (Host.Visible)
            {
                if (ProcessKey((char) e.KeyCode, System.Windows.Forms.Control.ModifierKeys))
                    e.SuppressKeyPress = true;
                else if (!backspaceORdel)
                    ResetTimer(1);
                else
                    ResetTimer();

                return;
            }

            if (!Host.Visible)
                if (System.Windows.Forms.Control.ModifierKeys == System.Windows.Forms.Keys.Control &&
                    e.KeyCode == System.Windows.Forms.Keys.Space)
                {
                    ShowAutocomplete(true);
                    e.SuppressKeyPress = true;
                    return;
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

        private void control_Scroll(object sender, System.Windows.Forms.ScrollEventArgs e)
        {
            Close();
        }

        private void control_LostFocus(object sender, System.EventArgs e)
        {
            if (!Host.Focused) Close();
        }

        public AutocompleteMenu GetAutocompleteMenu(System.Windows.Forms.Control control)
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
                var args = new System.ComponentModel.CancelEventArgs();
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
                        (Host.ListView as System.Windows.Forms.Control).Focus();
                        //ProcessKey((char) Keys.Down, Keys.None);
                    }
                }
            }
            else
                (Host.ListView as System.Windows.Forms.Control).Invalidate();
        }

        private void BuildAutocompleteList(bool forced)
        {
            var visibleItems = new System.Collections.Generic.List<AutocompleteItem>();

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

            Host.CalcSize();
        }

        internal void OnOpening(System.ComponentModel.CancelEventArgs args)
        {
            if (Opening != null)
                Opening(this, args);
        }

        private Range GetFragment(string searchPattern)
        {
            var tb = TargetControlWrapper;

            if (tb.SelectionLength > 0) return new Range(tb);

            var text = tb.Text;
            var regex = new System.Text.RegularExpressions.Regex(searchPattern);
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

        public void SetAutocompleteItems(System.Collections.Generic.IEnumerable<string> items)
        {
            var list = new System.Collections.Generic.List<AutocompleteItem>();
            if (items == null)
            {
                sourceItems = null;
                return;
            }
            foreach (var item in items)
                list.Add(new AutocompleteItem(item));
            SetAutocompleteItems(list);
        }

        public void SetAutocompleteItems(System.Collections.Generic.IEnumerable<AutocompleteItem> items)
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
                sourceItems = new System.Collections.Generic.List<AutocompleteItem>();

            if (sourceItems is System.Collections.IList)
                (sourceItems as System.Collections.IList).Add(item);
            else
                throw new System.Exception("Current autocomplete items does not support adding");
        }

        /// <summary>
        ///     Shows popup menu immediately
        /// </summary>
        /// <param name="forced">If True - MinFragmentLength will be ignored</param>
        public void Show(System.Windows.Forms.Control control, bool forced)
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
                (Host.ListView as System.Windows.Forms.Control).Invalidate(true);
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

            var expressionTextBox = (TargetControlWrapper.TargetControl as Computator.NET.UI.Controls.ExpressionTextBox);
            var scintillaEditor =
                (TargetControlWrapper.TargetControl as Computator.NET.UI.CodeEditors.ScintillaCodeEditorControl);

            var isExponent = false;

            if (expressionTextBox != null)
                isExponent = expressionTextBox.ExponentMode;
            else if (scintillaEditor != null)
                isExponent = scintillaEditor.ExponentMode;


            fragment.Text = isExponent ? Computator.NET.DataTypes.SpecialSymbols.AsciiToSuperscript(newText) : newText;
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
            SelectedItemIndex = System.Math.Max(0, System.Math.Min(SelectedItemIndex + shift, VisibleItems.Count - 1));
            //
            (Host.ListView as System.Windows.Forms.Control).Invalidate();
        }

        public bool ProcessKey(char c, System.Windows.Forms.Keys keyModifiers)
        {
            var page = Host.Height/(Font.Height + 4);
            if (keyModifiers == System.Windows.Forms.Keys.None)
                switch ((System.Windows.Forms.Keys) c)
                {
                    case System.Windows.Forms.Keys.Down:
                        SelectNext(+1);
                        return true;
                    case System.Windows.Forms.Keys.PageDown:
                        SelectNext(+page);
                        return true;
                    case System.Windows.Forms.Keys.Up:
                        SelectNext(-1);
                        return true;
                    case System.Windows.Forms.Keys.PageUp:
                        SelectNext(-page);
                        return true;
                    case System.Windows.Forms.Keys.Enter:
                        OnSelecting();
                        return true;
                    case System.Windows.Forms.Keys.Tab:
                        if (!AllowsTabKey)
                            break;
                        OnSelecting();
                        return true;
                    case System.Windows.Forms.Keys.Left:
                    case System.Windows.Forms.Keys.Right:
                        Close();
                        return false;
                    case System.Windows.Forms.Keys.Escape:
                        Close();
                        return true;
                }

            return false;
        }

        #region IExtenderProvider Members

        bool System.ComponentModel.IExtenderProvider.CanExtend(object extendee)
        {
            //find  AutocompleteMenu with lowest hashcode
            if (Container != null)
                foreach (var comp in Container.Components)
                    if (comp is AutocompleteMenu)
                        if (comp.GetHashCode() < GetHashCode())
                            return false;
            //we are main autocomplete menu on form ...
            //check extendee as TextBox
            if (!(extendee is System.Windows.Forms.Control))
                return false;
            var temp = TextBoxWrapper.Create(extendee as System.Windows.Forms.Control);
            return temp != null;
        }

        public void SetAutocompleteMenu(System.Windows.Forms.Control control, AutocompleteMenu menu)
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

        #endregion
    }
}