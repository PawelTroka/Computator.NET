using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using Computator.NET.Core.Abstract.Controls;
using Computator.NET.Core.Model;
using Computator.NET.Core.Natives;
using Computator.NET.Core.Services.ErrorHandling;
using Computator.NET.DataTypes;
using Computator.NET.DataTypes.Events;
using Computator.NET.DataTypes.SettingsTypes;
using Computator.NET.Desktop.Bootstrapping;
using Computator.NET.Desktop.Controls;
using Computator.NET.Desktop.Controls.CodeEditors;
using Computator.NET.Desktop.Views;
using Settings = Computator.NET.Core.Properties.Settings;

internal static partial class Program
{
    private static void OperatorInputRealX11()
    {
        GSLInitializer.Initialize();
        var previousEditor = Settings.Default.CodeEditor;
        var previousScripts = Settings.Default.ScriptingDirectory;
        var previousFunctions = Settings.Default.CustomFunctionsDirectory;
        var directory = Path.Combine(outputDirectory, "operator-input-documents");
        Directory.CreateDirectory(directory);
        Settings.Default.CodeEditor = CodeEditorType.TextEditor;
        Settings.Default.ScriptingDirectory = Settings.Default.CustomFunctionsDirectory = directory;
        var errors = new RecordingErrors();
        bool? previousNumLock = null;
        try
        {
            var bootstrapper = new WinFormsBootstrapper();
            using (bootstrapper.Container)
            {
                bootstrapper.RegisterInstance<IErrorHandler>(errors);
                using (var window = bootstrapper.Create<MainView>())
                {
                    window.StartPosition = FormStartPosition.Manual;
                    window.Location = new Point(5, 5);
                    window.Size = new Size(1100, 760);
                    window.Show();
                    PumpOperatorInput();
                    previousNumLock = OperatorNumLock.Enabled;
                    var state = bootstrapper.Create<ISharedViewState>();
                    state.CalculationsMode = CalculationsMode.Real;
                    var expression = (ExpressionTextBox)bootstrapper.Create<ITextProvider>();
                    var scripts = bootstrapper.Create<ScriptingView>().CodeEditorView;
                    var editor = SearchDescendants((Control)scripts).OfType<TextEditorCodeEditor>().Single();
                    var area = editor.ActiveTextAreaControl.TextArea;
                    Same(expression.FindForm(), window, "Operator input must use the displayed expression editor.");
                    Same(editor.FindForm(), window, "Operator input must use the displayed scripting editor.");

                    void Focus(Control input, ViewName view)
                    {
                        state.CurrentView = view;
                        state.IsExponent = false;
                        errors.Messages.Clear();
                        PumpOperatorInput();
                        NativeX11Input.FocusOwnedWindow(window.Handle, PumpOperatorInput);
                        input.Focus();
                        PumpOperatorInput();
                        Check(input.Focused, "Native operator input focus did not reach " + input.GetType().Name + ".");
                    }

                    void SendOperator(Control input, string key, bool? numLock = null)
                    {
                        if (numLock.HasValue)
                            Check(OperatorNumLock.Enabled == numLock.Value, "Num Lock changed before " + key + ".");
                        using (var trace = new OperatorKeyTrace(input, key + " num-lock=" + OperatorNumLock.Enabled))
                        {
                            // Preserve the asserted lock state while this operator is
                            // sent; --clearmodifiers can temporarily clear lock modifiers.
                            Xdotool("key " + key);
                            PumpOperatorInput();
                        }
                        if (numLock.HasValue)
                            Check(OperatorNumLock.Enabled == numLock.Value, "Num Lock changed while sending " + key + ".");
                    }

                    void Calculate(string expected)
                    {
                        state.DefaultActions[ViewName.Calculations](window, EventArgs.Empty);
                        errors.AssertEmpty();
                        Equal(Find(window, "calculationValueTextBox").Text, expected);
                    }

                    void RunScript(string expected)
                    {
                        state.DefaultActions[ViewName.Scripting](window, EventArgs.Empty);
                        errors.AssertEmpty();
                        Equal(Find(window, "consoleOutputTextBox").Text,
                            Computator.NET.Localization.Strings.ConsoleOutput + Environment.NewLine + expected);
                    }

                    // Keep each editor/operator/key source independently visible in the
                    // report, while sharing the real desktop and its menu/shortcut path.
                    var operators = new[]
                    {
                        new { Text = "+", Expression = "+", Name = "plus", Key = "plus", Keypad = "KP_Add", Result = "10" },
                        new { Text = "-", Expression = "-", Name = "minus", Key = "minus", Keypad = "KP_Subtract", Result = "6" },
                        new { Text = "*", Expression = "·", Name = "multiply", Key = "asterisk", Keypad = "KP_Multiply", Result = "16" },
                        new { Text = "/", Expression = "/", Name = "divide", Key = "slash", Keypad = "KP_Divide", Result = "4" }
                    };
                    foreach (var source in new[] { "main", "keypad-numlock-off", "keypad-numlock-on" })
                    {
                        var keypad = source != "main";
                        var numLock = source == "keypad-numlock-on";
                        foreach (var operation in operators)
                        {
                            var key = keypad ? operation.Keypad : operation.Key;
                            var suffix = source + "-" + operation.Name;
                            Run("mono-expression-operator-" + suffix, () =>
                            {
                                Focus(expression, ViewName.Calculations);
                                OperatorNumLock.Set(numLock);
                                expression.Clear();
                                TypeOperatorX11("8");
                                SendOperator(expression, key, numLock);
                                TypeOperatorX11("2");
                                Equal(expression.Text, "8" + operation.Expression + "2");
                                Calculate(operation.Result);
                            });
                            Run("mono-scripting-operator-" + suffix, () =>
                            {
                                Focus(area, ViewName.Scripting);
                                OperatorNumLock.Set(numLock);
                                scripts.Text = string.Empty;
                                editor.ActiveTextAreaControl.SelectionManager.ClearSelection();
                                editor.ActiveTextAreaControl.Caret.Position = editor.Document.OffsetToPosition(0);
                                TypeOperatorX11("write(8");
                                SendOperator(area, key, numLock);
                                TypeOperatorX11("2);");
                                Equal(scripts.Text, "write(8" + operation.Text + "2);");
                                RunScript(operation.Result);
                            });
                        }
                        Run("mono-expression-native-two-plus-two-" + source, () =>
                        {
                            Focus(expression, ViewName.Calculations);
                            OperatorNumLock.Set(numLock);
                            expression.Clear();
                            TypeOperatorX11("2");
                            SendOperator(expression, keypad ? "KP_Add" : "plus", numLock);
                            TypeOperatorX11("2");
                            Equal(expression.Text, "2+2");
                            Calculate("4");
                        });
                    }
                    Run("mono-expression-native-superscript-operators", () =>
                    {
                        Focus(expression, ViewName.Calculations);
                        expression.Clear();
                        TypeOperatorX11("2");
                        SendOperator(expression, "asciicircum");
                        Check(state.IsExponent, "The native caret key did not enter exponent mode.");
                        TypeOperatorX11("(3");
                        SendOperator(expression, "KP_Add");
                        TypeOperatorX11("2");
                        SendOperator(expression, "KP_Subtract");
                        TypeOperatorX11("1)");
                        SendOperator(expression, "KP_Multiply");
                        TypeOperatorX11("2");
                        SendOperator(expression, "KP_Divide");
                        TypeOperatorX11("4");
                        SendOperator(expression, "asciicircum");
                        Check(!state.IsExponent, "The native caret key did not leave exponent mode.");
                        Equal(expression.Text, "2⁽³⁺²⁻¹⁾˙²˸⁴");
                        Calculate("4");
                    });
                    Run("mono-expression-native-selection-undo-keeps-unicode", () =>
                    {
                        string[] ReplaceAndUndo(string key)
                        {
                            Focus(expression, ViewName.Calculations);
                            expression.Text = "2·3+4²";
                            expression.ClearUndo();
                            expression.Select(3, 1);
                            SendOperator(expression, key);
                            Equal(expression.Text, "2·3-4²");
                            Calculate("-10");
                            KeyOperatorX11("ctrl+z");
                            var firstUndo = expression.Text;
                            // Native Mono records selected-text deletion separately
                            // from insertion. Compare with the unmodified main key.
                            KeyOperatorX11("ctrl+z");
                            var secondUndo = expression.Text;
                            Console.WriteLine("Expression " + key + " Undo sequence: '" + firstUndo + "', '" + secondUndo + "'.");
                            Equal(secondUndo, "2·3+4²");
                            Calculate("22");
                            return new[] { firstUndo, secondUndo };
                        }
                        var mainUndo = ReplaceAndUndo("minus");
                        var keypadUndo = ReplaceAndUndo("KP_Subtract");
                        Check(mainUndo.SequenceEqual(keypadUndo), "Keypad replacement changed the native main-key Undo behavior.");
                    });
                    Run("mono-scripting-native-insertion-undo-keeps-unicode", () =>
                    {
                        const string source = "write(2·3+4²); // α π zażółć gęślą jaźń";
                        Focus(area, ViewName.Scripting);
                        scripts.Text = source;
                        editor.ActiveTextAreaControl.SelectionManager.ClearSelection();
                        editor.ActiveTextAreaControl.Caret.Position = editor.Document.OffsetToPosition(source.IndexOf("3", StringComparison.Ordinal));
                        SendOperator(area, "KP_Subtract");
                        Equal(scripts.Text, "write(2·-3+4²); // α π zażółć gęślą jaźń");
                        RunScript("10");
                        KeyOperatorX11("ctrl+z");
                        Equal(scripts.Text, source);
                        RunScript("22");
                    });

                    Control PrepareInput(bool scripting, string text)
                    {
                        var input = scripting ? (Control)area : expression;
                        Focus(input, scripting ? ViewName.Scripting : ViewName.Calculations);
                        if (scripting)
                        {
                            scripts.Text = text;
                            editor.ActiveTextAreaControl.SelectionManager.ClearSelection();
                            editor.ActiveTextAreaControl.Caret.Position = editor.Document.OffsetToPosition(text.Length);
                        }
                        else
                        {
                            expression.Text = text;
                            expression.Select(text.Length, 0);
                        }
                        return input;
                    }

                    Run("mono-scripting-control-keypad-divide-keeps-shortcut-routing", () =>
                    {
                        const string text = "write(8+2);";
                        PrepareInput(true, text);
                        var shortcuts = 0;
                        ICSharpCode.TextEditor.DialogKeyProcessor shortcut = keyData =>
                        {
                            if (keyData != (Keys.Control | Keys.Divide)) return false;
                            shortcuts++;
                            return true;
                        };
                        // Observe the editor's command route independently of its
                        // current syntax definition's optional comment metadata.
                        area.DoProcessDialogKey += shortcut;
                        try
                        {
                            SendOperator(area, "ctrl+KP_Divide");
                            Check(shortcuts == 1, "Ctrl+keypad Divide did not reach the editor command route exactly once.");
                            Equal(scripts.Text, text);
                        }
                        finally { area.DoProcessDialogKey -= shortcut; }
                    });
                    foreach (var scripting in new[] { false, true })
                    {
                        var name = scripting ? "scripting" : "expression";
                        string InputText() => scripting ? scripts.Text : expression.Text;

                        Run("mono-" + name + "-keypad-honors-suppress-keypress", () =>
                        {
                            var input = PrepareInput(scripting, "2");
                            var keyDowns = 0;
                            KeyEventHandler suppress = (sender, args) =>
                            {
                                if (args.KeyCode != Keys.Add) return;
                                keyDowns++;
                                args.SuppressKeyPress = true;
                            };
                            input.KeyDown += suppress;
                            try
                            {
                                SendOperator(input, "KP_Add");
                                Check(keyDowns == 1, "The keypad must deliver exactly one KeyDown before character insertion.");
                                Equal(InputText(), "2");
                            }
                            finally { input.KeyDown -= suppress; }
                            SendOperator(input, "KP_Add");
                            Equal(InputText(), "2+");
                        });
                        Run("mono-" + name + "-keypad-preserves-read-only-buffer", () =>
                        {
                            var input = PrepareInput(scripting, "2·3");
                            if (scripting) editor.Document.ReadOnly = true;
                            else expression.ReadOnly = true;
                            try
                            {
                                SendOperator(input, "KP_Add");
                                Equal(InputText(), "2·3");
                            }
                            finally
                            {
                                if (scripting) editor.Document.ReadOnly = false;
                                else expression.ReadOnly = false;
                            }
                        });
                        Run("mono-" + name + "-repeated-keypad-operators-insert-once", () =>
                        {
                            var input = PrepareInput(scripting, string.Empty);
                            SendOperator(input, "KP_Add KP_Add KP_Subtract KP_Subtract KP_Multiply KP_Multiply KP_Divide KP_Divide");
                            Equal(InputText(), scripting ? "++--**//" : "++--··//");
                        });
                        Run("mono-" + name + "-open-menu-keeps-keypad-out-of-background-editor", () =>
                        {
                            var input = PrepareInput(scripting, "2");
                            var menu = window.Controls.OfType<MenuStripView>().Single().Controls.OfType<MenuStrip>().Single();
                            var file = menu.Items.OfType<ToolStripMenuItem>().First();
                            try
                            {
                                var bounds = file.Bounds;
                                var point = menu.PointToScreen(new Point(bounds.Left + bounds.Width / 2,
                                    bounds.Top + bounds.Height / 2));
                                ClickOperatorX11(point.X, point.Y);
                                WaitForOperatorInput(() => file.DropDown.Visible, "A native click did not open the File menu.");
                                Console.WriteLine("Open menu keypad case: editor=" + name + " focused=" + input.Focused);
                                SendOperator(input, "KP_Add");
                                Equal(InputText(), "2");
                            }
                            finally
                            {
                                KeyOperatorX11("Escape");
                                KeyOperatorX11("Escape");
                            }
                        });
                    }

                    // These are intentional fixture drafts. Disposing, as in the
                    // find/replace acceptance, avoids an interactive save prompt.
                    window.Dispose();
                }
            }
        }
        finally
        {
            // The sequential fixture owns the only desktop. Its presenters
            // subscribe to this static bus, so do not retain their disposed views
            // when the next acceptance case constructs another MainView.
            ((EventAggregator)EventAggregator.Instance).Dispose();
            try
            {
                if (previousNumLock.HasValue) OperatorNumLock.Set(previousNumLock.Value);
            }
            finally
            {
                Settings.Default.CodeEditor = previousEditor;
                Settings.Default.ScriptingDirectory = previousScripts;
                Settings.Default.CustomFunctionsDirectory = previousFunctions;
            }
        }
    }

