using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using Computator.NET.Config;
using Computator.NET.Properties;
using Computator.NET.UI.CodeEditors;
using Computator.NET.UI.Menus.Commands;
using Computator.NET.UI.MVP;
using Computator.NET.UI.MVP.Views;

namespace Computator.NET.UI.Commands
{

    class ExponentCommand : CommandBase
    {
        public ExponentCommand()
        {
            this.CheckOnClick = true;

            SharedViewState.Instance.PropertyChanged += (o, e) =>
            {
                if (e.PropertyName == nameof(SharedViewState.Instance.IsExponent))
                    this.Checked = SharedViewState.Instance.IsExponent;
            };

            this.Icon = Resources.exponentation;
            this.Text = MenuStrings.exponentiationToolStripMenuItem_Text;
            this.ToolTip = MenuStrings.exponentiationToolStripMenuItem_Text;
        }


        public override void Execute()
        {
            SharedViewState.Instance.IsExponent = !SharedViewState.Instance.IsExponent;
        }
    }

    class RunCommand : CommandBase
    {


        public RunCommand()
        {
            this.Icon = Resources.runToolStripButtonImage;
            this.Text = MenuStrings.runToolStripButton_Text;
            this.ToolTip = MenuStrings.runToolStripButton_Text;
        }


        public override void Execute()
        {
            SharedViewState.Instance.CurrentAction.Invoke(this, new EventArgs());
        }
    }

    class HelpCommand : CommandBase
    {


        public HelpCommand()
        {
            this.Icon = Resources.helpToolStripButtonImage;
            this.Text = MenuStrings.helpToolStripButton_Text;
            this.ToolTip = MenuStrings.helpToolStripButton_Text;
        }


        public override void Execute()
        {
            new AboutBox1().ShowDialog();
        }
    }

    class PrintPreviewCommand : CommandBase
    {
        private ICanFileEdit scriptingCodeEditor;
        private ICanFileEdit customFunctionsCodeEditor;

        private IMainForm mainFormView;

        public PrintPreviewCommand(ICanFileEdit scriptingCodeEditor, ICanFileEdit customFunctionsCodeEditor, IMainForm mainFormView)
        {
          //  this.Icon = Resources.printToolStripButtonImage;
            this.Text = MenuStrings.printPreviewToolStripMenuItem_Text;
            this.ToolTip = MenuStrings.printPreviewToolStripMenuItem_Text;

            this.scriptingCodeEditor = scriptingCodeEditor;
            this.customFunctionsCodeEditor = customFunctionsCodeEditor;
            this.mainFormView = mainFormView;
        }


        public override void Execute()
        {
            switch ((int)SharedViewState.Instance.CurrentView)
            {
                case 0:
                    //if (_calculationsMode == CalculationsMode.Real)
                    mainFormView.ChartingView.Charts[SharedViewState.Instance.CalculationsMode].PrintPreview();
                    // else
                    //  SendStringAsKey("^P");
                    break;

                case 4:
                    //scriptingCodeEditor();

                    break;

                case 5:
                    //this.customFunctionsCodeEditor
                    break;

                default:
                    mainFormView.SendStringAsKey("^P"); //this.chart2d.Printing.PrintPreview();
                    break;
            }

        }
    }
    class PrintCommand : CommandBase
    {
        private ICanFileEdit scriptingCodeEditor;
        private ICanFileEdit customFunctionsCodeEditor;

        private IMainForm mainFormView;

        public PrintCommand(ICanFileEdit scriptingCodeEditor, ICanFileEdit customFunctionsCodeEditor, IMainForm mainFormView)
        {
            this.Icon = Resources.printToolStripButtonImage;
            this.Text = MenuStrings.printToolStripButton_Text;
            this.ToolTip = MenuStrings.printToolStripButton_Text;

            this.scriptingCodeEditor = scriptingCodeEditor;
            this.customFunctionsCodeEditor = customFunctionsCodeEditor;
            this.mainFormView = mainFormView;
        }


