namespace AutocompleteMenuNS
{
    public class AutocompleteListView : System.Windows.Forms.UserControl, IAutocompleteListView
    {
        private readonly Computator.NET.UI.AutocompleteMenu.WebBrowserForm formTip;
        private readonly int hoveredItemIndex = -1;
        private readonly Computator.NET.UI.AutocompleteMenu.WebBrowserToolTip toolTip;
        private System.Diagnostics.Stopwatch _showToolTipStopwatch = new System.Diagnostics.Stopwatch();
        private System.Threading.Tasks.Task _showToolTipTask = null;
        private System.ComponentModel.BackgroundWorker _showToolTipWorker = null;
        private int itemHeight;
        private int oldItemCount;
        private int selectedItemIndex = -1;
        private System.Collections.Generic.IList<AutocompleteItem> visibleItems;

        internal AutocompleteListView()
        {
            // functionsDetails = new Dictionary<string, FunctionInfo>();
            toolTip = new Computator.NET.UI.AutocompleteMenu.WebBrowserToolTip();
            SetStyle(
                System.Windows.Forms.ControlStyles.AllPaintingInWmPaint |
                System.Windows.Forms.ControlStyles.OptimizedDoubleBuffer | System.Windows.Forms.ControlStyles.UserPaint,
                true);
            base.Font = new System.Drawing.Font(System.Drawing.FontFamily.GenericSansSerif, 9);
            ItemHeight = Font.Height + 2;
            VerticalScroll.SmallChange = ItemHeight;
            BackColor = System.Drawing.Color.White;
            LeftPadding = 18;
            ToolTipDuration = 3000;
            // this.LostFocus += Control_LostFocus;
            //this.VisibleChanged += Control_VisibleChanged;
            //this.ClientSizeChanged += Control_ClientSizeChanged;
            // toolTip.Click += ToolTip_Click;
            //  toolTip.LostFocus += ToolTip_LostFocus;
            formTip = new Computator.NET.UI.AutocompleteMenu.WebBrowserForm();
        }

        public int ItemHeight
        {
            get { return itemHeight; }
            set
            {
                itemHeight = value;
                VerticalScroll.SmallChange = value;
                oldItemCount = -1;
                AdjustScroll();
            }
        }

        public override System.Drawing.Font Font
        {
            get { return base.Font; }
            set
            {
                base.Font = value;
                ItemHeight = Font.Height + 2;
            }
        }

        public int LeftPadding { get; set; }

        /// <summary>
        ///     Duration (ms) of tooltip showing
        /// </summary>
        public int ToolTipDuration { get; set; }

        /// <summary>
        ///     Occurs when user selected item for inserting into text
        /// </summary>
        public event System.EventHandler ItemSelected;

        /// <summary>
        ///     Occurs when current hovered item is changing
        /// </summary>
        public event System.EventHandler<HoveredEventArgs> ItemHovered;

        public System.Windows.Forms.ImageList ImageList { get; set; }

        public System.Collections.Generic.IList<AutocompleteItem> VisibleItems
        {
            get { return visibleItems; }
            set
            {
                visibleItems = value;
                SelectedItemIndex = -1;
                AdjustScroll();
                Invalidate();
            }
        }

        public int SelectedItemIndex
        {
            get { return selectedItemIndex; }
            set
            {
                AutocompleteItem item = null;
                if (value >= 0 && value < VisibleItems.Count)
                    item = VisibleItems[value];

                selectedItemIndex = value;

                OnItemHovered(new HoveredEventArgs {Item = item});

                if (item != null)
                {
                    ShowToolTip(item);
                    ScrollToSelected();
                }

                Invalidate();
            }
        }

        public System.Drawing.Rectangle GetItemRectangle(int itemIndex)
        {
            var y = itemIndex*ItemHeight - VerticalScroll.Value;
            return new System.Drawing.Rectangle(0, y, ClientSize.Width - 1, ItemHeight - 1);
        }

