namespace Computator.NET.UI.CodeEditors
{
    public enum TSLMode
    {
        Scripting,
        Functions
    }

    internal class CodeEditorControlWrapper : System.Windows.Forms.UserControl, ICodeEditorControl,
        System.ComponentModel.INotifyPropertyChanged
    {
        private readonly Evaluation.ScriptEvaluator _eval;
        private readonly /*AvalonEditCodeEditorControl*/ AvalonEditCodeEditor avalonEditor;
        private readonly System.Windows.Forms.Integration.ElementHost avalonEditorWrapper;

        private readonly System.Windows.Forms.SaveFileDialog saveFileDialog = new System.Windows.Forms.SaveFileDialog
        {
            Filter = Config.GlobalConfig.tslFilesFIlter
        };

        private readonly ScintillaCodeEditorControl scintillaEditor;
        private readonly Controls.DocumentsTabControl tabControl;
        private DataTypes.SettingsTypes.CodeEditorType _codeEditorType;

        private System.Collections.Generic.Dictionary<DataTypes.SettingsTypes.CodeEditorType, ICodeEditorControl>
            codeEditors;

        public CodeEditorControlWrapper()
        {
            _eval = new Evaluation.ScriptEvaluator();
            avalonEditor = new AvalonEditCodeEditor();

            avalonEditorWrapper = new System.Windows.Forms.Integration.ElementHost
            {
                BackColor = System.Drawing.Color.White,
                Dock = System.Windows.Forms.DockStyle.Fill
            };

            scintillaEditor = new ScintillaCodeEditorControl
            {
                Dock = System.Windows.Forms.DockStyle.Fill
            };
            avalonEditorWrapper.Child = avalonEditor;


            tabControl = new Controls.DocumentsTabControl {Dock = System.Windows.Forms.DockStyle.Top};

            var panel = new System.Windows.Forms.Panel {Dock = System.Windows.Forms.DockStyle.Fill};
            panel.Controls.AddRange(new System.Windows.Forms.Control[] {avalonEditorWrapper, scintillaEditor});

            var tableLayout = new System.Windows.Forms.TableLayoutPanel
            {
                Dock = System.Windows.Forms.DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 2
            };

            tableLayout.Controls.Add(tabControl, 0, 0);
            tableLayout.Controls.Add(panel, 0, 1);
            Controls.Add(tableLayout);
            ChangeEditorType();
            SetFont(Properties.Settings.Default.ScriptingFont);

            tabControl.SelectedIndexChanged += TabControl_SelectedIndexChanged;
            tabControl.ControlRemoved += TabControl_ControlRemoved;
            tabControl.ControlAdded += TabControl_ControlAdded;
        }

        public override bool Focused
            =>
                (_codeEditorType == DataTypes.SettingsTypes.CodeEditorType.AvalonEdit)
                    ? avalonEditorWrapper.Focused
                    : ((System.Windows.Forms.Control) (CurrentCodeEditor)).Focused;

        private ICodeEditorControl CurrentCodeEditor
        {
            get
            {
                switch (_codeEditorType)
                {
                    case DataTypes.SettingsTypes.CodeEditorType.AvalonEdit:
                        return avalonEditor;
                    case DataTypes.SettingsTypes.CodeEditorType.Scintilla:
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
            (CurrentCodeEditor).RenameDocument(filename, newFilename);
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

        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;

        public void Save()
        {
            if (!System.IO.File.Exists(CurrentFileName))
            {
                saveFileDialog.FileName = CurrentFileName;
                if (saveFileDialog.ShowDialog() != System.Windows.Forms.DialogResult.OK) return;
                System.IO.File.WriteAllText(saveFileDialog.FileName, Text);

                if (saveFileDialog.FileName != CurrentFileName)
                {
                    CurrentCodeEditor.RenameDocument(CurrentFileName, saveFileDialog.FileName);
                    CurrentFileName = saveFileDialog.FileName;
                }
                tabControl.SelectedTab.ImageIndex = 0;
            }
            else
            {
                System.IO.File.WriteAllText(CurrentFileName, Text);
                tabControl.SelectedTab.ImageIndex = 0;
            }
        }

        public void SaveAs()
        {
            saveFileDialog.FileName = CurrentFileName;

            if (saveFileDialog.ShowDialog() != System.Windows.Forms.DialogResult.OK)
                return;

            System.IO.File.WriteAllText(saveFileDialog.FileName, Text);

            if (saveFileDialog.FileName != CurrentFileName)
            {
                CurrentCodeEditor.RenameDocument(tabControl.SelectedTab.Text, saveFileDialog.FileName);
                CurrentFileName = saveFileDialog.FileName;
            }
            tabControl.SelectedTab.ImageIndex = 0;
        }

        private void TabControl_ControlAdded(object sender, System.Windows.Forms.ControlEventArgs e)
        {
            // throw new System.NotImplementedException();
            //(e.Control as TabPage).ImageIndex = 0;
        }

        private void TabControl_ControlRemoved(object sender, System.Windows.Forms.ControlEventArgs e)
        {
            // if (_codeEditorType == CodeEditorType.Scintilla)
            {
                CloseDocument(e.Control.Text);
                //MessageBox.Show("Chuj!!!"+ e.Control.Text);
            }
        }

        private void TabControl_SelectedIndexChanged(object sender, System.EventArgs e)
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

        public void SetFont(System.Drawing.Font font)
        {
            avalonEditor.SetFont(font);
            scintillaEditor.SetFont(font);
        }

        public void ProcessScript(System.Windows.Forms.RichTextBox things, string customCode = "")
        {
            //  try
            //  {
            var function = _eval.Evaluate(CurrentCodeEditor.Text, customCode);
            function.Evaluate(things);
            //   }
            /*   catch (Exception ex2)
            {
                var sb = new StringBuilder();
                sb.AppendLine(ex2.Message);
                if (ex2.InnerException == null) throw new Exception(sb.ToString(), ex2);
                sb.AppendLine(ex2.InnerException.Message);
                if (ex2.InnerException.InnerException == null) throw new Exception(sb.ToString(), ex2);
                sb.AppendLine(ex2.InnerException.InnerException.Message);
                if (ex2.InnerException.InnerException != null)
                    sb.AppendLine(ex2.InnerException.InnerException.Message);
                //MessageBox.Show(sb.ToString(), "Error");
                throw new Exception(sb.ToString(), ex2);
            }*/
        }

        public void ChangeEditorType()
        {
            _codeEditorType = Properties.Settings.Default.CodeEditor;
            switch (_codeEditorType)
            {
                case DataTypes.SettingsTypes.CodeEditorType.AvalonEdit:


                    //////////////
                    avalonEditor.Text = scintillaEditor.Text;
                    /////////////


                    avalonEditorWrapper.Show();
                    scintillaEditor.Hide();
                    break;
                case DataTypes.SettingsTypes.CodeEditorType.Scintilla:
                    ////////////////////
                    scintillaEditor.Text = avalonEditor.Text;
                    ///////////////////
                    avalonEditorWrapper.Hide();
                    scintillaEditor.Show();
                    break;
            }
        }

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new System.ComponentModel.PropertyChangedEventArgs(propertyName));
        }
    }
}