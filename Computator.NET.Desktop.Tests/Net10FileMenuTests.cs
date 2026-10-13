using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows.Forms;
using Computator.NET.Desktop.Controls;
using NUnit.Framework;

namespace Computator.NET.Desktop.Tests
{
    [TestFixture]
    [Apartment(ApartmentState.STA)]
    public class Net10FileMenuTests
    {
        [Test]
        public void FileMenuUsesSupportedControlsAndCannotDeleteTheRoot()
        {
            var directory = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "Computator-Tree-" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(directory);
            try
            {
                ContextMenuStrip menu;
                using (var tree = new DirectoryTree { Path = directory })
                {
                    NUnit.Framework.Assert.That(tree.Handle, Is.Not.EqualTo(IntPtr.Zero));
                    menu = tree.ContextMenuStrip;
                    NUnit.Framework.Assert.That(menu, Is.Not.Null);
                    NUnit.Framework.Assert.That(menu.Items.Count, Is.EqualTo(3));
                    NUnit.Framework.Assert.That(menu.Items.Cast<ToolStripItem>().All(item => item is ToolStripMenuItem), Is.True);
                    menu.Items[2].PerformClick();
                    NUnit.Framework.Assert.That(Directory.Exists(directory), Is.True);
                    NUnit.Framework.Assert.That(tree.Nodes.Count, Is.EqualTo(1));
                }
                NUnit.Framework.Assert.That(menu.IsDisposed, Is.True);
            }
            finally { Directory.Delete(directory, true); }
        }

        [Test]
        public void RefreshReplacesStaleContextAndEmptyDirectoriesCanExpand()
        {
            var directory = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "Computator-Tree-" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(System.IO.Path.Combine(directory, "empty"));
            try
            {
                using (var tree = new DirectoryTree { Path = directory })
                {
                    _ = tree.Handle;
                    tree.Nodes[0].Nodes[0].Expand();
                    var oldRoot = tree.Nodes[0];
                    tree.RefreshDisplay();
                    NUnit.Framework.Assert.That(tree.Nodes[0], Is.Not.SameAs(oldRoot));
                    tree.ContextMenuStrip.Items[2].PerformClick();
                    NUnit.Framework.Assert.That(Directory.Exists(directory), Is.True);
                }
            }
            finally { Directory.Delete(directory, true); }
        }
    }
}
