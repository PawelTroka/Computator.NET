using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Windows;
using System.Windows.Forms;
using Computator.NET.Controls.CodeEditors.AvalonEdit;
using Computator.NET.Controls.CodeEditors.Scintilla;
using Computator.NET.Core.Abstract.Controls;
using Computator.NET.Core.Properties;
using Computator.NET.DataTypes;
using Computator.NET.DataTypes.SettingsTypes;

namespace Computator.NET.Controls.CodeEditors
{
    public class CodeEditorControlWrapper : UserControl, ICodeDocumentsEditor

    {
        private readonly Dictionary<CodeEditorType, ICodeEditorControl> _codeEditors;
        #if !__MonoCS__
        private readonly System.Windows.Forms.Integration.ElementHost avalonEditorWrapper;
        #endif
        private readonly SaveFileDialog saveFileDialog = new SaveFileDialog
        {
            Filter = GlobalConfig.TslFilesFIlter
        };

        private readonly DocumentsTabControl tabControl;
        private CodeEditorType _codeEditorType;


        public CodeEditorControlWrapper(ScintillaCodeEditorControl scintillaCodeEditorControl,
#if !__MonoCS__
            AvalonEditCodeEditor avalonEditCodeEditor,
#endif
            TextEditorCodeEditor textEditorCodeEditor)
        {
            scintillaCodeEditorControl.Dock=DockStyle.Fill;

            _codeEditors = new Dictionary
                <CodeEditorType, ICodeEditorControl>
            {
                {
                    CodeEditorType.Scintilla, scintillaCodeEditorControl
                },
#if !__MonoCS__
                {CodeEditorType.AvalonEdit, avalonEditCodeEditor},
#endif
                {CodeEditorType.TextEditor, textEditorCodeEditor }
            };
#if !__MonoCS__
            avalonEditorWrapper = new System.Windows.Forms.Integration.ElementHost
            {
                BackColor = Color.White,
                Dock = DockStyle.Fill,
                Child = _codeEditors[CodeEditorType.AvalonEdit] as UIElement
            };
#endif

            tabControl = new DocumentsTabControl {Dock = DockStyle.Top, AutoSize = true};
            

            var panel = new Panel {Dock = DockStyle.Fill};
            panel.Controls.AddRange(new[] {avalonEditorWrapper, _codeEditors[CodeEditorType.Scintilla] as Control, _codeEditors[CodeEditorType.TextEditor] as Control});

            var tableLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 2,
                AutoSize = true,
            };
            AutoSize = true;
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
#if !__MonoCS__
                _codeEditorType == CodeEditorType.AvalonEdit
                    ? avalonEditorWrapper.Focused
                    :
#endif
            ((Control) CurrentCodeEditor).Focused;

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
#if !__MonoCS__
                case CodeEditorType.AvalonEdit:
                    //  avalonEditor.Text = (_codeEditors[CodeEditorType.Scintilla] as Control).Text;
                    avalonEditorWrapper.Show();
                    (_codeEditors[CodeEditorType.Scintilla] as Control).Hide();
                    (_codeEditors[CodeEditorType.TextEditor] as Control).Hide();
                    break;
#endif
                case CodeEditorType.Scintilla:
                    avalonEditorWrapper.Hide();
                    (_codeEditors[CodeEditorType.Scintilla] as Control).Show();
                    (_codeEditors[CodeEditorType.TextEditor] as Control).Hide();
                    break;
                case CodeEditorType.TextEditor:
                    avalonEditorWrapper.Hide();
                    (_codeEditors[CodeEditorType.Scintilla] as Control).Hide();
                    (_codeEditors[CodeEditorType.TextEditor] as Control).Show();
                    break;
            }
        }
    }
}