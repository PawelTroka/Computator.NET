using System;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Windows.Forms;
using Computator.NET.Core.Autocompletion;
using Computator.NET.Core.Model;
using Computator.NET.DataTypes.SettingsTypes;
using CompletionMenu = Computator.NET.Desktop.Controls.AutocompleteMenu.AutocompleteMenu;
using CompletionList = Computator.NET.Desktop.Controls.AutocompleteMenu.AutocompleteListView;
using Settings = Computator.NET.Core.Properties.Settings;

internal static partial class Program
{
    // This is a real Mono/WinForms-on-X11 acceptance test. The xdotool events
    // come from XTEST, rather than invoking protected OnKeyDown/OnMouseClick
    // methods or testing a fabricated bitmap of a hidden control.
    private static void AutocompleteRealX11()
    {
        var previousTooltip = Settings.Default.TooltipType;
        using (var window = new Form
        {
            Text = "Mono completion X11 acceptance",
            Size = new Size(610, 310),
            Location = new Point(90, 100),
            ShowInTaskbar = false,
            StartPosition = FormStartPosition.Manual
        })
        using (var input = new TextBox { Location = new Point(24, 34), Size = new Size(340, 32) })
        using (var menu = new CompletionMenu(new SharedViewState()) { AppearInterval = 50 })
        {
            var list = (CompletionList)menu.ListView;
            var host = (ToolStripDropDown)typeof(CompletionMenu)
                .GetProperty("Host", BindingFlags.Instance | BindingFlags.NonPublic)
                .GetValue(menu, null);
            var help = (ToolStripDropDown)typeof(CompletionList)
                .GetField("toolTip", BindingFlags.Instance | BindingFlags.NonPublic)
                .GetValue(list);
            var reference = (RichTextBox)help.Items.OfType<ToolStripControlHost>().Single().Control;
            try
            {
                Settings.Default.TooltipType = TooltipType.Default;
                menu.SetAutocompleteItems(new[]
                {
                    Completion("sin(x)", "Sine reference"),
                    Completion("sinh(x)", "Hyperbolic sine reference"),
                    Completion("sign(x)", "Sign reference")
                });
                window.Controls.Add(input);
                menu.SetAutocompleteMenu(input, menu);
                window.Show();
                window.Activate();
                input.Focus();
                PumpX11();
                // xvfb-run does not launch a window manager. Set the real X11
                // input focus explicitly; Form.Activate alone is insufficient.
                Xdotool("search --onlyvisible --name Mono.completion.X11.acceptance windowfocus --sync");
                input.Focus();
                PumpX11();

                TypeX11("si");
                WaitForX11(() => menu.Visible, "Typing did not open the Mono autocomplete dropdown."
                    + " text='" + input.Text + "', focus=" + input.Focused
                    + ", containsFocus=" + input.ContainsFocus
                    + ", matches=" + menu.VisibleItems.Count + ", windowFocus=" + window.ContainsFocus);
                Check(input.Text == "si", "Native X11 keystrokes were not received by the editor.");
                Check(menu.VisibleItems.Count == 3, "Completion filtering lost the matching expressions.");
                Check(host.Width >= 200 && host.Width < 660, "The Mono dropdown is empty or oversized: " + host.Bounds);
                Check(host.Height >= list.ItemHeight && host.Height <= 380, "The dropdown height does not fit visible rows.");
                Check(Screen.FromControl(input).WorkingArea.Contains(host.Bounds), "Completion list is outside the screen.");
                WaitForX11(() => help.Visible, "The Mono function reference failed to open.");
                Check(Screen.FromControl(input).WorkingArea.Contains(help.Bounds), "Reference help is clipped beyond the screen.");
                Check(reference.Text.Contains("Sine reference") && reference.ReadOnly,
                    "The native text reference is missing or editable.");
                Check(input.ContainsFocus, "Opening autocomplete moved keyboard focus away from the editor.");
                CaptureAutocompleteX11(window, host, help, "autocomplete-after.png");

                // Keyboard selection and insertion through the real X11 input
                // queue (including the existing fragment replacement logic).
                KeyX11("Down");
                PumpX11();
                Check(menu.SelectedItemIndex == 1, "ArrowDown did not change the highlighted suggestion.");
                KeyX11("Return");
                PumpX11();
                Equal(input.Text, "sinh(x)");
                Check(!menu.Visible && !help.Visible, "Accepting a suggestion left a topmost popup behind.");

                // A single real mouse click must not be swallowed by the
                // TextBox.LostFocus race before the list receives it.
                ResetAutocompleteInput(menu, input);
                TypeX11("si");
                WaitForX11(() => menu.Visible, "Mouse selection did not reopen completion.");
                var item = menu.GetItemRectangle(2);
                var point = list.PointToScreen(new Point(item.Left + Math.Min(50, item.Width / 2),
                    item.Top + item.Height / 2));
                MoveAndClickX11(point.X, point.Y);
                PumpX11();
                Equal(input.Text, "sign(x)");
                Check(!menu.Visible, "Mouse acceptance did not close completion.");

                // More items than the viewport: native PageDown and Up must
                // move selection and reveal the chosen row via scrolling.
                menu.SetAutocompleteItems(Enumerable.Range(0, 42)
                    .Select(i => Completion("si" + i.ToString("D2") + "(x)", "Scrollable reference " + i))
                    .ToArray());
                ResetAutocompleteInput(menu, input);
                TypeX11("si");
                WaitForX11(() => menu.Visible, "Scrolling scenario did not open completion.");
                Check(menu.VisibleItems.Count == 42, "Long suggestions were incorrectly filtered.");
                Check(list.VerticalScroll.Visible, "The Mono completion list lacks a usable vertical scrollbar.");
                KeyX11("Next");
                KeyX11("Next");
                PumpX11();
                Check(menu.SelectedItemIndex > 4 && list.VerticalScroll.Value > 0,
                    "PageDown failed to advance and scroll the actual completion list.");
                KeyX11("Up");
                PumpX11();
                Check(menu.SelectedItemIndex >= 0, "ArrowUp left no valid selection.");
                KeyX11("Escape");
                PumpX11();
                Check(!menu.Visible && !help.Visible, "Escape did not dismiss both autocomplete windows.");

                // Repeated show/close and Ctrl+Space through X11 must not
                // leave orphaned help/toolstrip windows.
                for (var i = 0; i < 3; i++)
                {
                    input.Focus();
                    KeyX11("ctrl+space");
                    WaitForX11(() => menu.Visible, "Ctrl+Space failed after a previous close.");
                    KeyX11("Escape");
                    PumpX11();
                    Check(!menu.Visible && !help.Visible, "A reopened popup remained visible after Escape.");
                }

                // Near the lower-right display edge the same native dropdown
                // must flip above the caret and keep its reference on-screen.
                window.Location = new Point(Screen.PrimaryScreen.WorkingArea.Right - window.Width - 5,
                    Screen.PrimaryScreen.WorkingArea.Bottom - window.Height - 5);
                input.Location = new Point(window.ClientSize.Width - input.Width - 16,
                    window.ClientSize.Height - input.Height - 16);
                window.Activate();
                ResetAutocompleteInput(menu, input);
                TypeX11("si");
                WaitForX11(() => menu.Visible, "Edge-position completion did not open.");
                var area = Screen.FromControl(input).WorkingArea;
                Check(area.Contains(host.Bounds), "Completion dropdown is clipped at the bottom/right screen edge.");
                WaitForX11(() => help.Visible, "Edge-position function reference did not open.");
                Check(area.Contains(help.Bounds), "Function description is off-screen near the edge.");
                Check(reference.Text.Contains("Scrollable reference"), "Wrong reference shown after reflow.");
                menu.Close();
                Check(!host.Visible && !help.Visible, "Final close left a floating reference behind.");
            }
            finally
            {
                menu.Close();
                menu.SetAutocompleteMenu(input, null);
                Settings.Default.TooltipType = previousTooltip;
            }
        }
    }