        public void ShowToolTip(AutocompleteItem autocompleteItem, System.Windows.Forms.Control control = null)
        {
            toolTip.Close();
            var signature = autocompleteItem.Text;
            if (!Computator.NET.Config.GlobalConfig.functionsDetails.ContainsKey(signature))
                return;
            var functionInfo = Computator.NET.Config.GlobalConfig.functionsDetails[signature];

            if (string.IsNullOrEmpty(functionInfo.Description) || string.IsNullOrWhiteSpace(functionInfo.Description) ||
                string.IsNullOrEmpty(functionInfo.Title) || string.IsNullOrWhiteSpace(functionInfo.Title)
                || functionInfo.Description.Contains("here goes description (not done yet)")
                || functionInfo.Title.Contains("_title_")
                )
                return;

            if (Computator.NET.Properties.Settings.Default.TooltipType ==
                Computator.NET.DataTypes.SettingsTypes.TooltipType.Default)
            {
                toolTip.setFunctionInfo(functionInfo);

                if (control == null)
                    control = this;

                toolTip.Show(control, Width + 3, 0);
            }
            else if (Computator.NET.Properties.Settings.Default.TooltipType ==
                     Computator.NET.DataTypes.SettingsTypes.TooltipType.Form)
            {
                formTip.setFunctionInfo(functionInfo);
                formTip.Show();
            }
        }

        private void showToolTipTimer_Tick(object sender, System.EventArgs e)
        {
            throw new System.NotImplementedException();
        }

        public void closeToolTip()
        {
            toolTip.Close();
        }

        //  private void ToolTip_Click(object sender, EventArgs e)
        //  {
        //      toolTip.Focus();
        //  }
/*
        private void ToolTip_LostFocus(object sender, EventArgs e)
        {
            toolTip.Close();
        }

       private void Control_ClientSizeChanged(object sender, EventArgs e)
        {
            toolTip.Close();
            this.Invalidate();
        }

        private void Control_VisibleChanged(object sender, EventArgs e)
        {
            if (Visible == false)
                toolTip.Close();
        }
        
        private void Control_LostFocus(object sender, EventArgs e)
        {
            toolTip.Close();
        }*/

        protected override void Dispose(bool disposing)
        {
            // toolTip.Close();
            if (disposing)
            {
                toolTip.Dispose();
            }
            base.Dispose(disposing);
        }

        private void OnItemHovered(HoveredEventArgs e)
        {
            if (ItemHovered != null)
                ItemHovered(this, e);
        }

        private void AdjustScroll()
        {
            if (VisibleItems == null)
                return;
            if (oldItemCount == VisibleItems.Count)
                return;

            var needHeight = ItemHeight*VisibleItems.Count + 1;
            Height = System.Math.Min(needHeight, MaximumSize.Height);
            AutoScrollMinSize = new System.Drawing.Size(0, needHeight);
            oldItemCount = VisibleItems.Count;
        }

        private void ScrollToSelected()
        {
            var y = SelectedItemIndex*ItemHeight - VerticalScroll.Value;
            if (y < 0)
                VerticalScroll.Value = SelectedItemIndex*ItemHeight;
            if (y > ClientSize.Height - ItemHeight)
                VerticalScroll.Value = System.Math.Min(VerticalScroll.Maximum,
                    SelectedItemIndex*ItemHeight - ClientSize.Height + ItemHeight);
            //some magic for update scrolls
            AutoScrollMinSize -= new System.Drawing.Size(1, 0);
            AutoScrollMinSize += new System.Drawing.Size(1, 0);
        }