        public override void Execute()
        {
            switch ((int)SharedViewState.Instance.CurrentView)
            {
                case 0:
                    //if (_calculationsMode == CalculationsMode.Real)
                    mainFormView.ChartingView.Charts[SharedViewState.Instance.CalculationsMode].Print();
                    // else
                    //  SendStringAsKey("^P");
                    break;

                case 4:
                    //scriptingCodeEditor();

                    break;

                case 5:
                    //this.customFunctionsCodeEditor
                    break;

                default:
                    mainFormView.SendStringAsKey("^P"); //this.chart2d.Printing.PrintPreview();
                    break;
            }

        }
    }

    class UndoCommand : CommandBase
    {
        private ICanFileEdit scriptingCodeEditor;
        private ICanFileEdit customFunctionsCodeEditor;
        private IMainForm mainFormView;

        public UndoCommand(ICanFileEdit scriptingCodeEditor, ICanFileEdit customFunctionsCodeEditor, IMainForm mainFormView)
        {
            //this.Icon = Resources.copyToolStripButtonImage;
            this.Text = MenuStrings.undoToolStripMenuItem_Text;
            this.ToolTip = MenuStrings.undoToolStripMenuItem_Text;

            this.scriptingCodeEditor = scriptingCodeEditor;
            this.customFunctionsCodeEditor = customFunctionsCodeEditor;
            this.mainFormView = mainFormView;
            // this.mainFormView = mainFormView;
        }


        public override void Execute()
        {
            if ((int)SharedViewState.Instance.CurrentView < 4)
                mainFormView.SendStringAsKey("^Z"); //expressionTextBox.Undo();
            else if ((int)SharedViewState.Instance.CurrentView == 4)
            {
                if (scriptingCodeEditor.Focused)
                    scriptingCodeEditor.Undo();
                else
                    mainFormView.SendStringAsKey("^Z");
            }
            else
            {
                if (customFunctionsCodeEditor.Focused)
                    customFunctionsCodeEditor.Undo();
                else
                    mainFormView.SendStringAsKey("^Z");
            }

            mainFormView.SendStringAsKey("^Z");
        }
    }

    class RedoCommand : CommandBase
    {
        private ICanFileEdit scriptingCodeEditor;
        private ICanFileEdit customFunctionsCodeEditor;
        private IMainForm mainFormView;

        public RedoCommand(ICanFileEdit scriptingCodeEditor, ICanFileEdit customFunctionsCodeEditor, IMainForm mainFormView)
        {
            //this.Icon = Resources.copyToolStripButtonImage;
            this.Text = MenuStrings.redoToolStripMenuItem_Text;
            this.ToolTip = MenuStrings.redoToolStripMenuItem_Text;

            this.scriptingCodeEditor = scriptingCodeEditor;
            this.customFunctionsCodeEditor = customFunctionsCodeEditor;
            this.mainFormView = mainFormView;
            // this.mainFormView = mainFormView;
        }


        public override void Execute()
        {
            if ((int)SharedViewState.Instance.CurrentView < 4)
            {
                mainFormView.SendStringAsKey("^Y");
                //expressionTextBox.do()
            }
            else if ((int)SharedViewState.Instance.CurrentView == 4)
            //scriptingCodeEditor.Focus();
            {
                if (scriptingCodeEditor.Focused)
                    scriptingCodeEditor.Redo();
                else
                    mainFormView.SendStringAsKey("^Y");
            }
            else
            {
                if (customFunctionsCodeEditor.Focused)
                    customFunctionsCodeEditor.Redo();
                else
                    mainFormView.SendStringAsKey("^Y");
            }

            mainFormView.SendStringAsKey("^Y");
        }
    }



    class SelectAllCommand : CommandBase
    {
        private ICanFileEdit scriptingCodeEditor;
        private ICanFileEdit customFunctionsCodeEditor;
        private IMainForm mainFormView;

        public SelectAllCommand(ICanFileEdit scriptingCodeEditor, ICanFileEdit customFunctionsCodeEditor, IMainForm mainFormView)
        {
            //this.Icon = Resources.copyToolStripButtonImage;
            this.Text = MenuStrings.selectAllToolStripMenuItem_Text;
            this.ToolTip = MenuStrings.selectAllToolStripMenuItem_Text;

            this.scriptingCodeEditor = scriptingCodeEditor;
            this.customFunctionsCodeEditor = customFunctionsCodeEditor;
            this.mainFormView = mainFormView;
            // this.mainFormView = mainFormView;
        }


