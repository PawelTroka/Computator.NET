using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using Computator.NET.Core.Abstract.Views;
using Computator.NET.Engine.Symbolic;

namespace Computator.NET.Desktop.Views
{
    /// <summary>Native, keyboard-accessible controls for the shared symbolic engine.</summary>
    public sealed class SymbolicCalculationsView : UserControl, ISymbolicCalculationsView
    {
        private sealed class Operation
        {
            public Operation(string id, string label, string example, string help)
            { Id = id; Label = label; Example = example; Help = help; }
            public string Id { get; }
            public string Label { get; }
            public string Example { get; }
            public string Help { get; }
            public override string ToString() => Label;
        }

        private static readonly Operation[] Operations =
        {
            new Operation("simplify", "Simplify", "sin²(x) + cos²(x)", "Simplify an expression. The domain does not imply that variables are positive."),
            new Operation("expand", "Expand", "(x + 1)³", "Expand products and powers into sums."),
            new Operation("factor", "Factor", "x² - 1", "Factor an algebraic expression."),
            new Operation("differentiate", "Differentiate", "sin(x²)", "Differentiate with respect to one variable. Other symbols remain parameters."),
            new Operation("integrate", "Indefinite integral", "x²", "Find an antiderivative. An unevaluated integral is shown explicitly when a closed form is unavailable."),
            new Operation("definite-integrate", "Definite integral", "x²", "Evaluate an integral between the lower and upper bounds."),
            new Operation("limit", "Limit", "sin(x)/x", "Take a limit at the selected point, from both sides, the left, or the right."),
            new Operation("solve", "Solve equation", "x² = 2", "Solve one equation for the selected variable. An expression without = is equated to zero."),
            new Operation("substitute", "Substitute", "x² + y", "Give one replacement per line, for example x = 2 and y = 3."),
            new Operation("evaluate", "Numerical value", "sqrt(2)", "Evaluate numerically at the requested decimal precision. Symbols without values remain symbolic."),
            new Operation("taylor", "Taylor polynomial", "exp(x)", "Expand around the point, up to and including the selected degree."),
            new Operation("solve-system", "Solve equation system", "", "Enter equations below, one per line, and a comma-separated list of unknowns. The expression above is not used."),
            new Operation("determinant", "Matrix determinant", "[[1,2],[3,4]]", "Enter a matrix above using [[a,b],[c,d]]. A determinant requires a square matrix."),
            new Operation("inverse", "Matrix inverse", "[[1,2],[3,4]]", "Find an exact inverse of a nonsingular square matrix."),
            new Operation("transpose", "Matrix transpose", "[[1,2,3],[4,5,6]]", "Transpose a matrix; rectangular matrices are supported."),
            new Operation("trace", "Matrix trace", "[[1,2],[3,4]]", "Sum the diagonal entries of a square matrix.")
        };

        private readonly ComboBox _operation = Choice("symbolicOperation", Operations);
        private readonly ComboBox _domain = Choice("symbolicDomain", new object[] { "complex", "real" });
        private readonly ComboBox _direction = Choice("symbolicDirection", new object[] { "both", "left", "right" });
        private readonly TextBox _variable = Input("symbolicVariable", "x");
        private readonly TextBox _point = Input("symbolicPoint", "0");
        private readonly TextBox _lower = Input("symbolicLowerBound", "0");
        private readonly TextBox _upper = Input("symbolicUpperBound", "1");
        private readonly NumericUpDown _order = Number("symbolicOrder", 1, 20, 1);
        private readonly NumericUpDown _precision = Number("symbolicPrecision", 1, 100, 30);
        private readonly TextBox _substitutions = Multiline("symbolicSubstitutions", false);
        private readonly TextBox _equations = Multiline("symbolicEquations", false);
        private readonly TextBox _variables = Input("symbolicVariables", "x, y");
        private readonly TextBox _result = Multiline("symbolicResult", true);
        private readonly TextBox _latex = Multiline("symbolicLatex", true);
        private readonly Label _help = Paragraph("symbolicHelp");
        private readonly Label _status = Paragraph("symbolicStatus");
        private readonly Label _warnings = Paragraph("symbolicWarnings");
        private readonly Button _calculate = Button("symbolicCalculate", "&Calculate");
        private readonly Button _cancel = Button("symbolicCancel", "Cancel");
        private readonly Dictionary<string, Control> _fields = new Dictionary<string, Control>();
        private readonly FlowLayoutPanel _parameters;
        private readonly Control _substitutionPanel;
        private readonly Control _systemPanel;
        private readonly ToolTip _toolTip = new ToolTip();
        private bool _initializing = true;