        protected override void OnPaint(System.Windows.Forms.PaintEventArgs e)
        {
            var rtl = RightToLeft == System.Windows.Forms.RightToLeft.Yes;
            AdjustScroll();
            var startI = VerticalScroll.Value/ItemHeight - 1;
            var finishI = (VerticalScroll.Value + ClientSize.Height)/ItemHeight + 1;
            startI = System.Math.Max(startI, 0);
            finishI = System.Math.Min(finishI, VisibleItems.Count);
            var y = 0;

            for (var i = startI; i < finishI; i++)
            {
                y = i*ItemHeight - VerticalScroll.Value;

                if (ImageList != null && VisibleItems[i].ImageIndex >= 0)
                    if (rtl)
                        e.Graphics.DrawImage(ImageList.Images[VisibleItems[i].ImageIndex], Width - 1 - LeftPadding, y);
                    else
                        e.Graphics.DrawImage(ImageList.Images[VisibleItems[i].ImageIndex], 1, y);

                var textRect = new System.Drawing.Rectangle(LeftPadding, y, ClientSize.Width - 1 - LeftPadding,
                    ItemHeight - 1);
                if (rtl)
                    textRect = new System.Drawing.Rectangle(1, y, ClientSize.Width - 1 - LeftPadding, ItemHeight - 1);

                if (i == SelectedItemIndex)
                {
                    System.Drawing.Brush selectedBrush =
                        new System.Drawing.Drawing2D.LinearGradientBrush(new System.Drawing.Point(0, y - 3),
                            new System.Drawing.Point(0, y + ItemHeight),
                            System.Drawing.Color.White, System.Drawing.Color.Orange);
                    e.Graphics.FillRectangle(selectedBrush, textRect);
                    e.Graphics.DrawRectangle(System.Drawing.Pens.Orange, textRect);
                }
                if (i == hoveredItemIndex)
                    e.Graphics.DrawRectangle(System.Drawing.Pens.Red, textRect);

                var sf = new System.Drawing.StringFormat();
                if (rtl)
                    sf.FormatFlags = System.Drawing.StringFormatFlags.DirectionRightToLeft;

                var args = new PaintItemEventArgs(e.Graphics, e.ClipRectangle)
                {
                    Font = Font,
                    TextRect = new System.Drawing.RectangleF(textRect.Location, textRect.Size),
                    StringFormat = sf,
                    IsSelected = i == SelectedItemIndex,
                    IsHovered = i == hoveredItemIndex
                };
                //call drawing
                VisibleItems[i].OnPaint(args);
            }
        }

        protected override void OnScroll(System.Windows.Forms.ScrollEventArgs se)
        {
            base.OnScroll(se);
            Invalidate(true);
        }

        protected override void OnMouseClick(System.Windows.Forms.MouseEventArgs e)
        {
            base.OnMouseClick(e);

            if (e.Button == System.Windows.Forms.MouseButtons.Left)
            {
                SelectedItemIndex = PointToItemIndex(e.Location);
                ScrollToSelected();
                Invalidate();
            }
        }

        protected override void OnMouseDoubleClick(System.Windows.Forms.MouseEventArgs e)
        {
            base.OnMouseDoubleClick(e);
            SelectedItemIndex = PointToItemIndex(e.Location);
            Invalidate();
            OnItemSelected();
        }

        private void OnItemSelected()
        {
            if (ItemSelected != null)
                ItemSelected(this, System.EventArgs.Empty);
        }

        private int PointToItemIndex(System.Drawing.Point p)
        {
            return (p.Y + VerticalScroll.Value)/ItemHeight;
        }

        protected override bool ProcessCmdKey(ref System.Windows.Forms.Message msg, System.Windows.Forms.Keys keyData)
        {
            var host = Parent as AutocompleteMenuHost;
            if (host != null)
                if (host.Menu.ProcessKey((char) keyData, System.Windows.Forms.Keys.None))
                    return true;

            return base.ProcessCmdKey(ref msg, keyData);
        }

        public void SelectItem(int itemIndex)
        {
            SelectedItemIndex = itemIndex;
            ScrollToSelected();
            Invalidate();
        }

        public void SetItems(System.Collections.Generic.List<AutocompleteItem> items)
        {
            VisibleItems = items;
            SelectedItemIndex = -1;
            AdjustScroll();
            Invalidate();
        }
    }
}