    // Mono's Application.DoEvents skips control/menu preprocessing. These
    // keyboard regressions need the same RunLoop/ProcessCmdKey path as the app.
    private static void PumpOperatorInput()
    {
        using (var context = new ApplicationContext())
        using (var timer = new Timer { Interval = 100 })
        {
            timer.Tick += (_, _) => context.ExitThread();
            timer.Start();
            Application.Run(context);
        }
    }

    private static void TypeOperatorX11(string text)
    {
        Xdotool("type --clearmodifiers --delay 35 " + text);
        PumpOperatorInput();
    }

    private static void KeyOperatorX11(string key)
    {
        Xdotool("key --clearmodifiers " + key);
        PumpOperatorInput();
    }

    private static void ClickOperatorX11(int x, int y)
    {
        Xdotool("mousemove " + x + " " + y + " click 1");
        PumpOperatorInput();
    }

    private static void WaitForOperatorInput(Func<bool> ready, string message)
    {
        var watch = Stopwatch.StartNew();
        while (!ready() && watch.ElapsedMilliseconds < 3000) PumpOperatorInput();
        Check(ready(), message);
    }

    private static class OperatorNumLock
    {
        internal static bool Enabled
        {
            get
            {
                var display = XOpenDisplay(IntPtr.Zero);
                Check(display != IntPtr.Zero, "Cannot query Num Lock without the authenticated X11 display.");
                try
                {
                    var name = XInternAtom(display, "Num Lock", 1);
                    int index, enabled = 0, physical;
                    Check(name != UIntPtr.Zero &&
                        XkbGetNamedIndicator(display, name, out index, out enabled, IntPtr.Zero, out physical) != 0,
                        "XKB did not expose the Num Lock indicator for native keypad acceptance.");
                    return enabled != 0;
                }
                finally { XCloseDisplay(display); }
            }
        }