        public SymbolicCalculationsView()
        {
            Name = "symbolicCalculationsView";
            AutoScaleMode = AutoScaleMode.Font;
            Padding = new Padding(12);
            BackColor = SystemColors.Window;
            var layout = new TableLayoutPanel
            {
                Name = "symbolicLayout", Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 7,
                Padding = Padding.Empty
            };
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            for (var row = 0; row < 6; row++) layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            Controls.Add(layout);

            var toolbar = new FlowLayoutPanel
            { Dock = DockStyle.Fill, AutoSize = true, WrapContents = true, Margin = new Padding(0, 0, 0, 4) };
            _operation.Width = 190;
            _operation.AccessibleName = "Symbolic operation";
            var example = Button("symbolicExample", "Load example");
            toolbar.Controls.AddRange(new Control[] { _operation, example, _calculate, _cancel });
            layout.Controls.Add(toolbar, 0, 0);
            layout.Controls.Add(_help, 0, 1);

            _parameters = new FlowLayoutPanel
            { Name = "symbolicParameters", Dock = DockStyle.Fill, AutoSize = true, WrapContents = true };
            AddField("domain", "Domain", _domain);
            AddField("variable", "Variable", _variable);
            AddField("order", "Order / degree", _order);
            AddField("point", "Point", _point);
            AddField("direction", "Direction", _direction);
            AddField("lower", "Lower bound", _lower);
            AddField("upper", "Upper bound", _upper);
            AddField("precision", "Decimal digits", _precision);
            layout.Controls.Add(_parameters, 0, 2);

            _substitutions.Height = 70;
            _substitutions.ScrollBars = ScrollBars.Vertical;
            _substitutions.Text = "x = 2" + Environment.NewLine + "y = 3";
            _substitutionPanel = Block("Replacements — one name = expression per line (optional for numerical value)", _substitutions);
            _equations.Height = 70;
            _equations.ScrollBars = ScrollBars.Vertical;
            _equations.Text = "2·x + y = 5" + Environment.NewLine + "x - y = 1";
            var system = new TableLayoutPanel { Dock = DockStyle.Top, AutoSize = true, ColumnCount = 1 };
            system.Controls.Add(Block("Equations — one per line", _equations));
            system.Controls.Add(Block("Unknowns — separated by commas", _variables));
            _systemPanel = system;
            var structured = new TableLayoutPanel { Dock = DockStyle.Top, AutoSize = true, ColumnCount = 1 };
            structured.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            structured.Controls.Add(_substitutionPanel);
            structured.Controls.Add(_systemPanel);
            layout.Controls.Add(structured, 0, 3);
            layout.Controls.Add(_status, 0, 4);
            layout.Controls.Add(_warnings, 0, 5);

            var tabs = new TabControl { Name = "symbolicResults", Dock = DockStyle.Fill, MinimumSize = new Size(150, 110) };
            var resultPage = new TabPage("Result") { Padding = new Padding(6) };
            var latexPage = new TabPage("LaTeX") { Padding = new Padding(6) };
            resultPage.Controls.Add(_result);
            latexPage.Controls.Add(_latex);
            tabs.TabPages.AddRange(new[] { resultPage, latexPage });
            layout.Controls.Add(tabs, 0, 6);

            _calculate.Click += (sender, args) => ComputeClicked?.Invoke(this, args);
            _cancel.Click += (sender, args) => CancelClicked?.Invoke(this, args);
            example.Click += (sender, args) => LoadExample();
            _operation.SelectedIndexChanged += (sender, args) => { UpdateFields(); Changed(); };
            foreach (var control in new Control[] { _domain, _direction, _variable, _point, _lower, _upper,
                _order, _precision, _substitutions, _equations, _variables })
            {
                if (control is ComboBox combo) combo.SelectedIndexChanged += (sender, args) => Changed();
                else if (control is NumericUpDown number) number.ValueChanged += (sender, args) => Changed();
                else control.TextChanged += (sender, args) => Changed();
                control.KeyDown += OnInputKeyDown;
            }
            _operation.KeyDown += OnInputKeyDown;
            _toolTip.SetToolTip(_domain, "Real or principal complex values. Real does not mean positive.");
            _toolTip.SetToolTip(example, "Load an example into the shared expression editor and reset parameters for this operation.");
            _toolTip.SetToolTip(_result, "Select text and press Ctrl+C to copy the TSL result back into the expression editor.");
            _toolTip.SetToolTip(_latex, "Copyable LaTeX source for this result.");
            Resize += (sender, args) => ResizeParagraphs();
            _cancel.Enabled = false;
            UpdateFields();
            _initializing = false;
        }

        public event EventHandler ComputeClicked;
        public event EventHandler CancelClicked;
        public event EventHandler InputChanged;
        public event EventHandler<SymbolicExampleEventArgs> ExampleRequested;