        public override void Execute()
        {
            if ((int)SharedViewState.Instance.CurrentView < 4)
            {
                mainFormView.SendStringAsKey("^A"); //expressionTextBox.SelectAll();
            }
            else if ((int)SharedViewState.Instance.CurrentView == 4)
            {
                if (scriptingCodeEditor.Focused)
                    scriptingCodeEditor.SelectAll();
                else
                    mainFormView.SendStringAsKey("^A");
            }
            else
            {
                if (customFunctionsCodeEditor.Focused)
                    customFunctionsCodeEditor.SelectAll();
                //  else
                mainFormView.SendStringAsKey("^A");
            }

            mainFormView.SendStringAsKey("^A");
        }
    }






    class CutCommand : CommandBase
    {
        private ICanFileEdit scriptingCodeEditor;
        private ICanFileEdit customFunctionsCodeEditor;
        private IMainForm mainFormView;

        public CutCommand(ICanFileEdit scriptingCodeEditor, ICanFileEdit customFunctionsCodeEditor, IMainForm mainFormView)
        {
            this.Icon = Resources.cutToolStripButtonImage;
            this.Text = MenuStrings.cutToolStripButton_Text;
            this.ToolTip = MenuStrings.cutToolStripButton_Text;

            this.scriptingCodeEditor = scriptingCodeEditor;
            this.customFunctionsCodeEditor = customFunctionsCodeEditor;
            this.mainFormView = mainFormView;
            // this.mainFormView = mainFormView;
        }


        public override void Execute()
        {

            switch ((int)SharedViewState.Instance.CurrentView)
            {
                case 4:
                    if (scriptingCodeEditor.Focused)
                        scriptingCodeEditor.Cut();
                    else
                        mainFormView.SendStringAsKey("^X");
                    break;

                case 5:
                    if (customFunctionsCodeEditor.Focused)
                        customFunctionsCodeEditor.Cut();
                    else
                        mainFormView.SendStringAsKey("^X");
                    break;

                default: //if ((int)SharedViewState.Instance.CurrentView < 4)
                    mainFormView.SendStringAsKey("^X"); //expressionTextBox.Cut();
                    break;
            }
        }
    }

    class CopyCommand : CommandBase
    {
        private ICanFileEdit scriptingCodeEditor;
        private ICanFileEdit customFunctionsCodeEditor;
        private IMainForm mainFormView;

        public CopyCommand(ICanFileEdit scriptingCodeEditor, ICanFileEdit customFunctionsCodeEditor, IMainForm mainFormView)
        {
            this.Icon = Resources.copyToolStripButtonImage;
            this.Text = MenuStrings.copyToolStripButton_Text;
            this.ToolTip = MenuStrings.copyToolStripButton_Text;

            this.scriptingCodeEditor = scriptingCodeEditor;
            this.customFunctionsCodeEditor = customFunctionsCodeEditor;
            this.mainFormView = mainFormView;
            // this.mainFormView = mainFormView;
        }


        public override void Execute()
        {

            if ((int)SharedViewState.Instance.CurrentView < 4)
            {
                mainFormView.SendStringAsKey("^C"); //expressionTextBox.Copy();
            }
            else if ((int)SharedViewState.Instance.CurrentView == 4)
            {
                if (scriptingCodeEditor.Focused)
                    scriptingCodeEditor.Copy();
                else
                    mainFormView.SendStringAsKey("^C");
            }
            else
            {
                if (customFunctionsCodeEditor.Focused)
                    customFunctionsCodeEditor.Copy();
                else
                    mainFormView.SendStringAsKey("^C");
            }
        }
    }

    class PasteCommand : CommandBase
    {
        private ICanFileEdit scriptingCodeEditor;
        private ICanFileEdit customFunctionsCodeEditor;
        private IMainForm mainFormView;

