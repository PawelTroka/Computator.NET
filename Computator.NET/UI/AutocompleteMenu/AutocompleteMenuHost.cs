namespace AutocompleteMenuNS
{
    internal class AutocompleteMenuHost : System.Windows.Forms.ToolStripDropDown
    {
        public readonly AutocompleteMenu Menu;
        private IAutocompleteListView listView;

        public AutocompleteMenuHost(AutocompleteMenu menu)
        {
            AutoClose = false;
            AutoSize = false;
            Margin = System.Windows.Forms.Padding.Empty;
            Padding = System.Windows.Forms.Padding.Empty;

            Menu = menu;
            ListView = new AutocompleteListView();
        }

        public System.Windows.Forms.ToolStripControlHost Host { get; set; }

        public IAutocompleteListView ListView
        {
            get { return listView; }
            set
            {
                if (value == null)
                    listView = new AutocompleteListView();
                else
                {
                    if (!(value is System.Windows.Forms.Control))
                        throw new System.Exception("ListView must be derived from Control class");

                    listView = value;
                }

                Host = new System.Windows.Forms.ToolStripControlHost(ListView as System.Windows.Forms.Control);
                Host.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
                Host.Padding = System.Windows.Forms.Padding.Empty;
                Host.AutoSize = false;
                Host.AutoToolTip = false;

                (ListView as System.Windows.Forms.Control).MaximumSize = Menu.MaximumSize;
                (ListView as System.Windows.Forms.Control).Size = Menu.MaximumSize;

                CalcSize();
                Items.Clear();
                Items.Add(Host);
                (ListView as System.Windows.Forms.Control).Parent = this;
            }
        }

        public override System.Windows.Forms.RightToLeft RightToLeft
        {
            get { return base.RightToLeft; }
            set
            {
                base.RightToLeft = value;
                (ListView as System.Windows.Forms.Control).RightToLeft = value;
            }
        }

        internal void CalcSize()
        {
            Host.Size = (ListView as System.Windows.Forms.Control).Size;
            Size = new System.Drawing.Size((ListView as System.Windows.Forms.Control).Size.Width + 4,
                (ListView as System.Windows.Forms.Control).Size.Height + 4);
        }
    }
}