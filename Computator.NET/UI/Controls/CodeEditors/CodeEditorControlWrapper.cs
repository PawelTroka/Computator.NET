using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Forms.Integration;
using Computator.NET.DataTypes;
using Computator.NET.DataTypes.SettingsTypes;
using Computator.NET.Properties;
using Computator.NET.UI.Controls.CodeEditors.AvalonEdit;
using Computator.NET.UI.Controls.CodeEditors.Scintilla;

namespace Computator.NET.UI.Controls.CodeEditors
{
    public class CodeEditorControlWrapper : UserControl, ICodeDocumentsEditor, ICanFileEdit,
        INotifyPropertyChanged
    {
        private readonly Dictionary<CodeEditorType, ICodeEditorControl> _codeEditors = new Dictionary
            <CodeEditorType, ICodeEditorControl>
        {
            {
                CodeEditorType.Scintilla, new ScintillaCodeEditorControl
                {
                    Dock = DockStyle.Fill
                }
            },
            {CodeEditorType.AvalonEdit, new AvalonEditCodeEditor()}
        };


        private readonly ElementHost avalonEditorWrapper;

        private readonly SaveFileDialog saveFileDialog = new SaveFileDialog
        {
            Filter = GlobalConfig.tslFilesFIlter
        };

        private readonly DocumentsTabControl tabControl;
        private CodeEditorType _codeEditorType;

        public CodeEditorControlWrapper()
        {
            avalonEditorWrapper = new ElementHost
            {
                BackColor = Color.White,
                Dock = DockStyle.Fill,
                Child = _codeEditors[CodeEditorType.AvalonEdit] as UIElement
            };


            tabControl = new DocumentsTabControl {Dock = DockStyle.Top};

            var panel = new Panel {Dock = DockStyle.Fill};
            panel.Controls.AddRange(new[] {avalonEditorWrapper, _codeEditors[CodeEditorType.Scintilla] as Control});

            var tableLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 2
            };

            tableLayout.Controls.Add(tabControl, 0, 0);
            tableLayout.Controls.Add(panel, 0, 1);
            Controls.Add(tableLayout);
            SetEditorVisibility();
            SetFont(Settings.Default.ScriptingFont);

            tabControl.SelectedIndexChanged += TabControl_SelectedIndexChanged;
            tabControl.ControlRemoved += TabControl_ControlRemoved;
            tabControl.ControlAdded += TabControl_ControlAdded;

            NewDocument();


            Settings.Default.PropertyChanged += (o, e) =>
            {
                switch (e.PropertyName)
                {
                    case "CodeEditor":
                        ChangeEditorType();
                        break;


                    case "ScriptingFont":
                        SetFont(Settings.Default.ScriptingFont);

                        break;
                }
            };
        }

        private ICodeEditorControl CurrentCodeEditor
        {
            get { return _codeEditors[_codeEditorType]; }
        }

        public void NewDocument()
        {
            NewDocument("");
        }

        public override bool Focused
            =>
                _codeEditorType == CodeEditorType.AvalonEdit
                    ? avalonEditorWrapper.Focused
                    : ((Control) CurrentCodeEditor).Focused;

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

        public void NewDocument(string filename)
        {
            //   if(string.IsNullOrEmpty(filename))
            tabControl.AddTab(filename);
            // else
            // this.CurrentCodeEditor.NewDocument(filename);
        }

        public void HighlightErrors(IEnumerable<CompilerError> errors)
        {
            CurrentCodeEditor.HighlightErrors(errors);
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

        public event PropertyChangedEventHandler PropertyChanged;

        public void AppendText(string text)
        {
            CurrentCodeEditor.AppendText(text);
        }

        public void SwitchDocument(string filename)
        {
            CurrentCodeEditor.SwitchDocument(filename);
        }

        public void CloseDocument(string filename)
        {
            CurrentCodeEditor.CloseDocument(filename);
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

        public void SetFont(Font font)
        {
            foreach (var codeEditorControl in _codeEditors)
            {
                codeEditorControl.Value.SetFont(font);
            }
        }

        public void ChangeEditorType() //TODO: test
        {
            if (_codeEditorType == Settings.Default.CodeEditor) return;

            var currentDocument = CurrentFileName;

            var documents = new Dictionary<string, string>();

            foreach (var document in CurrentCodeEditor.Documents)
            {
                SwitchDocument(document);
                documents.Add(document, Text);
                CloseDocument(document);
            }

            SetEditorVisibility();

            foreach (var document in documents)
            {
                if (CurrentCodeEditor.ContainsDocument(document.Key))
                    SwitchDocument(document.Key);
                else
                    CurrentCodeEditor.NewDocument(document.Key);

                Text = document.Value;
                // MessageBox.Show(Text);
            }

            SwitchDocument(currentDocument);
        }

        private void SetEditorVisibility()
        {
            _codeEditorType = Settings.Default.CodeEditor;

            switch (_codeEditorType)
            {
                case CodeEditorType.AvalonEdit:
                    //  avalonEditor.Text = (_codeEditors[CodeEditorType.Scintilla] as Control).Text;
                    avalonEditorWrapper.Show();
                    (_codeEditors[CodeEditorType.Scintilla] as Control).Hide();
                    break;
                case CodeEditorType.Scintilla:
                    // (_codeEditors[CodeEditorType.Scintilla] as Control).Text = avalonEditor.Text;
                    avalonEditorWrapper.Hide();
                    (_codeEditors[CodeEditorType.Scintilla] as Control).Show();
                    break;
            }
        }


        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}