        public PasteCommand(ICanFileEdit scriptingCodeEditor, ICanFileEdit customFunctionsCodeEditor, IMainForm mainFormView)
        {
            this.Icon = Resources.pasteToolStripButtonImage;
            this.Text = MenuStrings.pasteToolStripButton_Text;
            this.ToolTip = MenuStrings.pasteToolStripButton_Text;

            this.scriptingCodeEditor = scriptingCodeEditor;
            this.customFunctionsCodeEditor = customFunctionsCodeEditor;
            this.mainFormView = mainFormView;
            // this.mainFormView = mainFormView;
        }


        public override void Execute()
        {

            if ((int)SharedViewState.Instance.CurrentView < 4)
            {
                mainFormView.SendStringAsKey("^V"); //expressionTextBox.Paste();
            }
            else if ((int)SharedViewState.Instance.CurrentView == 4)
            {
                if (scriptingCodeEditor.Focused)
                    scriptingCodeEditor.Paste();
                else
                    mainFormView.SendStringAsKey("^V");
            }
            else
            {
                if (customFunctionsCodeEditor.Focused)
                    customFunctionsCodeEditor.Paste();
                else
                    mainFormView.SendStringAsKey("^V");
            }
        }
    }

    class SaveAsCommand : CommandBase
    {
        private ICanFileEdit scriptingCodeEditor;
        private ICanFileEdit customFunctionsCodeEditor;


        public SaveAsCommand(ICanFileEdit scriptingCodeEditor, ICanFileEdit customFunctionsCodeEditor)
        {
          //  this.Icon = Resources.save;
            this.Text = MenuStrings.saveAsToolStripMenuItem_Text;
            this.ToolTip = MenuStrings.saveAsToolStripMenuItem_Text;

            this.scriptingCodeEditor = scriptingCodeEditor;
            this.customFunctionsCodeEditor = customFunctionsCodeEditor;
            // this.mainFormView = mainFormView;
        }


        public override void Execute()
        {

            switch ((int)SharedViewState.Instance.CurrentView)
            {
                case 0:

                    //mainFormView.SendStringAsKey("^S");
                    break;

                case 4:
                    scriptingCodeEditor.SaveAs();
                    break;

                case 5:
                    customFunctionsCodeEditor.SaveAs();
                    break;

                default:
                    //mainFormView.SendStringAsKey("^S");
                    break;
            }
        }
    }

    class SaveCommand : CommandBase
    {
        private ICanFileEdit scriptingCodeEditor;
        private ICanFileEdit customFunctionsCodeEditor;


        public SaveCommand(ICanFileEdit scriptingCodeEditor, ICanFileEdit customFunctionsCodeEditor)
        {
            this.Icon = Resources.saveToolStripButtonImage;
            this.Text = MenuStrings.saveToolStripButton_Text;
            this.ToolTip = MenuStrings.saveToolStripButton_Text;

            this.scriptingCodeEditor = scriptingCodeEditor;
            this.customFunctionsCodeEditor = customFunctionsCodeEditor;
            // this.mainFormView = mainFormView;
        }


        public override void Execute()
        {

            switch ((int)SharedViewState.Instance.CurrentView)
            {
                case 0:

                    //mainFormView.SendStringAsKey("^S");
                    break;

                case 4:
                     scriptingCodeEditor.Save();
                    break;

                case 5:
                    customFunctionsCodeEditor.Save();
                    break;

                default:
                    //mainFormView.SendStringAsKey("^S");
                    break;
            }

           // mainFormView.SendStringAsKey("^S");
        }
    }

    class OpenCommand : CommandBase
    {
        private ICanFileEdit scriptingCodeEditor;
        private ICanFileEdit customFunctionsCodeEditor;


        public OpenCommand(ICanFileEdit scriptingCodeEditor, ICanFileEdit customFunctionsCodeEditor)
        {
            this.Icon = Resources.openToolStripButtonImage;
            this.Text = MenuStrings.openToolStripButton_Text;
            this.ToolTip = MenuStrings.openToolStripButton_Text;

            this.scriptingCodeEditor = scriptingCodeEditor;
            this.customFunctionsCodeEditor = customFunctionsCodeEditor;
            // this.mainFormView = mainFormView;
        }


