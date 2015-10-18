using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Windows.Forms;
using Computator.NET.DataTypes.SettingsTypes;
using Computator.NET.Evaluation;

namespace Computator.NET.UI.CodeEditors
{
    class CodeEditorControlWrapper : Control
    {
        private AvalonEditCodeEditorControl avalonEditor;
        private ScintillaCodeEditorControl scintillaEditor;

        private Dictionary<CodeEditorType, ICodeEditorControl> codeEditors;
        private readonly ScriptEvaluator _eval;
        private CodeEditorType _codeEditorType;

        public CodeEditorControlWrapper()
        {

            _eval = new ScriptEvaluator();
            avalonEditor = new AvalonEditCodeEditorControl()
            { Dock = DockStyle.Fill };

            scintillaEditor = new ScintillaCodeEditorControl()
            {
                Dock = DockStyle.Fill
            };
            this.Controls.Add(avalonEditor);
            this.Controls.Add(scintillaEditor);

            ChangeEditorType();
            //this.SizeChanged += (o,e) => scintillaEditor.RefreshSize();
            //  Properties.Settings.Default.PropertyChanged += (o, e) => { if (e.PropertyName == "") ChangeEditorType(); };
            SetFont(Properties.Settings.Default.ScriptingFont);       
        }

        public override bool Focused => (CurrentCodeEditor as Control).Focused;

        public void Undo()
        {
            this.CurrentCodeEditor.Undo();
        }

        public void Redo()
        {
            this.CurrentCodeEditor.Redo();
        }

        public void Cut()
        {
            this.CurrentCodeEditor.Cut();
        }

        public void Paste()
        {
            this.CurrentCodeEditor.Paste();
        }

        public void Copy()
        {
            this.CurrentCodeEditor.Copy();
        }

        public void SelectAll()
        {
            this.CurrentCodeEditor.SelectAll();
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

        public override string Text
        {
            get { return CurrentCodeEditor.Text; }
            set { CurrentCodeEditor.Text = value; }
        }

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

        public void ChangeEditorType()
        {
            _codeEditorType = Properties.Settings.Default.CodeEditor;
            switch (_codeEditorType)
            {
                case CodeEditorType.AvalonEdit:

                    avalonEditor.Text = scintillaEditor.Text;
                    avalonEditor.Show();
                    scintillaEditor.Hide();
                    break;
                case CodeEditorType.Scintilla:
                    scintillaEditor.Text = avalonEditor.Text;
                    avalonEditor.Hide();
                    scintillaEditor.Show();
                    break;
            }
        }
    }
}
