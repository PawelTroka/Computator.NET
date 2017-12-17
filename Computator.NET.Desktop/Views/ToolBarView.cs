using System.Windows.Forms;
using Computator.NET.Core.Menu.Commands;
using Computator.NET.Core.Menu.Commands.EditCommands;
using Computator.NET.Core.Menu.Commands.FileCommands;
using Computator.NET.Core.Menu.Commands.TopMenuCommands;
using HelpCommand = Computator.NET.Core.Menu.Commands.HelpCommands.HelpCommand;

namespace Computator.NET.Desktop.Views
{
    public partial class ToolBarView : UserControl
    {
        private ToolBarView()
        {
            InitializeComponent();
        }

        public ToolBarView(NewCommand newCommand, OpenCommand openCommand, SaveCommand saveCommand, PrintCommand printCommand, CutCommand cutCommand, CopyCommand copyCommand, PasteCommand pasteCommand, HelpCommand helpCommand, ExponentCommand exponentCommand, RunCommand runCommand) : this()
        {
            SetCommands(newCommand,openCommand,saveCommand,null,printCommand,cutCommand,copyCommand,pasteCommand,null,helpCommand,null,exponentCommand,null,runCommand);
        }

        private void SetCommands(params IToolbarCommand[] commands)
        {
            toolStrip1.Items.Clear();
            foreach (var command in commands)
            {
                if (command == null)
                {
                    toolStrip1.Items.Add(new ToolStripSeparator());
                    continue;
                }
                var button = new ToolStripButton
                {
                    Checked = command.Checked,
                    CheckOnClick = command.CheckOnClick,
                    ToolTipText = command.ToolTip.Replace(@"&", ""), //hack for accelerator keys
                    Text = command.Text,
                    Image = command.Icon,
                    Enabled = command.IsEnabled,
                    Visible = command.Visible,
                    ImageScaling = ToolStripItemImageScaling.SizeToFit,
                    DisplayStyle = ToolStripItemDisplayStyle.Image
                };


                var c = command; // create a closure around the command
                command.PropertyChanged += (o, s) =>
                {
                    button.CheckOnClick = c.CheckOnClick;
                    button.Checked = c.Checked;
                    button.ToolTipText = c.ToolTip;

                    button.Visible = c.Visible;
                    button.Text = c.Text;
                    button.Image = c.Icon;
                    button.Enabled = c.IsEnabled;
                };
                button.Click += (sender, args) => c.Execute();
                toolStrip1.Items.Add(button);
            }
        }
    }
}