        public void SelectOperation(string operation)
        {
            var index = Array.FindIndex(Operations, value => value.Id == operation);
            if (index < 0) throw new ArgumentException("Unknown symbolic operation.", nameof(operation));
            _operation.SelectedIndex = index;
        }

        public SymbolicRequest CreateRequest(string expression)
        {
            var operation = ((Operation)_operation.SelectedItem).Id;
            var request = new SymbolicRequest
            {
                Operation = operation, Expression = expression ?? "", Domain = (string)_domain.SelectedItem
            };
            if (new[] { "differentiate", "integrate", "definite-integrate", "limit", "solve", "taylor" }.Contains(operation))
                request.Variable = _variable.Text.Trim();
            if (operation == "differentiate" || operation == "taylor") request.Order = (int)_order.Value;
            if (operation == "limit" || operation == "taylor") request.Point = _point.Text.Trim();
            if (operation == "limit") request.Direction = (string)_direction.SelectedItem;
            if (operation == "evaluate") request.Precision = (int)_precision.Value;
            if (operation == "definite-integrate")
            {
                request.LowerBound = _lower.Text.Trim();
                request.UpperBound = _upper.Text.Trim();
            }
            if (operation == "substitute" || operation == "evaluate")
            {
                foreach (var line in Lines(_substitutions.Text))
                {
                    var equals = line.IndexOf('=');
                    if (equals <= 0 || equals == line.Length - 1)
                        throw new ArgumentException("Use one replacement per line: name = expression.");
                    var name = line.Substring(0, equals).Trim();
                    var value = line.Substring(equals + 1).Trim();
                    if (request.Substitutions.ContainsKey(name))
                        throw new ArgumentException("A replacement for '" + name + "' appears more than once.");
                    request.Substitutions.Add(name, value);
                }
            }
            if (operation == "solve-system")
            {
                request.Expression = "";
                request.Equations = Lines(_equations.Text);
                request.Variables = _variables.Text.Split(',').Select(value => value.Trim()).ToArray();
            }
            return request;
        }

        public void SetBusy(bool busy)
        {
            // Run can be invoked before the tab has painted. Create the dispatcher
            // handle on its owning UI thread, never from the worker continuation.
            if (busy && !IsHandleCreated) { var handle = Handle; }
            _calculate.Enabled = !busy;
            _cancel.Enabled = busy;
            UseWaitCursor = false; // Editing and cancellation remain available while the worker runs.
        }

        public void ClearResult()
        {
            _result.Clear();
            _latex.Clear();
            _warnings.Text = "";
        }

        public void ShowResult(SymbolicResult result)
        {
            _result.Text = result.Result ?? "";
            _latex.Text = result.Latex ?? "";
            _warnings.Text = string.Join(Environment.NewLine, result.Warnings ?? new string[0]);
            switch (result.Status)
            {
                case "exact": ShowStatus("Exact symbolic result in TSL notation"); break;
                case "approximate": ShowStatus("Approximate numerical result"); break;
                case "unevaluated": ShowStatus("Unevaluated — the engine did not find a closed form for this request."); break;
                case "undefined": ShowStatus("Undefined for the requested expression or domain."); break;
                default: ShowStatus("Result: " + result.Status); break;
            }
        }

        public void ShowError(string message)
        {
            ClearResult();
            _status.ForeColor = Color.Firebrick;
            _status.Text = "Could not calculate: " + message;
        }

        public void ShowStatus(string message)
        {
            _status.ForeColor = SystemColors.ControlText;
            _status.Text = message;
        }

        public void Dispatch(Action action)
        {
            if (IsDisposed || Disposing || !IsHandleCreated) return;
            try
            {
                if (InvokeRequired) BeginInvoke((Action)(() => { if (!IsDisposed && !Disposing) action(); }));
                else action();
            }
            catch (InvalidOperationException) when (IsDisposed || Disposing || !IsHandleCreated) { }
        }

        private void UpdateFields()
        {
            var operation = (Operation)_operation.SelectedItem;
            _help.Text = "Use TSL above: x², 2·x or x⁽ʸ⁺¹⁾. " + operation.Help + Environment.NewLine +
                "Type ^ to toggle superscripts; * becomes ·. Logarithms: ln(x), log10(x), log(x, base).";
            var variable = new[] { "differentiate", "integrate", "definite-integrate", "limit", "solve", "taylor" }.Contains(operation.Id);
            _fields["variable"].Visible = variable;
            _fields["order"].Visible = operation.Id == "differentiate" || operation.Id == "taylor";
            _order.Minimum = operation.Id == "taylor" ? 0 : 1;
            _fields["point"].Visible = operation.Id == "limit" || operation.Id == "taylor";
            _fields["direction"].Visible = operation.Id == "limit";
            _fields["lower"].Visible = _fields["upper"].Visible = operation.Id == "definite-integrate";
            _fields["precision"].Visible = operation.Id == "evaluate";
            _substitutionPanel.Visible = operation.Id == "substitute" || operation.Id == "evaluate";
            _systemPanel.Visible = operation.Id == "solve-system";
            ResizeParagraphs();
        }