    private static AutocompleteItem Completion(string signature, string title)
    {
        return new AutocompleteItem(signature)
        {
            Details = new FunctionInfo
            {
                Title = title,
                Signature = signature,
                Description = "<p>Readable help for " + signature + " with a complete function description.</p>"
            }
        };
    }

    private static void ResetAutocompleteInput(CompletionMenu menu, TextBox input)
    {
        menu.Close();
        input.Text = "";
        input.SelectionStart = 0;
        input.Focus();
        PumpX11();
    }

    private static void TypeX11(string characters)
    {
        Xdotool("type --clearmodifiers --delay 35 " + characters);
        PumpX11();
    }

    private static void KeyX11(string key)
    {
        Xdotool("key --clearmodifiers " + key);
        PumpX11();
    }

    private static void MoveAndClickX11(int x, int y)
    {
        Xdotool("mousemove " + x + " " + y + " click 1");
        PumpX11();
    }

    private static void Xdotool(string arguments)
    {
        using (var process = Process.Start(new ProcessStartInfo("xdotool", arguments)
        {
            UseShellExecute = false, RedirectStandardError = true, CreateNoWindow = true
        }))
        {
            if (!process.WaitForExit(6000))
            {
                process.Kill();
                throw new InvalidOperationException("Timed out sending real X11 events: " + arguments);
            }
            Check(process.ExitCode == 0, "xdotool failed: " + process.StandardError.ReadToEnd());
        }
    }

    private static void WaitForX11(Func<bool> condition, string error)
    {
        var timer = Stopwatch.StartNew();
        while (!condition() && timer.ElapsedMilliseconds < 3000)
            PumpX11();
        Check(condition(), error);
    }

    private static void PumpX11()
    {
        for (var i = 0; i < 6; i++)
        {
            Application.DoEvents();
            Thread.Sleep(12);
        }
    }

    private static void CaptureAutocompleteX11(Form window, ToolStripDropDown menu,
        ToolStripDropDown reference, string name)
    {
        var rect = Rectangle.Union(window.Bounds, Rectangle.Union(menu.Bounds, reference.Bounds));
        rect.Inflate(8, 8);
        rect.Intersect(Screen.FromControl(window).Bounds);
        Check(rect.Width > 0 && rect.Height > 0, "No visible autocomplete area to capture.");
        using (var image = new Bitmap(rect.Width, rect.Height))
        {
            using (var graphics = Graphics.FromImage(image))
                graphics.CopyFromScreen(rect.Location, Point.Empty, rect.Size);
            AssertImageHasVariation(image, 20);
            image.Save(Path.Combine(outputDirectory, name), ImageFormat.Png);
        }
    }
}
