using System;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Windows.Forms;
using Computator.NET.Localization;

namespace Computator.NET.Desktop.Controls.CodeEditors
{
    internal sealed class FindReplaceDialog : Form
    {
        private readonly Func<CodeEditorSearchTarget> _target;
        private readonly Func<string> _documentName;
        private readonly TextBox _query = new TextBox { Name = "findText", Dock = DockStyle.Fill };
        private readonly TextBox _replacement = new TextBox { Name = "replacementText", Dock = DockStyle.Fill };
        private readonly CheckBox _matchCase = new CheckBox { Name = "matchCase", AutoSize = true };
        private readonly CheckBox _wholeWord = new CheckBox { Name = "wholeWord", AutoSize = true };
        private readonly Label _scope = new Label { Name = "searchScope", AutoSize = true };
        private readonly Label _status = new Label { Name = "searchStatus", Dock = DockStyle.Fill, AutoSize = true };
        private readonly Label _replacementLabel;
        private readonly Button _replace;
        private readonly Button _replaceAll;
        private readonly Button _next;
        private readonly Button _previous;
        private readonly Button _advanced;
        private bool _replaceMode;

        internal FindReplaceDialog(Func<CodeEditorSearchTarget> target, Func<string> documentName)
        {
            _target = target;
            _documentName = documentName;
            Name = "findReplaceDialog";
            Font = SystemFonts.MessageBoxFont;
            AutoScaleMode = AutoScaleMode.Dpi;
            ShowInTaskbar = false;
            MinimizeBox = false;
            MaximizeBox = false;
            StartPosition = FormStartPosition.CenterParent;
            ClientSize = new Size(660, 270);
            MinimumSize = new Size(620, 280);

            var layout = new TableLayoutPanel { Dock = DockStyle.Fill, Padding = new Padding(12),
                ColumnCount = 2, RowCount = 6 };
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            for (var i = 0; i < 5; i++) layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            layout.Controls.Add(_scope, 0, 0);
            layout.SetColumnSpan(_scope, 2);
            layout.Controls.Add(LabelFor("FindLabel"), 0, 1);
            layout.Controls.Add(_query, 1, 1);
            _replacementLabel = LabelFor("ReplaceLabel");
            layout.Controls.Add(_replacementLabel, 0, 2);
            layout.Controls.Add(_replacement, 1, 2);

            _matchCase.Text = TextFor("MatchCase");
            _wholeWord.Text = TextFor("WholeWord");
            var options = new FlowLayoutPanel { AutoSize = true, Dock = DockStyle.Fill };
            options.Controls.AddRange(new Control[] { _matchCase, _wholeWord });
            layout.Controls.Add(options, 1, 3);

            var buttons = new FlowLayoutPanel { AutoSize = true, Dock = DockStyle.Fill, WrapContents = true };
            _next = ButtonFor("Next", "findNext", () => Find(false));
            _previous = ButtonFor("Previous", "findPrevious", () => Find(true));
            _replace = ButtonFor("Replace", "replaceCurrent", ReplaceCurrent);
            _replaceAll = ButtonFor("ReplaceAll", "replaceAll", ReplaceAll);
            _advanced = ButtonFor("AdvancedSearch", "advancedSearch", () =>
            {
                var current = _target();
                if (!current.HasAdvancedSearch) return;
                Hide();
                current.ShowAdvancedSearch();
            });
            var close = ButtonFor("Close", "closeSearch", Close);
            close.DialogResult = DialogResult.Cancel;
            buttons.Controls.AddRange(new Control[] { _next, _previous, _replace, _replaceAll, _advanced, close });
            layout.Controls.Add(buttons, 0, 4);
            layout.SetColumnSpan(buttons, 2);
            layout.Controls.Add(_status, 0, 5);
            layout.SetColumnSpan(_status, 2);
            Controls.Add(layout);
            AcceptButton = _next;
            CancelButton = close;
            _query.TextChanged += (sender, args) => ResetStatus();
            _matchCase.CheckedChanged += (sender, args) => ResetStatus();
            _wholeWord.CheckedChanged += (sender, args) => ResetStatus();
            RefreshScope();
        }