        private void LoadExample()
        {
            var operation = (Operation)_operation.SelectedItem;
            _variable.Text = "x";
            _point.Text = "0";
            _order.Value = operation.Id == "taylor" ? 5 : 1;
            _precision.Value = 30;
            _lower.Text = "0";
            _upper.Text = "1";
            _domain.SelectedIndex = 0;
            _direction.SelectedIndex = 0;
            _substitutions.Text = "x = 2" + Environment.NewLine + "y = 3";
            _equations.Text = "2·x + y = 5" + Environment.NewLine + "x - y = 1";
            _variables.Text = "x, y";
            ExampleRequested?.Invoke(this, new SymbolicExampleEventArgs(operation.Example));
        }

        private void OnInputKeyDown(object sender, KeyEventArgs args)
        {
            if (args.KeyCode == Keys.Escape && _cancel.Enabled)
            { CancelClicked?.Invoke(this, EventArgs.Empty); args.SuppressKeyPress = true; }
            if (args.KeyCode == Keys.Enter && (args.Control || !(sender is TextBox text && text.Multiline)))
            { ComputeClicked?.Invoke(this, EventArgs.Empty); args.SuppressKeyPress = true; }
        }

        private void Changed() { if (!_initializing) InputChanged?.Invoke(this, EventArgs.Empty); }
        private static string[] Lines(string text) => text.Split(new[] { '\r', '\n', ';' }, StringSplitOptions.RemoveEmptyEntries)
            .Select(value => value.Trim()).Where(value => value.Length != 0).ToArray();

        private void AddField(string name, string label, Control input)
        {
            input.AccessibleName = label;
            var field = new FlowLayoutPanel
            { AutoSize = true, FlowDirection = FlowDirection.TopDown, WrapContents = false, Margin = new Padding(0, 3, 12, 6) };
            field.Controls.Add(new Label { Text = label, AutoSize = true, Margin = new Padding(3, 0, 3, 2) });
            field.Controls.Add(input);
            _parameters.Controls.Add(field);
            _fields.Add(name, field);
        }

        private void ResizeParagraphs()
        {
            var width = Math.Max(150, ClientSize.Width - Padding.Horizontal - 12);
            foreach (var label in new[] { _help, _status, _warnings }) label.MaximumSize = new Size(width, 0);
        }

        private static ComboBox Choice(string name, object[] choices)
        {
            var control = new ComboBox { Name = name, DropDownStyle = ComboBoxStyle.DropDownList, Width = 115 };
            control.Items.AddRange(choices);
            control.SelectedIndex = 0;
            return control;
        }
        private static TextBox Input(string name, string text) => new TextBox { Name = name, Text = text, Width = 115, MaxLength = 8192 };
        private static NumericUpDown Number(string name, int minimum, int maximum, int value) =>
            new NumericUpDown { Name = name, Minimum = minimum, Maximum = maximum, Value = value, Width = 95 };
        private static TextBox Multiline(string name, bool readOnly) => new TextBox
        {
            Name = name, Multiline = true, ReadOnly = readOnly, WordWrap = false, ScrollBars = ScrollBars.Both,
            Dock = DockStyle.Fill, MaxLength = readOnly ? 0 : 32768, Font = new Font(FontFamily.GenericMonospace, 11),
            BackColor = SystemColors.Window, AccessibleName = name == "symbolicLatex" ? "LaTeX result" : name
        };
        private static Button Button(string name, string text) =>
            new Button { Name = name, Text = text, AutoSize = true, MinimumSize = new Size(95, 28), UseVisualStyleBackColor = true };
        private static Label Paragraph(string name) =>
            new Label { Name = name, AutoSize = true, Margin = new Padding(3, 3, 3, 6), Dock = DockStyle.Fill };
        private static Control Block(string label, Control input)
        {
            var table = new TableLayoutPanel { AutoSize = true, Dock = DockStyle.Top, ColumnCount = 1, RowCount = 2 };
            table.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            table.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            table.RowStyles.Add(new RowStyle(SizeType.Absolute, input.Height));
            table.Controls.Add(new Label { Text = label, AutoSize = true }, 0, 0);
            input.Dock = DockStyle.Fill;
            table.Controls.Add(input, 0, 1);
            input.AccessibleName = label;
            return table;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _toolTip.Dispose();
                foreach (var input in new[] { _result, _latex, _equations, _substitutions }) input.Font.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
