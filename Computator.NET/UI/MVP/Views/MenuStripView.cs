using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Computator.NET.UI.Commands;

namespace Computator.NET.UI.MVP.Views
{
    public partial class MenuStripView : UserControl, IToolbarView
    {
        public MenuStripView()
        {
            InitializeComponent();
        }

        public void SetCommands(IEnumerable<IToolbarCommand> commands)
        {
            menuStrip2.Items.Clear();
            foreach (var command in commands)
            {
                if (command == null)
                {
                    menuStrip2.Items.Add(new ToolStripSeparator());
                    continue;
                }

                var button = CommandToButton(command);
                menuStrip2.Items.Add(button);
                AddChildren(button, command.ChildrenCommands);
            }
        }

        private static ToolStripMenuItem CommandToButton(IToolbarCommand command)
        {
            var button = new ToolStripMenuItem()
            {
                Checked = command.Checked,
                CheckOnClick = command.CheckOnClick,
                ToolTipText = command.ToolTip.Replace(@"&", ""), //hack for accelerator keys
                Text = command.Text,
                Image = command.Icon,
                Enabled = command.IsEnabled,
                ImageScaling = ToolStripItemImageScaling.None,
                DisplayStyle = ToolStripItemDisplayStyle.ImageAndText,
                ShowShortcutKeys = true,
                ShortcutKeyDisplayString = command.ShortcutKeyString
            };
            var c = command; // create a closure around the command
            command.PropertyChanged += (o, s) =>
            {
                button.CheckOnClick = c.CheckOnClick;
                button.Checked = c.Checked;
                button.ToolTipText = c.ToolTip;

                button.Text = c.ToolTip;
                button.Image = c.Icon;
                button.Enabled = c.IsEnabled;
            };

            if (!(command is DummyCommand))
                button.Click += (sender, args) => c.Execute();
            return button;
        }

        private void AddChildren(ToolStripMenuItem button, IEnumerable<IToolbarCommand> childrenCommands)
        {
            if (childrenCommands == null)
                return;
            foreach (var childrenCommand in childrenCommands)
            {
                if (childrenCommand == null)
                {
                    button.DropDownItems.Add(new ToolStripSeparator());
                    continue;
                }

                var newButton = CommandToButton(childrenCommand);
                button.DropDownItems.Add(newButton);
                AddChildren(newButton,childrenCommand.ChildrenCommands);
            }
        }
    }
}
