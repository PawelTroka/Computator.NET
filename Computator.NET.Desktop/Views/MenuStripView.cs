using System.Collections.Generic;
using System.Windows.Forms;
using Computator.NET.Controls;
using Computator.NET.Core.Menu.Commands;
using Computator.NET.Core.Menu.Commands.DummyCommands;
using Computator.NET.Core.Menu.Commands.TopMenuCommands;

namespace Computator.NET.Views
{
    public partial class MenuStripView : UserControl
    {
        private MenuStripView()
        {
            InitializeComponent();
        }

        public MenuStripView(FileCommand fileCommand, EditCommand editCommand, FunctionsCommand functionsCommand, ConstantsCommand constantsCommand, ChartCommand chartCommand, TransformCommand transformCommand, ToolsCommand toolsCommand, HelpCommand helpCommand) : this()
        {
            SetCommands(fileCommand, editCommand, functionsCommand, constantsCommand, chartCommand, transformCommand,
                toolsCommand, helpCommand);
        }

        public void SetCommands(params IToolbarCommand[] commands)
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
            var button = new ToolStripMenuItem();
            if (command.IsOption)
                button = new ToolStripRadioButtonMenuItem();

            button.Checked = command.Checked;
            button.CheckOnClick = command.CheckOnClick;
            button.ToolTipText = command.ToolTip?.Replace(@"&", "");
            button.Text = command.Text;
            button.Image = command.Icon;
            button.Enabled = command.IsEnabled;
            button.Visible = command.Visible;
            button.ImageScaling = ToolStripItemImageScaling.None;
            button.DisplayStyle = ToolStripItemDisplayStyle.ImageAndText;
            button.ShowShortcutKeys = true;
            button.ShortcutKeyDisplayString = command.ShortcutKeyString;
            var c = command; // create a closure around the command
            command.PropertyChanged += (o, s) =>
            {
                button.ShortcutKeyDisplayString = c.ShortcutKeyString;
                button.CheckOnClick = c.CheckOnClick;
                button.Checked = c.Checked;
                button.ToolTipText = c.ToolTip;
                button.Visible = c.Visible;
                button.Text = c.Text;
                button.Image = c.Icon;
                button.Enabled = c.IsEnabled;
            };

            if (!(command is DummyCommand))
            {
                button.Click += (sender, args) => c.Execute();
                button.MouseDown += (sender, args) =>
                {
                    if(args.Button!=MouseButtons.Left && !(SystemInformation.MouseButtonsSwapped
#if !__MonoCS__
                    || System.Windows.SystemParameters.SwapButtons
#endif
                    ))
                    c.Execute();
                    else if (args.Button != MouseButtons.Right && (SystemInformation.MouseButtonsSwapped
#if !__MonoCS__
                    || System.Windows.SystemParameters.SwapButtons
#endif
                    ))
                        c.Execute();
                };
            }
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
                    button?.DropDownItems.Add(new ToolStripSeparator());
                    continue;
                }

                var newButton = CommandToButton(childrenCommand);
                button?.DropDownItems.Add(newButton);


                AddChildren(newButton, childrenCommand.ChildrenCommands);
            }
        }
    }
}