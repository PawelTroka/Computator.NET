using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace Computator.NET.Desktop.Controls.AutocompleteMenu
{
    [ToolboxItem(false)]
    internal class AutocompleteMenuHost : ToolStripDropDown
    {
        public readonly AutocompleteMenu Menu;
        private IAutocompleteListView listView;

        public AutocompleteMenuHost(AutocompleteMenu menu)
        {
            AutoClose = false;
            AutoSize = false;
            
            Margin = Padding.Empty;
            Padding = Padding.Empty;

            Menu = menu;
            ListView = new AutocompleteListView();
        }

        public ToolStripControlHost Host { get; set; }

        public IAutocompleteListView ListView
        {
            get { return listView; }
            set
            {
                if (listView != null)
                    (listView as Control).LostFocus -= ListView_LostFocus;

                if (value == null)
                    listView = new AutocompleteListView();
                else
                {
                    if (!(value is Control))
                        throw new Exception("ListView must be derived from Control class");

                    listView = value;
                }

                Host = new ToolStripControlHost(ListView as Control)
                {
                    Margin = new Padding(2, 2, 2, 2),
                    Padding = Padding.Empty,
                    AutoSize = false,
                    AutoToolTip = false
                };

                (ListView as Control).MaximumSize = Menu.MaximumSize;
                (ListView as Control).Size = Menu.MaximumSize;
                (ListView as Control).LostFocus += ListView_LostFocus;

                CalcSize();
                Items.Clear();
                Items.Add(Host);
                (ListView as Control).Parent = this;
            }
        }

        public override RightToLeft RightToLeft
        {
            get { return base.RightToLeft; }
            set
            {
                base.RightToLeft = value;
                (ListView as Control).RightToLeft = value;
            }
        }

        protected override void OnPaintBackground(PaintEventArgs e)
        {
            using (var brush = new SolidBrush(listView.Colors.BackColor))
                e.Graphics.FillRectangle(brush, e.ClipRectangle);
        }

        internal void CalcSize()
        {
            Host.Size = (ListView as Control).Size;
            Size = new Size((ListView as Control).Size.Width + 4, (ListView as Control).Size.Height + 4);
        }

        protected override void OnLostFocus(EventArgs e)
        {
            base.OnLostFocus(e);
            if (!(ListView as Control).Focused)
                Close();
        }

        private void ListView_LostFocus(object sender, EventArgs e)
        {
            if (!Focused)
                Close();
        }
    }
}