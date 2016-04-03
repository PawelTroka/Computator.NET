using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Forms.Integration;
using Computator.NET.Compilation;
using Computator.NET.Config;
using Computator.NET.DataTypes.SettingsTypes;
using Computator.NET.Evaluation;
using Computator.NET.UI.Controls;
using File = System.IO.File;
using Settings = Computator.NET.Properties.Settings;

namespace Computator.NET.UI.CodeEditors
{
    public class CodeEditorControlWrapper : UserControl, ICodeEditorControl,
        INotifyPropertyChanged
    {
        private readonly ScriptEvaluator _eval;
        private readonly /*AvalonEditCodeEditorControl*/ AvalonEditCodeEditor avalonEditor;
        private readonly ElementHost avalonEditorWrapper;

        private readonly SaveFileDialog saveFileDialog = new SaveFileDialog
        {
            Filter = GlobalConfig.tslFilesFIlter
        };

        private readonly ScintillaCodeEditorControl scintillaEditor;
        private readonly DocumentsTabControl tabControl;
        private CodeEditorType _codeEditorType;

        //private Dictionary<CodeEditorType, ICodeEditorControl> codeEditors;

        public CodeEditorControlWrapper()
        {
            _eval = new ScriptEvaluator();
            avalonEditor = new AvalonEditCodeEditor();

            avalonEditorWrapper = new ElementHost
            {
                BackColor = Color.White,
                Dock = DockStyle.Fill
            };

            scintillaEditor = new ScintillaCodeEditorControl
            {
                Dock = DockStyle.Fill
            };
            avalonEditorWrapper.Child = avalonEditor;


            tabControl = new DocumentsTabControl { Dock = DockStyle.Top };

            var panel = new Panel { Dock = DockStyle.Fill };
            panel.Controls.AddRange(new Control[] { avalonEditorWrapper, scintillaEditor });

            var tableLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 2
            };

            tableLayout.Controls.Add(tabControl, 0, 0);
            tableLayout.Controls.Add(panel, 0, 1);
            Controls.Add(tableLayout);
            ChangeEditorType(true);
            SetFont(Settings.Default.ScriptingFont);

            tabControl.SelectedIndexChanged += TabControl_SelectedIndexChanged;
            tabControl.ControlRemoved += TabControl_ControlRemoved;
            tabControl.ControlAdded += TabControl_ControlAdded;

            
            scintillaEditor.DataBindings.Add("ExponentMode", this, "ExponentMode", false,
                DataSourceUpdateMode.OnPropertyChanged);


            //TODO: somehow manage to get avalonedit codeeditor ExponentMode bind to this ExponentMode property
            //avalonEditor.DataBindings.Add("ExponentMode", this, "ExponentMode", false, DataSourceUpdateMode.OnPropertyChanged);

            
        }


        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (keyData == (Keys.Control | Keys.T))
            {
                NewDocument();
                return true;
            }
            return base.ProcessCmdKey(ref msg, keyData);
        }

        public override bool Focused
            =>
                (_codeEditorType == CodeEditorType.AvalonEdit)
                    ? avalonEditorWrapper.Focused
                    : ((Control)(CurrentCodeEditor)).Focused;

        private ICodeEditorControl CurrentCodeEditor
        {
            get
            {
                switch (_codeEditorType)
                {
                    case CodeEditorType.AvalonEdit:
                        return avalonEditor;
                    case CodeEditorType.Scintilla:
                        return scintillaEditor;
                    default:
                        return null;
                }
            }
        }

        public string CurrentFileName
        {
            get { return tabControl.SelectedTab.Text; }
            set { tabControl.SelectedTab.Text = value; }
        }

        public void ClearHighlightedErrors()
        {
            CurrentCodeEditor.ClearHighlightedErrors();
        }

        public override string Text
        {
            get { return CurrentCodeEditor.Text; }
            set { CurrentCodeEditor.Text = value; }
        }

        public void Undo()
        {
            CurrentCodeEditor.Undo();
        }

        public void Redo()
        {
            CurrentCodeEditor.Redo();
        }

        public void Cut()
        {
            CurrentCodeEditor.Cut();
        }

        public void Paste()
        {
            CurrentCodeEditor.Paste();
        }

        public void Copy()
        {
            CurrentCodeEditor.Copy();
        }

        public void SelectAll()
        {
            CurrentCodeEditor.SelectAll();
        }

        public bool ExponentMode
        {
            get { return CurrentCodeEditor.ExponentMode; }
            set
            {
                if (CurrentCodeEditor.ExponentMode != value)
                {
                    CurrentCodeEditor.ExponentMode = value;
                    OnPropertyChanged(nameof(ExponentMode));
                }
            }
        }

        public void AppendText(string text)
        {
            CurrentCodeEditor.AppendText(text);
        }

        public void RenameDocument(string filename, string newFilename)
        {
            if (CurrentCodeEditor.ContainsDocument(filename))
            {
                CurrentCodeEditor.RenameDocument(filename, newFilename);
                tabControl.RenameTab(filename, newFilename);
            }
        }

        public bool ContainsDocument(string filename)
        {
            return CurrentCodeEditor.ContainsDocument(filename);
        }

        public void NewDocument(string filename = "")
        {
            //   if(string.IsNullOrEmpty(filename))
            tabControl.AddTab(filename);
            // else
            // this.CurrentCodeEditor.NewDocument(filename);
        }

        public void SwitchDocument(string filename)
        {
            CurrentCodeEditor.SwitchDocument(filename);
        }

        public void CloseDocument(string filename)
        {
            CurrentCodeEditor.CloseDocument(filename);
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public void Save()
        {
            if (!File.Exists(CurrentFileName))
            {
                saveFileDialog.FileName = CurrentFileName;
                if (saveFileDialog.ShowDialog() != DialogResult.OK) return;
                File.WriteAllText(saveFileDialog.FileName, Text);

                if (saveFileDialog.FileName != CurrentFileName)
                {
                    CurrentCodeEditor.RenameDocument(CurrentFileName, saveFileDialog.FileName);
                    CurrentFileName = saveFileDialog.FileName;
                }
                tabControl.SelectedTab.ImageIndex = 0;
            }
            else
            {
                File.WriteAllText(CurrentFileName, Text);
                tabControl.SelectedTab.ImageIndex = 0;
            }
        }

        public void SaveAs()
        {
            saveFileDialog.FileName = CurrentFileName;

            if (saveFileDialog.ShowDialog() != DialogResult.OK)
                return;

            File.WriteAllText(saveFileDialog.FileName, Text);

            if (saveFileDialog.FileName != CurrentFileName)
            {
                CurrentCodeEditor.RenameDocument(tabControl.SelectedTab.Text, saveFileDialog.FileName);
                CurrentFileName = saveFileDialog.FileName;
            }
            tabControl.SelectedTab.ImageIndex = 0;
        }

        private void TabControl_ControlAdded(object sender, ControlEventArgs e)
        {
            // throw new System.NotImplementedException();
            //(e.Control as TabPage).ImageIndex = 0;
        }

        private void TabControl_ControlRemoved(object sender, ControlEventArgs e)
        {
            // if (_codeEditorType == CodeEditorType.Scintilla)
            {
                CloseDocument(e.Control.Text);
                //MessageBox.Show("Chuj!!!"+ e.Control.Text);
            }
        }

        private void TabControl_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (tabControl.SelectedIndex < tabControl.TabPages.Count - 1)
            {
                if (ContainsDocument(tabControl.SelectedTab.Text))
                    SwitchDocument(tabControl.SelectedTab.Text);
                else
                {
                    CurrentCodeEditor.NewDocument(tabControl.SelectedTab.Text);
                }
            }
        }

        public void SwitchTab(string tabName)
        {
            foreach (TabPage tabPage in tabControl.TabPages)
            {
                if (tabPage.Text == tabName)
                {
                    tabControl.SelectedTab = tabPage;
                }
            }
        }

        public void RemoveTab(string tabName)
        {
            foreach (TabPage tabPage in tabControl.TabPages)
            {
                if (tabPage.Text == tabName)
                {
                    tabControl.TabPages.Remove(tabPage);
                }
            }
        }

        public void SetFont(Font font)
        {
            avalonEditor.SetFont(font);
            scintillaEditor.SetFont(font);
        }

        public void ProcessScript(RichTextBox things, string customCode = "")
        {
            ClearHighlightedErrors();
            var function = _eval.Evaluate(CurrentCodeEditor.Text, customCode);
            function.Evaluate(things);
        }

        public void HighlightErrors(List<CompilerError> errors)
        {
            CurrentCodeEditor.HighlightErrors(errors);
        }

        public IEnumerable<string> Documents { get { return CurrentCodeEditor.Documents; } }

        public void ChangeEditorType(bool firstTime=false)
        {
            if (_codeEditorType == Settings.Default.CodeEditor && !firstTime) return;

            var documents = new Dictionary<string, string>();

            foreach (var document in Documents)
            {
                SwitchDocument(document);
                documents.Add(document, Text);
                
            }

            _codeEditorType = Settings.Default.CodeEditor;

            switch (_codeEditorType)
            {
                case CodeEditorType.AvalonEdit:
                    //  avalonEditor.Text = scintillaEditor.Text;
                    avalonEditorWrapper.Show();
                    scintillaEditor.Hide();
                    break;
                case CodeEditorType.Scintilla:
                    // scintillaEditor.Text = avalonEditor.Text;
                    avalonEditorWrapper.Hide();
                    scintillaEditor.Show();
                    break;
            }

            foreach (var document in documents)
            {
                SwitchDocument(document.Key);
                Text = document.Value;
               // MessageBox.Show(Text);
            }
        }


        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}