        internal void SetMode(bool replace, bool useSelection)
        {
            _replaceMode = replace;
            Text = TextFor(replace ? "ReplaceTitle" : "FindTitle");
            _replacementLabel.Visible = _replacement.Visible = _replace.Visible = _replaceAll.Visible = replace;
            if (useSelection)
            {
                var target = _target();
                var length = target.SelectionLength;
                if (length > 0 && length <= 4096)
                {
                    var selected = target.Text.Substring(target.SelectionStart, length);
                    if (selected.IndexOfAny(new[] { '\r', '\n' }) < 0) _query.Text = selected;
                }
            }
            RefreshScope();
            ActiveControl = _query;
            _query.SelectAll();
        }

        internal void RefreshScope()
        {
            _scope.Text = string.Format(CultureInfo.CurrentCulture, TextFor("Document"), Path.GetFileName(_documentName()));
            _advanced.Visible = _target().HasAdvancedSearch;
            ResetStatus();
        }

        private void ResetStatus()
        {
            var readOnly = _target().IsReadOnly;
            _status.Text = TextFor(readOnly && _replaceMode ? "ReadOnly" : "EnterSearch");
            _next.Enabled = _previous.Enabled = _query.Text.Length > 0;
            _replace.Enabled = _replaceAll.Enabled = _query.Text.Length > 0 && !readOnly;
        }

        protected override void OnActivated(EventArgs e)
        {
            base.OnActivated(e);
            RefreshScope();
        }

        internal void FindNext(bool backwards) => Find(backwards);

        private void Find(bool backwards)
        {
            if (_query.Text.Length == 0) { ResetStatus(); return; }
            var target = _target();
            var start = backwards ? target.SelectionStart - 1 : target.SelectionStart + target.SelectionLength;
            var index = TextSearch.Find(target.Text, _query.Text, start, backwards,
                _matchCase.Checked, _wholeWord.Checked);
            if (index >= 0) target.Select(index, _query.Text.Length);
            _status.Text = TextFor(index >= 0 ? "Found" : "NoMatch");
        }

        private void ReplaceCurrent()
        {
            var target = _target();
            if (target.IsReadOnly) { ResetStatus(); return; }
            var start = target.SelectionStart;
            var query = _query.Text;
            var text = target.Text;
            var selected = query.Length > 0 && target.SelectionLength == query.Length &&
                string.Compare(text, start, query, 0, query.Length,
                    _matchCase.Checked ? StringComparison.Ordinal : StringComparison.OrdinalIgnoreCase) == 0 &&
                (!_wholeWord.Checked || TextSearch.IsWholeWord(text, start, query.Length));
            if (!selected) { Find(false); return; }
            if (!target.Replace(new[] { start }, query.Length, _replacement.Text))
            {
                _status.Text = TextFor("ReadOnly");
                return;
            }
            target.Select(start + _replacement.Text.Length, 0);
            Find(false);
        }

        private void ReplaceAll()
        {
            var target = _target();
            if (target.IsReadOnly) { ResetStatus(); return; }
            var matches = TextSearch.FindAll(target.Text, _query.Text, _matchCase.Checked, _wholeWord.Checked);
            if (!target.Replace(matches, _query.Text.Length, _replacement.Text))
            {
                _status.Text = TextFor("ReadOnly");
                return;
            }
            if (matches.Count > 0) target.Select(matches[0], _replacement.Text.Length);
            _status.Text = string.Format(CultureInfo.CurrentCulture, TextFor("Replaced"), matches.Count);
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (keyData == Keys.F3 || keyData == (Keys.Shift | Keys.F3))
            {
                Find(keyData != Keys.F3);
                return true;
            }
            if (keyData == (Keys.Control | Keys.H)) { SetMode(true, false); return true; }
            if (keyData == (Keys.Control | Keys.F)) { SetMode(false, false); return true; }
            return base.ProcessCmdKey(ref msg, keyData);
        }

        private static string TextFor(string key) => EditorSearchStrings.Get(key);
        private static Label LabelFor(string key) => new Label { Text = TextFor(key), AutoSize = true,
            Anchor = AnchorStyles.Left, Margin = new Padding(3, 6, 8, 3) };
        private static Button ButtonFor(string key, string name, Action action)
        {
            var button = new Button { Name = name, Text = TextFor(key), AutoSize = true, MinimumSize = new Size(80, 28) };
            button.Click += (sender, args) => action();
            return button;
        }
    }
}
