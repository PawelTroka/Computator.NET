using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Forms;
using Computator.NET.Core.Abstract.Controls;
using Computator.NET.Core.Properties;
using Computator.NET.DataTypes.Configuration;
using Computator.NET.DataTypes.SettingsTypes;
using Computator.NET.Desktop.Controls.CodeEditors.AvalonEdit;

namespace Computator.NET.Desktop.Controls.CodeEditors
{
    public class CodeEditorControlWrapper : UserControl, ICodeDocumentsEditor

    {
        private readonly Dictionary<CodeEditorType, ICodeEditorControl> _codeEditors;
        #if !__MonoCS__
        private readonly System.Windows.Forms.Integration.ElementHost avalonEditorWrapper;
        #endif
        private readonly SaveFileDialog saveFileDialog = new SaveFileDialog
        {
            Filter = HandledFilesInformation.TslFilesFIlter
        };

        private readonly DocumentsTabControl tabControl;
        private CodeEditorType _codeEditorType;
        private TabPage _activeTab;
        private bool _restoringTabs;


        public CodeEditorControlWrapper(
#if !__MonoCS__
            ScintillaCodeEditorControl scintillaCodeEditorControl,
            AvalonEditCodeEditor avalonEditCodeEditor,
#endif
            TextEditorCodeEditor textEditorCodeEditor)
        {
            _codeEditors = new Dictionary
                <CodeEditorType, ICodeEditorControl>
            {
#if !__MonoCS__
                {CodeEditorType.Scintilla, scintillaCodeEditorControl},
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
            panel.Controls.AddRange(new[]
            {
                        #if !__MonoCS__
                avalonEditorWrapper,
                _codeEditors[CodeEditorType.Scintilla] as Control,
#endif
                 _codeEditors[CodeEditorType.TextEditor] as Control
            });

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
            SetEditorVisibility(Settings.Default.CodeEditor);
            SetFont(Settings.Default.ScriptingFont);
            tabControl.SelectedIndexChanged += TabControl_SelectedIndexChanged;
            tabControl.ControlRemoved += TabControl_ControlRemoved;
            tabControl.ControlAdded += TabControl_ControlAdded;

            NewDocument();


            Settings.Default.PropertyChanged += Settings_PropertyChanged;
        }

        private void Settings_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
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
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
                Settings.Default.PropertyChanged -= Settings_PropertyChanged;
            base.Dispose(disposing);
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

        [System.ComponentModel.DesignerSerializationVisibility(System.ComponentModel.DesignerSerializationVisibility.Hidden)]
        public string CurrentFileName
        {
            get { return tabControl.SelectedTab.Text; }
            set { RenameDocument(CurrentFileName, value); }
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
            var documentTab = FindDocumentTab(filename);
            if (documentTab == null || !CurrentCodeEditor.ContainsDocument(documentTab.Text)) return;

            newFilename = DocumentFilePath.Normalize(newFilename);
            EnsureCanRenameDocument(documentTab.Text, newFilename);
            var oldFilename = documentTab.Text;
            CurrentCodeEditor.RenameDocument(oldFilename, newFilename);
            tabControl.RenameTab(oldFilename, newFilename);
        }

        public void EnsureCanRenameDocument(string filename, string newFilename)
        {
            newFilename = DocumentFilePath.Normalize(newFilename);
            var sourceTab = FindDocumentTab(filename);
            var destinationTab = FindDocumentTab(newFilename);
            if (destinationTab != null && destinationTab != sourceTab)
                throw new IOException("This file is already open in another tab. Switch to that tab or choose a different file name.");
        }

        public bool ContainsDocument(string filename)
        {
            var documentTab = FindDocumentTab(filename);
            return documentTab != null && CurrentCodeEditor.ContainsDocument(documentTab.Text);
        }

        public void NewDocument(string filename)
        {
            if (string.IsNullOrEmpty(filename))
            {
                tabControl.AddTab();
                return;
            }

            filename = DocumentFilePath.Normalize(filename);
            var existingTab = FindDocumentTab(filename);
            if (existingTab != null)
            {
                SwitchTab(existingTab.Text);
                return;
            }

            // Finish disk I/O before committing a tab or touching an active
            // buffer. A locked or unreadable file cannot become Save's target.
            var text = File.Exists(filename) ? File.ReadAllText(filename) : string.Empty;
            var previousTab = tabControl.SelectedTab;
            try
            {
                CurrentCodeEditor.NewDocument(filename, text);
                tabControl.AddTab(filename);
            }
            catch
            {
                RestoreAfterFailedOpen(FindDocumentTab(filename), previousTab, filename);
                throw;
            }
        }

        private TabPage FindDocumentTab(string filename)
        {
            if (string.IsNullOrEmpty(filename)) return null;
            foreach (TabPage documentTab in tabControl.TabPages)
                if (IsDocumentTab(documentTab) && string.Equals(documentTab.Text, filename, StringComparison.OrdinalIgnoreCase))
                    return documentTab;

            if (!DocumentFilePath.TryNormalize(filename, out var fullPath)) return null;
            foreach (TabPage documentTab in tabControl.TabPages)
                if (documentTab.Tag is string documentPath &&
                    string.Equals(documentPath, fullPath, StringComparison.OrdinalIgnoreCase))
                    return documentTab;
            return null;
        }

        private bool IsDocumentTab(TabPage tab)
        {
            return tab != null && tab.Name != "addTabPage" && tabControl.TabPages.Contains(tab);
        }

        private void RestoreAfterFailedOpen(TabPage failedTab, TabPage previousTab, string failedFilename)
        {
            _restoringTabs = true;
            try
            {
                CurrentCodeEditor.CloseDocument(failedFilename);
                if (failedTab != null && failedTab != previousTab) tabControl.TabPages.Remove(failedTab);
                if (IsDocumentTab(previousTab) && CurrentCodeEditor.ContainsDocument(previousTab.Text))
                {
                    tabControl.SelectedTab = previousTab;
                    CurrentCodeEditor.SwitchDocument(previousTab.Text);
                    _activeTab = previousTab;
                }
            }
            finally
            {
                _restoringTabs = false;
            }
        }

        public void HighlightErrors(IEnumerable<CompilerError> errors)
        {
            CurrentCodeEditor.HighlightErrors(errors);
        }

        public void SwitchTab(string tabName)
        {
            var documentTab = FindDocumentTab(tabName);
            if (documentTab == null) return;
            tabControl.SelectedTab = documentTab;
            CurrentCodeEditor.SwitchDocument(documentTab.Text);
            _activeTab = documentTab;
        }

        public void RemoveTab(string tabName)
        {
            var documentTab = FindDocumentTab(tabName);
            if (documentTab != null) tabControl.TabPages.Remove(documentTab);
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
                CurrentFileName = saveFileDialog.FileName;
            }
            tabControl.SelectedTab.ImageIndex = 0;
        }

