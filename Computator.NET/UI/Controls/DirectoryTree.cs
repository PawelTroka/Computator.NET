namespace Computator.NET
{
    public class DirectoryTree : System.Windows.Forms.TreeView
    {
        public delegate void DirectorySelectedDelegate(object sender, DirectorySelectedEventArgs e);

        private string drive;

        public string Drive
        {
            get { return drive; }
            set
            {
                drive = value;
                if (drive != null)
                    RefreshDisplay();
            }
        }

        public event DirectorySelectedDelegate DirectorySelected;
        // This is public so a Refresh can be triggered manually.
        public void RefreshDisplay()
        {
            // Erase the existing tree.
            Nodes.Clear();

            // Set the first node.
            var rootNode = new System.Windows.Forms.TreeNode(drive);
            Nodes.Add(rootNode);

            // Fill the first level and expand it.

            Fill(rootNode);
            Nodes[0].Expand();
        }

        private void Fill(System.Windows.Forms.TreeNode dirNode)
        {
            var dir = new System.IO.DirectoryInfo(dirNode.FullPath);

            // An exception could be thrown in this code if you don't
            // have sufficient security permissions for a file or directory.
            // You can catch and then ignore this exception.

            foreach (var dirItem in dir.GetDirectories())
            {
                // Add node for the directory.
                var newNode = new System.Windows.Forms.TreeNode(dirItem.Name);
                dirNode.Nodes.Add(newNode);
                newNode.Nodes.Add("*");
            }
            foreach (var dirItem in dir.GetFiles())
            {
                // Add node for the directory.
                var newNode = new System.Windows.Forms.TreeNode(dirItem.Name);
                dirNode.Nodes.Add(newNode);
                //newNode.Nodes.Add("*");
            }
        }

        protected override void OnBeforeExpand(System.Windows.Forms.TreeViewCancelEventArgs e)
        {
            base.OnBeforeExpand(e);

            // If a dummy node is found, remove it and read the real directory list.
            if (e.Node.Nodes[0].Text == "*")
            {
                e.Node.Nodes.Clear();
                Fill(e.Node);
            }
        }

        protected override void OnAfterSelect(System.Windows.Forms.TreeViewEventArgs e)
        {
            base.OnAfterSelect(e);

            // Raise the DirectorySelected event.
            if (DirectorySelected != null)
            {
                DirectorySelected(this,
                    new DirectorySelectedEventArgs(e.Node.FullPath));
            }
        }
    }

    public class DirectorySelectedEventArgs : System.EventArgs
    {
        public string DirectoryName;

        public DirectorySelectedEventArgs(string directoryName)
        {
            DirectoryName = directoryName;
        }
    }
}