        public override void Execute()
        {
            var ofd = new OpenFileDialog { Filter = GlobalConfig.tslFilesFIlter };//TODO: move this to mainView or something
            if (ofd.ShowDialog() != DialogResult.OK)
                return;



            switch ((int)SharedViewState.Instance.CurrentView)
            {
                case 0:

                    //mainFormView.SendStringAsKey("^S");
                    break;

                case 4:
                    scriptingCodeEditor.NewDocument(ofd.FileName);
                    break;

                case 5:
                    customFunctionsCodeEditor.NewDocument(ofd.FileName);
                    break;

                default:
                    //mainFormView.SendStringAsKey("^S");
                    break;
            }

            //mainFormView.SendStringAsKey("^S");
        }
    }

    class NewCommand : CommandBase
    {
        private ICanFileEdit scriptingCodeEditor;
        private ICanFileEdit customFunctionsCodeEditor;


        public NewCommand(ICanFileEdit scriptingCodeEditor, ICanFileEdit customFunctionsCodeEditor)
        {
            this.Icon = Resources.newToolStripButtonImage;
            this.Text = MenuStrings.newToolStripButton_Text;
            this.ToolTip = MenuStrings.newToolStripButton_Text;

            this.scriptingCodeEditor = scriptingCodeEditor;
            this.customFunctionsCodeEditor = customFunctionsCodeEditor;
           // this.mainFormView = mainFormView;
        }


        public override void Execute()
        {
            switch ((int)SharedViewState.Instance.CurrentView)
            {
                case 0:

                    //mainFormView.SendStringAsKey("^S");
                    break;

                case 4:
                    scriptingCodeEditor.NewDocument();
                    break;

                case 5:
                    customFunctionsCodeEditor.NewDocument();
                    break;

                default:
                    //mainFormView.SendStringAsKey("^S");
                    break;
            }

            //mainFormView.SendStringAsKey("^S");
        }
    }

    internal abstract class CommandBase : IToolbarCommand
    {
        private bool _checked;
        private bool _checkOnClick;

        private ToolStripItemImageScaling _imageScaling;
        private ToolStripItemDisplayStyle _displayStyle;

        private string _text;
        private Image icon;
        private bool isEnabled;
        private string toolTip;

        protected CommandBase()
        {
            isEnabled = true;
            _checked = _checkOnClick = false;
        }

        public string Text
        {
            get { return _text; }
            set
            {
                if (_text != value)
                {
                    _text = value;
                    OnPropertyChanged(nameof(Text));
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public abstract void Execute();

        public bool IsEnabled
        {
            get { return isEnabled; }
            set
            {
                if (isEnabled != value)
                {
                    isEnabled = value;
                    OnPropertyChanged("IsEnabled");
                }
            }
        }

        public Image Icon
        {
            get { return icon; }
            set
            {
                if (icon != value)
                {
                    icon = value;
                    OnPropertyChanged("Icon");
                }
            }
        }

        public string ToolTip
        {
            get { return toolTip; }
            set
            {
                if (toolTip != value)
                {
                    toolTip = value;
                    OnPropertyChanged("ToolTip");
                }
            }
        }

        public Keys ShortcutKey { get; set; }

        public bool Checked
        {
            get { return _checked; }
            set
            {
                if (_checked != value)
                {
                    _checked = value;
                    OnPropertyChanged(nameof(Checked));
                }
            }
        }

        public bool CheckOnClick
        {
            get { return _checkOnClick; }
            set
            {
                if (_checkOnClick != value)
                {
                    _checkOnClick = value;
                    OnPropertyChanged(nameof(CheckOnClick));
                }
            }
        }

        public IEnumerable<IToolbarCommand> ChildrenCommands { get; set; }

        public ToolStripItemImageScaling ImageScaling
        {
            get { return _imageScaling; }
            set
            {
                if (_imageScaling != value)
                {
                    _imageScaling = value;
                    OnPropertyChanged(nameof(ImageScaling));
                }
            }
        }

        public ToolStripItemDisplayStyle DisplayStyle
        {
            get { return _displayStyle; }
            set
            {
                if (_displayStyle != value)
                {
                    _displayStyle = value;
                    OnPropertyChanged(nameof(DisplayStyle));
                }
            }
        }

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}