        public void AppendText(string text)
        {
            CurrentCodeEditor.AppendText(text);
        }

        public void SwitchDocument(string filename)
        {
            SwitchTab(filename);
        }

        public void CloseDocument(string filename)
        {
            RemoveTab(filename);
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
            if (_restoringTabs) return;
            CurrentCodeEditor.CloseDocument(e.Control.Text);

            // Editor implementations choose their own surviving document on close.
            // The selected tab owns the filename used by Save, so restore its
            // document even when removing a background tab raised no selection event.
            var selectedTab = tabControl.SelectedTab;
            if (selectedTab != null && selectedTab != e.Control &&
                CurrentCodeEditor.ContainsDocument(selectedTab.Text))
            {
                CurrentCodeEditor.SwitchDocument(selectedTab.Text);
                _activeTab = selectedTab;
            }
        }

        private void TabControl_SelectedIndexChanged(object sender, EventArgs e)
        {
            var selectedTab = tabControl.SelectedTab;
            if (_restoringTabs || !IsDocumentTab(selectedTab)) return;
            try
            {
                if (CurrentCodeEditor.ContainsDocument(selectedTab.Text))
                    CurrentCodeEditor.SwitchDocument(selectedTab.Text);
                else
                {
                    // Tabs created by the new-tab button have no disk identity,
                    // even if a same-named file exists in the working directory.
                    var text = selectedTab.Tag is string path && File.Exists(path)
                        ? File.ReadAllText(path) : string.Empty;
                    CurrentCodeEditor.NewDocument(selectedTab.Text, text);
                }
                _activeTab = selectedTab;
            }
            catch
            {
                RestoreAfterFailedOpen(selectedTab, _activeTab, selectedTab.Text);
                throw;
            }
        }

        public void SetFont(Font font)
        {
            foreach (var codeEditorControl in _codeEditors)
            {
                codeEditorControl.Value.SetFont(font);
            }
        }

        public void ChangeEditorType()
        {
            if (_codeEditorType == Settings.Default.CodeEditor) return;

            var currentDocument = CurrentFileName;
            var previousKind = _codeEditorType;
            var previousEditor = CurrentCodeEditor;
            var nextKind = Settings.Default.CodeEditor;
            var nextEditor = _codeEditors[nextKind];
            var documents = new Dictionary<string, string>();

            try
            {
                foreach (var document in previousEditor.Documents)
                {
                    previousEditor.SwitchDocument(document);
                    documents.Add(document, previousEditor.Text);
                }
            }
            finally
            {
                previousEditor.SwitchDocument(currentDocument);
            }

            if (nextEditor.Documents.Any())
                throw new InvalidOperationException("The inactive editor still contains documents; their contents have been preserved.");

            try
            {
                foreach (var document in documents)
                {
                    // Never reload files while moving unsaved buffers between
                    // implementations. Files can be locked, replaced or deleted.
                    nextEditor.NewDocument(document.Key, document.Value);
                    if (nextEditor.Text != document.Value)
                        throw new InvalidOperationException("The editor could not preserve the document text.");
                }
                nextEditor.SwitchDocument(currentDocument);
                SetEditorVisibility(nextKind);
            }
            catch
            {
                foreach (var document in documents.Keys)
                    nextEditor.CloseDocument(document);
                SetEditorVisibility(previousKind);
                previousEditor.SwitchDocument(currentDocument);
                throw;
            }

            // Retire the source registry only after the complete target is ready.
            foreach (var document in documents.Keys)
                previousEditor.CloseDocument(document);
        }

        private void SetEditorVisibility(CodeEditorType editorType)
        {
            _codeEditorType = editorType;

            switch (_codeEditorType)
            {
#if !__MonoCS__
                case CodeEditorType.AvalonEdit:
                    avalonEditorWrapper.Show();
                    (_codeEditors[CodeEditorType.Scintilla] as Control).Hide();
                    (_codeEditors[CodeEditorType.TextEditor] as Control).Hide();
                    break;

                case CodeEditorType.Scintilla:
                    avalonEditorWrapper.Hide();
                    (_codeEditors[CodeEditorType.Scintilla] as Control).Show();
                    (_codeEditors[CodeEditorType.TextEditor] as Control).Hide();
                    break;
#endif
                case CodeEditorType.TextEditor:
#if !__MonoCS__
                    avalonEditorWrapper.Hide();
                    (_codeEditors[CodeEditorType.Scintilla] as Control).Hide();
#endif
                    (_codeEditors[CodeEditorType.TextEditor] as Control).Show();
                    break;
            }
        }
    }
}
