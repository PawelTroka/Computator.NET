using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using System.Windows.Forms.Integration;
using Computator.NET.DataTypes.SettingsTypes;
using Computator.NET.Evaluation;
using Computator.NET.Properties;
using Computator.NET.UI.Controls;

namespace Computator.NET.UI.CodeEditors
{
    internal class CodeEditorControlWrapper : UserControl, ICodeEditorControl, INotifyPropertyChanged
    {
        private readonly ScriptEvaluator _eval;
        private readonly /*AvalonEditCodeEditorControl*/ AvalonEditCodeEditor avalonEditor;
        private readonly ElementHost avalonEditorWrapper;
        private readonly ScintillaCodeEditorControl scintillaEditor;
        private readonly DocumentsTabControl tabControl;
        private CodeEditorType _codeEditorType;
        private Dictionary<CodeEditorType, ICodeEditorControl> codeEditors;

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


            tabControl = new DocumentsTabControl {Dock = DockStyle.Top};

            var panel = new Panel {Dock = DockStyle.Fill};
            panel.Controls.AddRange(new Control[] {avalonEditorWrapper, scintillaEditor});

            var tableLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 2
            };

            tableLayout.Controls.Add(tabControl, 0, 0);
            tableLayout.Controls.Add(panel, 0, 1);
            Controls.Add(tableLayout);
            ChangeEditorType();
            SetFont(Settings.Default.ScriptingFont);

            tabControl.SelectedIndexChanged += TabControl_SelectedIndexChanged;
            tabControl.ControlRemoved += TabControl_ControlRemoved;
            tabControl.ControlAdded += TabControl_ControlAdded;
        }

        public override bool Focused
            =>
                (_codeEditorType == CodeEditorType.AvalonEdit)
                    ? avalonEditorWrapper.Focused
                    : ((Control) (CurrentCodeEditor)).Focused;

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

        public string SaveAs(string filename = "")
        {
            var str = CurrentCodeEditor.SaveAs(tabControl.SelectedTab.Text);

            if (!string.IsNullOrEmpty(str))
            {
                tabControl.SelectedTab.Text = str;
                tabControl.SelectedTab.ImageIndex = 0;
            }
            return str;
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

        public string SaveDocument(string filename = "")
        {
            //throw new System.NotImplementedException();

            var str = CurrentCodeEditor.SaveDocument(tabControl.SelectedTab.Text);

            if (!string.IsNullOrEmpty(str))
            {
                tabControl.SelectedTab.Text = str;
                tabControl.SelectedTab.ImageIndex = 0;
            }
            return str;
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

        public void SetFont(Font font)
        {
            avalonEditor.SetFont(font);
            scintillaEditor.SetFont(font);
        }

        public void ProcessScript(RichTextBox things, string customCode = "")
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
            _codeEditorType = Settings.Default.CodeEditor;
            switch (_codeEditorType)
            {
                case CodeEditorType.AvalonEdit:


                    //////////////
                    avalonEditor.Text = scintillaEditor.Text;
                    /////////////


                    avalonEditorWrapper.Show();
                    scintillaEditor.Hide();
                    break;
                case CodeEditorType.Scintilla:
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
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}