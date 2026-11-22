using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Computator.NET.Core.Abstract.Controls;
using Computator.NET.Core.Abstract.Views;
using Computator.NET.Core.Menu.Commands.TopMenuCommands;
using Computator.NET.Core.Model;
using Computator.NET.Core.Natives;
using Computator.NET.Core.SymbolicCalculations;
using Computator.NET.DataTypes.Events;
using Computator.NET.DataTypes.SettingsTypes;
using Computator.NET.Desktop.Bootstrapping;
using Computator.NET.Desktop.Views;
using Computator.NET.Engine.Symbolic;
using NUnit.Framework;
using Settings = Computator.NET.Core.Properties.Settings;

namespace Computator.NET.Desktop.Tests
{
    /// <summary>Actual window, composition, shared expression editor and CAS integration.</summary>
    [TestFixture, Apartment(ApartmentState.STA), NonParallelizable]
    public class SymbolicCalculationsWorkflowTests
    {
        private WinFormsBootstrapper _bootstrapper;
        private MainView _window;
        private SymbolicCalculationsView _view;
        private IExpressionTextBox _expression;
        private ISharedViewState _state;
        private string _directory;
        private string _oldScripts;
        private string _oldFunctions;
        private CodeEditorType _oldEditor;
        private bool _oldCrossThreadChecks;