        internal static void Set(bool enabled)
        {
            if (Enabled != enabled)
            {
                Xdotool("key Num_Lock");
                PumpOperatorInput();
            }
            Check(Enabled == enabled, "Native Num_Lock did not set the requested lock state: " + enabled + ".");
        }

        [DllImport("libX11.so.6", CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr XOpenDisplay(IntPtr name);
        [DllImport("libX11.so.6", CallingConvention = CallingConvention.Cdecl)]
        private static extern int XCloseDisplay(IntPtr display);
        [DllImport("libX11.so.6", CallingConvention = CallingConvention.Cdecl)]
        private static extern UIntPtr XInternAtom(IntPtr display, string name, int onlyIfExists);
        [DllImport("libX11.so.6", CallingConvention = CallingConvention.Cdecl)]
        private static extern int XkbGetNamedIndicator(IntPtr display, UIntPtr name, out int index,
            out int enabled, IntPtr map, out int physical);
    }

    // Observe the translated native message and managed editor events without
    // handling them. A swallowed key remains diagnosable even when no WM_CHAR
    // or KeyPress reaches the editor. XTEST input is supplied by xdotool above.
    private sealed class OperatorKeyTrace : IMessageFilter, IDisposable
    {
        private readonly Control input;
        private readonly string prefix;

        internal OperatorKeyTrace(Control input, string key)
        {
            this.input = input;
            prefix = "Operator input " + input.GetType().Name + " " + key + ": ";
            Console.WriteLine(prefix + "send XTEST key");
            Application.AddMessageFilter(this);
            input.KeyDown += KeyDown;
            input.KeyPress += KeyPress;
            input.KeyUp += KeyUp;
        }

        public bool PreFilterMessage(ref Message message)
        {
            if (message.Msg >= 0x0100 && message.Msg <= 0x0106)
                Console.WriteLine(prefix + "message=0x" + message.Msg.ToString("x4") +
                    " wParam=0x" + message.WParam.ToInt64().ToString("x") +
                    " target=" + Control.FromHandle(message.HWnd)?.GetType().Name);
            return false;
        }

        private void KeyDown(object sender, KeyEventArgs args) =>
            Console.WriteLine(prefix + "KeyDown=" + args.KeyData + " suppressed=" + args.SuppressKeyPress);

        private void KeyPress(object sender, KeyPressEventArgs args) =>
            Console.WriteLine(prefix + "KeyPress=U+" + ((int)args.KeyChar).ToString("X4") + " handled=" + args.Handled);

        private void KeyUp(object sender, KeyEventArgs args) =>
            Console.WriteLine(prefix + "KeyUp=" + args.KeyData);

        public void Dispose()
        {
            input.KeyDown -= KeyDown;
            input.KeyPress -= KeyPress;
            input.KeyUp -= KeyUp;
            Application.RemoveMessageFilter(this);
        }
    }
}
