using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Threading.Tasks;
using System.Windows.Forms;
using Computator.NET.Config;
using Computator.NET.DataTypes.SettingsTypes;
using Computator.NET.UI.AutocompleteMenu;
using Settings = Computator.NET.Properties.Settings;

namespace AutocompleteMenuNS
{
    public class AutocompleteListView : UserControl, IAutocompleteListView
    {
        private readonly WebBrowserForm formTip;
        private readonly int hoveredItemIndex = -1;
        private readonly WebBrowserToolTip toolTip;
        private Stopwatch _showToolTipStopwatch = new Stopwatch();
        private Task _showToolTipTask = null;
        private BackgroundWorker _showToolTipWorker = null;
        private int itemHeight;
        private int oldItemCount;
        private int selectedItemIndex = -1;
        private IList<AutocompleteItem> visibleItems;

        internal AutocompleteListView()
        {
            // functionsDetails = new Dictionary<string, FunctionInfo>();
            toolTip = new WebBrowserToolTip();
            SetStyle(
                ControlStyles.AllPaintingInWmPaint |
                ControlStyles.OptimizedDoubleBuffer | ControlStyles.UserPaint,
                true);
            base.Font = new Font(FontFamily.GenericSansSerif, 9);
            ItemHeight = Font.Height + 2;
            VerticalScroll.SmallChange = ItemHeight;
            BackColor = Color.White;
            LeftPadding = 18;
            ToolTipDuration = 3000;
            // this.LostFocus += Control_LostFocus;
            //this.VisibleChanged += Control_VisibleChanged;
            //this.ClientSizeChanged += Control_ClientSizeChanged;
            // toolTip.Click += ToolTip_Click;
            //  toolTip.LostFocus += ToolTip_LostFocus;
            formTip = new WebBrowserForm();
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

        public override Font Font
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
        public event EventHandler ItemSelected;

        /// <summary>
        ///     Occurs when current hovered item is changing
        /// </summary>
        public event EventHandler<HoveredEventArgs> ItemHovered;

        public ImageList ImageList { get; set; }

        public IList<AutocompleteItem> VisibleItems
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

        public Rectangle GetItemRectangle(int itemIndex)
        {
            var y = itemIndex*ItemHeight - VerticalScroll.Value;
            return new Rectangle(0, y, ClientSize.Width - 1, ItemHeight - 1);
        }

        public void ShowToolTip(AutocompleteItem autocompleteItem, Control control = null)
        {
            toolTip.Close();
            var signature = autocompleteItem.Text;
            if (!GlobalConfig.functionsDetails.ContainsKey(signature))
                return;
            var functionInfo = GlobalConfig.functionsDetails[signature];

            if (string.IsNullOrEmpty(functionInfo.Description) || string.IsNullOrWhiteSpace(functionInfo.Description) ||
                string.IsNullOrEmpty(functionInfo.Title) || string.IsNullOrWhiteSpace(functionInfo.Title)
                || functionInfo.Description.Contains("here goes description (not done yet)")
                || functionInfo.Title.Contains("_title_")
                )
                return;

            if (Settings.Default.TooltipType ==
                TooltipType.Default)
            {
                toolTip.setFunctionInfo(functionInfo);

                if (control == null)
                    control = this;

                toolTip.Show(control, Width + 3, 0);
            }
            else if (Settings.Default.TooltipType ==
                     TooltipType.Form)
            {
                formTip.setFunctionInfo(functionInfo);
                formTip.Show();
            }
        }

        private void showToolTipTimer_Tick(object sender, EventArgs e)
        {
            throw new NotImplementedException();
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
            Height = Math.Min(needHeight, MaximumSize.Height);
            AutoScrollMinSize = new Size(0, needHeight);
            oldItemCount = VisibleItems.Count;
        }

        private void ScrollToSelected()
        {
            var y = SelectedItemIndex*ItemHeight - VerticalScroll.Value;
            if (y < 0)
                VerticalScroll.Value = SelectedItemIndex*ItemHeight;
            if (y > ClientSize.Height - ItemHeight)
                VerticalScroll.Value = Math.Min(VerticalScroll.Maximum,
                    SelectedItemIndex*ItemHeight - ClientSize.Height + ItemHeight);
            //some magic for update scrolls
            AutoScrollMinSize -= new Size(1, 0);
            AutoScrollMinSize += new Size(1, 0);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            var rtl = RightToLeft == RightToLeft.Yes;
            AdjustScroll();
            var startI = VerticalScroll.Value/ItemHeight - 1;
            var finishI = (VerticalScroll.Value + ClientSize.Height)/ItemHeight + 1;
            startI = Math.Max(startI, 0);
            finishI = Math.Min(finishI, VisibleItems.Count);
            var y = 0;

            for (var i = startI; i < finishI; i++)
            {
                y = i*ItemHeight - VerticalScroll.Value;

                if (ImageList != null && VisibleItems[i].ImageIndex >= 0)
                    if (rtl)
                        e.Graphics.DrawImage(ImageList.Images[VisibleItems[i].ImageIndex], Width - 1 - LeftPadding, y);
                    else
                        e.Graphics.DrawImage(ImageList.Images[VisibleItems[i].ImageIndex], 1, y);

                var textRect = new Rectangle(LeftPadding, y, ClientSize.Width - 1 - LeftPadding,
                    ItemHeight - 1);
                if (rtl)
                    textRect = new Rectangle(1, y, ClientSize.Width - 1 - LeftPadding, ItemHeight - 1);

                if (i == SelectedItemIndex)
                {
                    Brush selectedBrush =
                        new LinearGradientBrush(new Point(0, y - 3),
                            new Point(0, y + ItemHeight),
                            Color.White, Color.Orange);
                    e.Graphics.FillRectangle(selectedBrush, textRect);
                    e.Graphics.DrawRectangle(Pens.Orange, textRect);
                }
                if (i == hoveredItemIndex)
                    e.Graphics.DrawRectangle(Pens.Red, textRect);

                var sf = new StringFormat();
                if (rtl)
                    sf.FormatFlags = StringFormatFlags.DirectionRightToLeft;

                var args = new PaintItemEventArgs(e.Graphics, e.ClipRectangle)
                {
                    Font = Font,
                    TextRect = new RectangleF(textRect.Location, textRect.Size),
                    StringFormat = sf,
                    IsSelected = i == SelectedItemIndex,
                    IsHovered = i == hoveredItemIndex
                };
                //call drawing
                VisibleItems[i].OnPaint(args);
            }
        }

        protected override void OnScroll(ScrollEventArgs se)
        {
            base.OnScroll(se);
            Invalidate(true);
        }

        protected override void OnMouseClick(MouseEventArgs e)
        {
            base.OnMouseClick(e);

            if (e.Button == MouseButtons.Left)
            {
                SelectedItemIndex = PointToItemIndex(e.Location);
                ScrollToSelected();
                Invalidate();
            }
        }

        protected override void OnMouseDoubleClick(MouseEventArgs e)
        {
            base.OnMouseDoubleClick(e);
            SelectedItemIndex = PointToItemIndex(e.Location);
            Invalidate();
            OnItemSelected();
        }

        private void OnItemSelected()
        {
            if (ItemSelected != null)
                ItemSelected(this, EventArgs.Empty);
        }

        private int PointToItemIndex(Point p)
        {
            return (p.Y + VerticalScroll.Value)/ItemHeight;
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            var host = Parent as AutocompleteMenuHost;
            if (host != null)
                if (host.Menu.ProcessKey((char) keyData, Keys.None))
                    return true;

            return base.ProcessCmdKey(ref msg, keyData);
        }

        public void SelectItem(int itemIndex)
        {
            SelectedItemIndex = itemIndex;
            ScrollToSelected();
            Invalidate();
        }

        public void SetItems(List<AutocompleteItem> items)
        {
            VisibleItems = items;
            SelectedItemIndex = -1;
            AdjustScroll();
            Invalidate();
        }
    }
}