        [SetUp]
        public void OpenActualWindow()
        {
            GSLInitializer.Initialize();
            _directory = Path.Combine(Path.GetTempPath(), "Computator-Symbolic-" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(_directory);
            _oldScripts = Settings.Default.ScriptingDirectory;
            _oldFunctions = Settings.Default.CustomFunctionsDirectory;
            _oldEditor = Settings.Default.CodeEditor;
            _oldCrossThreadChecks = Control.CheckForIllegalCrossThreadCalls;
            Settings.Default.ScriptingDirectory = _directory;
            Settings.Default.CustomFunctionsDirectory = _directory;
            Settings.Default.CodeEditor = CodeEditorType.TextEditor;
            Control.CheckForIllegalCrossThreadCalls = true;
            _bootstrapper = new WinFormsBootstrapper();
            _window = _bootstrapper.Create<MainView>();
            _view = _bootstrapper.Create<SymbolicCalculationsView>();
            _expression = _bootstrapper.Create<IExpressionTextBox>();
            _state = _bootstrapper.Create<ISharedViewState>();
            _window.Show();
            _state.CurrentView = ViewName.SymbolicCalculations;
            Application.DoEvents();
            NUnit.Framework.Assert.That(_view.FindForm(), Is.SameAs(_window));
            NUnit.Framework.Assert.That(_bootstrapper.Create<ISymbolicCalculationsView>(), Is.SameAs(_view));
            NUnit.Framework.Assert.That(_view.Enabled && _view.Visible, Is.True);
            NUnit.Framework.Assert.That(_window.ModeText, Is.EqualTo("Mode[Symbolic]"));
        }

        [TearDown]
        public void CloseActualWindow()
        {
            _window?.Dispose();
            _bootstrapper?.Container.Dispose();
            Settings.Default.ScriptingDirectory = _oldScripts;
            Settings.Default.CustomFunctionsDirectory = _oldFunctions;
            Settings.Default.CodeEditor = _oldEditor;
            Control.CheckForIllegalCrossThreadCalls = _oldCrossThreadChecks;
            if (_directory != null && Directory.Exists(_directory)) Directory.Delete(_directory, true);
        }

        [TestCase("simplify", "sin²(x) + cos²(x)", "1")]
        [TestCase("simplify", "log(8, 2)", "3")]
        [TestCase("limit", "sin(x)/x", "1")]
        [TestCase("substitute", "x²+y", "7")]
        [TestCase("determinant", "[[1,2],[3,4]]", "-2")]
        [TestCase("trace", "[[1,2],[3,4]]", "5")]
        public void NativeCalculateButtonReturnsIndependentKnownMathematicalResults(string operation, string expression, string expected)
        {
            _view.SelectOperation(operation);
            _expression.Text = expression;
            Find<Button>("symbolicCalculate").PerformClick();
            WaitForCompletion();
            NUnit.Framework.Assert.That(Find<TextBox>("symbolicResult").Text, Is.EqualTo(expected));
            NUnit.Framework.Assert.That(Find<TextBox>("symbolicLatex").Text, Is.Not.Empty);
            NUnit.Framework.Assert.That(Find<Label>("symbolicStatus").Text, Does.Contain("Exact"));
        }

        [Test]
        public void RealToolbarCommandAndExpressionEnterUseTheDisplayedSymbolicTab()
        {
            _view.SelectOperation("differentiate");
            _expression.Text = "x³";
            _bootstrapper.Create<RunCommand>().Execute();
            WaitForCompletion();
            NUnit.Framework.Assert.That(Normalize(Find<TextBox>("symbolicResult").Text), Is.EqualTo("3·x²"));

            _view.SelectOperation("definite-integrate");
            _expression.Text = "x²";
            Find<TextBox>("symbolicUpperBound").Text = "3";
            // Raise the native control's actual KeyPress path. ExpressionViewPresenter
            // must dispatch Enter to the currently selected tab, not numeric evaluation.
            var expressionControl = (Control)_expression;
            typeof(Control).GetMethod("OnKeyPress", BindingFlags.NonPublic | BindingFlags.Instance)
                .Invoke(expressionControl, new object[] { new KeyPressEventArgs('\r') });
            WaitForCompletion();
            NUnit.Framework.Assert.That(Find<TextBox>("symbolicResult").Text, Is.EqualTo("9"));
        }

        [Test]
        public void ExpressionEditorKeyEventsProduceNativeTslForTheSymbolicPresenter()
        {
            _view.SelectOperation("substitute");
            Find<TextBox>("symbolicSubstitutions").Text = "x = 3";
            TypeThroughExpressionKeyPress("x^2^+2*x");
            NUnit.Framework.Assert.That(_expression.Text, Is.EqualTo("x²+2·x"));
            NUnit.Framework.Assert.That(_state.IsExponent, Is.False);
            _bootstrapper.Create<RunCommand>().Execute();
            WaitForCompletion();
            NUnit.Framework.Assert.That(Find<TextBox>("symbolicResult").Text, Is.EqualTo("15"));
        }

        [Test]
        public void NativeParameterFieldsAcceptTslPowersAndProducts()
        {
            _view.SelectOperation("definite-integrate");
            _expression.Text = "2·x";
            Find<TextBox>("symbolicLowerBound").Text = "1²";
            Find<TextBox>("symbolicUpperBound").Text = "3²";
            _bootstrapper.Create<RunCommand>().Execute();
            WaitForCompletion();
            NUnit.Framework.Assert.That(Find<TextBox>("symbolicResult").Text, Is.EqualTo("80"));
        }

        [TestCase("x²+2·x", "x = 3", "15")]
        [TestCase("x⁽ʸ⁺¹⁾", "x = 2\ny = 3", "16")]
        [TestCase("x²˙ʸ", "x = 2\ny = 3", "64")]
        [TestCase("xᶜᵒˢ⁽ʸ⁾", "x = 2\ny = 0", "2")]
        [TestCase("x⁰ᱸ⁵", "x = 9", "3")]
        [TestCase("xꞋʸꞋ", "x = 2\ny = -3", "8")]
        [TestCase("x⁽ʸ ⁺ ¹⁾", "x = 2\ny = 3", "16")]
        [TestCase("sin⁽ʸ⁾(x)", "x = 0\ny = 2", "0")]
        [TestCase("sin³(PI/6)²", "", "1/64")]
        public void ClipboardPastedNativeTslReachesTheActualEngine(string expression, string replacements, string expected)
        {
            var previous = Clipboard.GetDataObject();
            try
            {
                _view.SelectOperation("substitute");
                Find<TextBox>("symbolicSubstitutions").Text = replacements;
                Clipboard.SetText(expression);
                var input = (TextBox)_expression;
                input.SelectAll();
                input.Paste();
                NUnit.Framework.Assert.That(_expression.Text, Is.EqualTo(expression));
                _bootstrapper.Create<RunCommand>().Execute();
                WaitForCompletion();
                NUnit.Framework.Assert.That(Find<TextBox>("symbolicResult").Text, Is.EqualTo(expected));
            }
            finally { RestoreClipboard(previous); }
        }

        [Test]
        public void CopiedTslResultCanBePastedBackAndCalculatedWithoutReformatting()
        {
            var previous = Clipboard.GetDataObject();
            try
            {
                _view.SelectOperation("differentiate");
                _expression.Text = "x³";
                _bootstrapper.Create<RunCommand>().Execute();
                WaitForCompletion();
                var result = Find<TextBox>("symbolicResult");
                NUnit.Framework.Assert.That(Normalize(result.Text), Is.EqualTo("3·x²"));
                result.SelectAll();
                result.Copy();
                var copied = Clipboard.GetText();
                NUnit.Framework.Assert.That(copied, Is.EqualTo(result.Text));
                var input = (TextBox)_expression;
                input.SelectAll();
                input.Paste();
                NUnit.Framework.Assert.That(_expression.Text, Is.EqualTo(copied));
                _view.SelectOperation("substitute");
                Find<TextBox>("symbolicSubstitutions").Text = "x = 2";
                _bootstrapper.Create<RunCommand>().Execute();
                WaitForCompletion();
                NUnit.Framework.Assert.That(Find<TextBox>("symbolicResult").Text, Is.EqualTo("12"));
            }
            finally { RestoreClipboard(previous); }
        }

        [Test]
        public void EveryOperationHasAWorkingNativeExampleAndResultContract()
        {
            var expectedOperations = new[] { "simplify", "expand", "factor", "differentiate", "integrate",
                "definite-integrate", "limit", "solve", "substitute", "evaluate", "taylor", "solve-system",
                "determinant", "inverse", "transpose", "trace" };
            foreach (var operation in expectedOperations)
            {
                _view.SelectOperation(operation);
                Find<Button>("symbolicExample").PerformClick();
                NUnit.Framework.Assert.That(_expression.Text, Does.Not.Contain("^").And.Not.Contain("*"), operation);
                Find<Button>("symbolicCalculate").PerformClick();
                WaitForCompletion();
                NUnit.Framework.Assert.That(Find<TextBox>("symbolicResult").Text, Is.Not.Empty, operation);
                NUnit.Framework.Assert.That(Find<TextBox>("symbolicLatex").Text, Is.Not.Empty, operation);
                NUnit.Framework.Assert.That(Find<Label>("symbolicStatus").Text,
                    Does.Not.Contain("Could not calculate").And.Not.Contain("time limit"), operation);
            }
        }

        [Test]
        public void InvalidInputClearsEarlierResultsAndTheSameViewRecovers()
        {
            _expression.Text = "2 + 2";
            _view.SelectOperation("simplify");
            Find<Button>("symbolicCalculate").PerformClick();
            WaitForCompletion();
            NUnit.Framework.Assert.That(Find<TextBox>("symbolicResult").Text, Is.EqualTo("4"));
            _expression.Text = "sin(";
            Find<Button>("symbolicCalculate").PerformClick();
            WaitForCompletion();
            NUnit.Framework.Assert.That(Find<TextBox>("symbolicResult").Text, Is.Empty);
            NUnit.Framework.Assert.That(Find<Label>("symbolicStatus").Text, Does.Contain("Could not calculate"));
            _expression.Text = "6·7";
            Find<Button>("symbolicCalculate").PerformClick();
            WaitForCompletion();
            NUnit.Framework.Assert.That(Find<TextBox>("symbolicResult").Text, Is.EqualTo("42"));
        }

        [Test]
        public void StructuredInputIsPreservedAndHiddenFieldsDoNotPolluteAnotherOperation()
        {
            _view.SelectOperation("substitute");
            Find<TextBox>("symbolicSubstitutions").Text = "x = 2\nx = 3";
            _expression.Text = "x²";
            Find<Button>("symbolicCalculate").PerformClick();
            NUnit.Framework.Assert.That(Find<Label>("symbolicStatus").Text, Does.Contain("more than once"));
            _view.SelectOperation("solve-system");
            Find<TextBox>("symbolicEquations").Text = "x+y=3\nx-y=1";
            Find<TextBox>("symbolicVariables").Text = "x, y";
            var request = _view.CreateRequest("ignored(");
            NUnit.Framework.Assert.That(request.Expression, Is.Empty);
            NUnit.Framework.Assert.That(request.Equations, Is.EqualTo(new[] { "x+y=3", "x-y=1" }));
            NUnit.Framework.Assert.That(request.Variables, Is.EqualTo(new[] { "x", "y" }));
            _view.SelectOperation("simplify");
            Find<TextBox>("symbolicVariable").Text = "";
            Find<TextBox>("symbolicPoint").Text = "invalid(";
            _expression.Text = "2+2";
            Find<Button>("symbolicCalculate").PerformClick();
            WaitForCompletion();
            NUnit.Framework.Assert.That(Find<TextBox>("symbolicResult").Text, Is.EqualTo("4"));
        }

        [Test]
        public void ResultsAndControlsAreRenderedAndKeyboardAccessible()
        {
            _view.SelectOperation("differentiate");
            Find<Button>("symbolicExample").PerformClick();
            Find<Button>("symbolicCalculate").PerformClick();
            WaitForCompletion();
            _window.Refresh();
            Application.DoEvents();
            NUnit.Framework.Assert.That(Find<TextBox>("symbolicResult").ReadOnly, Is.True);
            NUnit.Framework.Assert.That(Find<TextBox>("symbolicLatex").ReadOnly, Is.True);
            NUnit.Framework.Assert.That(Find<ComboBox>("symbolicOperation").AccessibleName, Is.EqualTo("Symbolic operation"));
            foreach (var name in new[] { "symbolicCalculate", "symbolicOperation", "symbolicResult", "symbolicStatus" })
            {
                var control = Find<Control>(name);
                NUnit.Framework.Assert.That(control.Visible, Is.True, name);
                NUnit.Framework.Assert.That(control.Width, Is.GreaterThan(50), name);
                NUnit.Framework.Assert.That(control.Height, Is.GreaterThan(10), name);
            }
            var directory = Path.Combine(TestContext.CurrentContext.WorkDirectory, "TestResults", "symbolic-desktop");
            Directory.CreateDirectory(directory);
            var path = Path.Combine(directory, "native-symbolic-tab.png");
            using (var bitmap = new Bitmap(_window.Width, _window.Height))
            {
                // Read the visible native window. Mono DrawToBitmap recurses into
                // hidden zero-height controls and does not establish on-screen output.
                using (var graphics = Graphics.FromImage(bitmap))
                    graphics.CopyFromScreen(_window.Location, Point.Empty, _window.Size);
                bitmap.Save(path, ImageFormat.Png);
            }
            TestContext.AddTestAttachment(path, "Actual visible native window and symbolic controls.");
        }

        private void WaitForCompletion()
        {
            var watch = Stopwatch.StartNew();
            while (Find<Button>("symbolicCancel").Enabled && watch.Elapsed < TimeSpan.FromSeconds(30))
            { Application.DoEvents(); Thread.Sleep(5); }
            Application.DoEvents();
            NUnit.Framework.Assert.That(Find<Button>("symbolicCancel").Enabled, Is.False,
                "The native dispatcher did not complete the bounded symbolic calculation.");
            NUnit.Framework.Assert.That(Find<Button>("symbolicCalculate").Enabled, Is.True);
        }

        private T Find<T>(string name) where T : Control => (T)_window.Controls.Find(name, true).Single();
        private static string Normalize(string expression) => expression.Replace(" ", "").Replace("\r", "").Replace("\n", "");

        private void TypeThroughExpressionKeyPress(string keys)
        {
            var input = (TextBox)_expression;
            input.Clear();
            input.Focus();
            _state.IsExponent = false;
            var keyPress = typeof(Control).GetMethod("OnKeyPress", BindingFlags.NonPublic | BindingFlags.Instance);
            foreach (var character in keys)
            {
                // Exercise the displayed control's actual event handlers, including
                // exponent-mode conversion, then insert the surviving character.
                // The Mono acceptance suite separately drives real XTEST keystrokes.
                var args = new KeyPressEventArgs(character);
                keyPress.Invoke(input, new object[] { args });
                if (!args.Handled) input.SelectedText = args.KeyChar.ToString();
            }
        }

        private static void RestoreClipboard(IDataObject previous)
        {
            if (previous == null) Clipboard.Clear();
            else Clipboard.SetDataObject(previous, true);
        }
    }
}
