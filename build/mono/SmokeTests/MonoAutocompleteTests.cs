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
using Computator.NET.Core.Autocompletion.DataSource;
using Computator.NET.Core.Model;
using Computator.NET.DataTypes.SettingsTypes;
using CompletionMenu = Computator.NET.Desktop.Controls.AutocompleteMenu.AutocompleteMenu;
using CompletionList = Computator.NET.Desktop.Controls.AutocompleteMenu.AutocompleteListView;
using Settings = Computator.NET.Core.Properties.Settings;

internal static partial class Program
{
    private static readonly Lazy<System.Collections.Generic.IDictionary<string, FunctionInfo>> AutocompleteReferenceCatalog =
        new Lazy<System.Collections.Generic.IDictionary<string, FunctionInfo>>(() => new FunctionsDetailsFileSource().Details);
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
            var reference = help.Items.OfType<ToolStripControlHost>().Single().Control;
            if (Environment.GetEnvironmentVariable("COMPUTATOR_REFERENCE_FOCUS_TRACE") == "1")
            {
                host.Closed += (_, e) => Console.WriteLine("Completion closed: " + e.CloseReason + Environment.NewLine + Environment.StackTrace);
                help.Closed += (_, e) => Console.WriteLine("Reference closed: " + e.CloseReason + Environment.NewLine + Environment.StackTrace);
                input.LostFocus += (_, _) => Console.WriteLine("Editor LostFocus, help focus=" + help.ContainsFocus + ", pointer=" + Control.MousePosition);
                input.GotFocus += (_, _) => Console.WriteLine("Editor GotFocus");
                input.MouseDown += (_, args) => Console.WriteLine("Editor MouseDown, focus=" + input.Focused + ", clicks=" + args.Clicks + ", pointer=" + Control.MousePosition);
                window.LostFocus += (_, _) => Console.WriteLine("Owner LostFocus, help focus=" + help.ContainsFocus + ", pointer=" + Control.MousePosition);
            }
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
                // The managed Show/Activate calls do not acknowledge native mapping.
                // Wait for this exact owned XID before assigning native focus.
                NativeX11Input.FocusOwnedWindow(window.Handle, PumpX11);
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
                Check(reference.Text.Contains("Sine reference") &&
                    (bool)reference.GetType().GetProperty("ReadOnly").GetValue(reference, null),
                    "The reference description is missing or editable.");
                Check(input.ContainsFocus, "Opening autocomplete moved keyboard focus away from the editor.");
                InspectX11WindowStack(window, input, host, list, help);
                CaptureAutocompleteX11(window, host, help, "autocomplete-after.png");
                ExerciseRichReferenceX11(window, input, menu, host, help, reference);

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
        var shipped = AutocompleteReferenceCatalog.Value;
        FunctionInfo source;
        var description = shipped.TryGetValue(signature, out source) ? source.Description :
            "<p>Readable help for " + signature + " with a complete function description.</p>";
        return new AutocompleteItem(signature)
        {
            Details = new FunctionInfo
            {
                Title = title,
                Signature = signature,
                Description = description
            }
        };
    }

    private static void ExerciseRichReferenceX11(Form window, TextBox input, CompletionMenu menu,
        ToolStripDropDown host, ToolStripDropDown help, Control reference)
    {
        var tabs = reference.Controls.OfType<TabControl>().Single();
        var description = (ScrollableControl)reference.Controls.Find("referenceDescription", true).Single();
        Check(tabs.SelectedIndex == 0 && description.AutoScrollMinSize.Height > description.ClientSize.Height,
            "The actual illustrated reference must open in a scrollable Description tab.");
        var textTab = tabs.GetTabRect(1);
        var textPoint = tabs.PointToScreen(new Point(textTab.Left + textTab.Width / 2, textTab.Top + textTab.Height / 2));
        MoveAndClickX11(textPoint.X, textPoint.Y);
        PumpX11();
        // Retain native screen pixels even on failure, before any cleanup hides
        // the evidence. This captures the real X11 window stack, not a bitmap
        // fabricated from the renderer's Draw method.
        InspectX11WindowStack(window, input, host, (Control)menu.ListView, help);
        Check(menu.Visible && help.Visible && tabs.SelectedIndex == 1,
            "Clicking the reference Text tab closed autocomplete/help before the actual X11 click completed.");
        var readable = (RichTextBox)reference.Controls.Find("referenceText", true).Single();
        Check(readable.Visible && readable.ReadOnly && readable.Text.Contains("Sine reference"),
            "The reference Text tab is not usable.");
        CaptureAutocompleteX11(window, host, help, "autocomplete-reference-text.png");

        var descriptionTab = tabs.GetTabRect(0);
        var descriptionPoint = tabs.PointToScreen(new Point(descriptionTab.Left + descriptionTab.Width / 2, descriptionTab.Top + descriptionTab.Height / 2));
        MoveAndClickX11(descriptionPoint.X, descriptionPoint.Y);
        Check(menu.Visible && help.Visible && tabs.SelectedIndex == 0, "Returning to Description closed the help.");
        var scrollPoint = description.PointToScreen(new Point(description.Width - SystemInformation.VerticalScrollBarWidth / 2,
            description.Height - SystemInformation.HorizontalScrollBarHeight / 2));
        MoveAndClickX11(scrollPoint.X, scrollPoint.Y);
        PumpX11();
        Check(menu.Visible && help.Visible && description.AutoScrollPosition.Y < 0,
            "The actual tooltip scrollbar failed to scroll the illustrated reference or dismissed it.");
        CaptureAutocompleteX11(window, host, help, "autocomplete-reference-scrolled.png");

        var editorPoint = input.PointToScreen(new Point(8, input.ClientSize.Height / 2));
        using (var other = new TextBox { Location = new Point(24, 220), Size = new Size(200, 24) })
        {
            window.Controls.Add(other);
            var otherPoint = other.PointToScreen(new Point(8, other.ClientSize.Height / 2));
            MoveAndClickX11(otherPoint.X, otherPoint.Y);
            InspectX11WindowStack(window, input, host, (Control)menu.ListView, help);
            Check(other.Focused, "The real X11 click did not reach the unrelated editor.");
            WaitForX11(() => !menu.Visible && !help.Visible,
                "Leaving the reference for an unrelated editor left autocomplete/help floating above it.");
            // Keep the other native control alive until focus has really moved
            // back; destroying the focused X11 window would invalidate this test.
            // Xvfb has no window manager to reactivate a popup's owner, just as
            // at fixture startup. Activate that actual X11 top-level window;
            // widget focus below must still come from the real pointer click.
            NativeX11Input.FocusOwnedWindow(window.Handle, PumpX11);
            menu.SetAutocompleteMenu(other, menu);
            try
            {
                MoveAndClickX11(editorPoint.X, editorPoint.Y);
                Check(input.Focused, "The real editor did not regain focus after reading the reference.");
                MoveAndClickX11(otherPoint.X, otherPoint.Y);
                Check(other.Focused && !input.Focused, "A shared completion menu focused its previous editor instead of the actually clicked one.");
                MoveAndClickX11(editorPoint.X, editorPoint.Y);
                Check(input.Focused && !other.Focused, "The actual editor did not recover after another attached editor was clicked.");
            }
            finally { menu.SetAutocompleteMenu(other, null); }
            KeyX11("End"); KeyX11("ctrl+space");
            WaitForX11(() => menu.Visible && help.Visible, "Completion did not reopen for the modal reference test.");
            window.Controls.Remove(other);
        }

        MoveAndClickX11(textPoint.X, textPoint.Y);
        Check(help.ContainsFocus, "The modal reference test must start from actual help focus.");
        using (var dialog = new Form { Text = "Mono reference modal acceptance", Size = new Size(240, 120), ShowInTaskbar = false })
        using (var pulse = new System.Windows.Forms.Timer { Interval = 100 })
        {
            dialog.Controls.Add(new Label { Dock = DockStyle.Fill, Text = "Reference dismissed for modal dialog", TextAlign = ContentAlignment.MiddleCenter });
            Exception failure = null; var ticks = 0;
            pulse.Tick += (_, _) =>
            {
                if (ticks == 0) CaptureReferenceDesktopX11(window, "autocomplete-reference-modal.png");
                try { Check(!menu.Visible && !help.Visible, "A modal owner dialog left autocomplete/reference visible."); }
                catch (Exception error) { failure = error; }
                if (++ticks >= 3 || failure != null) dialog.Close();
            };
            dialog.Shown += (_, _) => pulse.Start();
            dialog.ShowDialog(window); pulse.Stop();
            if (failure != null) throw failure;
            Check(ticks >= 3, "The modal reference guard was not observed across queued UI work.");
        }

        // Return through the native input queue and prove the original editor
        // still owns subsequent selection/insertion, rather than a hidden copy.
        NativeX11Input.FocusOwnedWindow(window.Handle, PumpX11);
        MoveAndClickX11(editorPoint.X, editorPoint.Y);
        Check(input.Focused, "The actual X11 editor click failed to restore keyboard focus.");
        KeyX11("End");
        TypeX11("n");
        Check(input.Text == "sin", "Real X11 typing after reference reading did not reach the displayed editor.");
        Check(input.Focused, "Passive reference refresh took keyboard focus from the editor.");
        CaptureAutocompleteX11(window, host, help, "autocomplete-reference-editor-typed.png");
        KeyX11("BackSpace");
        Check(input.Text == "si", "The real editor could not undo the test character after reading help.");
        KeyX11("ctrl+space");
        WaitForX11(() => menu.Visible && input.Focused, "Editor focus/completion did not recover after reading help.");
        Check(menu.SelectedItemIndex == 0 && input.Text == "si", "Reading help changed the real editor buffer or completion selection.");
        CaptureAutocompleteX11(window, host, help, "autocomplete-reference-editor-return.png");
    }

    private static void CaptureReferenceDesktopX11(Control anchor, string name)
    {
        var screen = Screen.PrimaryScreen.Bounds;
        using (var bitmap = new Bitmap(screen.Width, screen.Height))
        {
            using (var graphics = Graphics.FromImage(bitmap))
                NativeDesktopCapture.CopyFromScreen(graphics, screen.Location, Point.Empty, screen.Size, anchor);
            bitmap.Save(Path.Combine(outputDirectory, name), ImageFormat.Png);
        }
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

    private static void InspectX11WindowStack(Form window, Control input, ToolStripDropDown menu,
        Control list, ToolStripDropDown reference)
    {
        var details = "window=" + window.Bounds + " input=" + input.RectangleToScreen(input.ClientRectangle)
            + " menu=" + menu.Bounds + " list=" + list.RectangleToScreen(list.ClientRectangle)
            + " reference=" + reference.Bounds
            + " visible(menu,ref)=" + menu.Visible + "," + reference.Visible
            + " parent=" + list.Parent?.GetType().FullName + Environment.NewLine;
        using (var process = Process.Start(new ProcessStartInfo("xwininfo", "-root -tree")
        {
            UseShellExecute = false, RedirectStandardOutput = true,
            RedirectStandardError = true, CreateNoWindow = true
        }))
        {
            details += process.StandardOutput.ReadToEnd() + Environment.NewLine
                + process.StandardError.ReadToEnd();
            Check(process.WaitForExit(6000), "xwininfo hung during native window inspection.");
        }
        foreach (var popup in new[] { menu, reference })
        {
            var nativeId = "0x" + popup.Handle.ToInt64().ToString("x");
            using (var inspect = Process.Start(new ProcessStartInfo("xwininfo", "-id " + nativeId)
            {
                UseShellExecute = false, RedirectStandardOutput = true,
                RedirectStandardError = true, CreateNoWindow = true
            }))
            {
                details += nativeId + Environment.NewLine + inspect.StandardOutput.ReadToEnd()
                    + Environment.NewLine + inspect.StandardError.ReadToEnd();
                Check(inspect.WaitForExit(6000), "Native X11 map-state query hung.");
            }
        }
        File.WriteAllText(Path.Combine(outputDirectory, "autocomplete-x11-windows.log"), details);
        Console.WriteLine(details);
        var screen = Screen.FromControl(window).Bounds;
        using (var bitmap = new Bitmap(screen.Width, screen.Height))
        {
            using (var graphics = Graphics.FromImage(bitmap))
                NativeDesktopCapture.CopyFromScreen(graphics, screen.Location, Point.Empty, screen.Size, window);
            bitmap.Save(Path.Combine(outputDirectory, "autocomplete-entire-x11-screen.png"), ImageFormat.Png);
        }
    }

    private static void CaptureAutocompleteX11(Form window, ToolStripDropDown menu,
        ToolStripDropDown reference, string name)
    {
        var rect = Rectangle.Union(window.Bounds, Rectangle.Union(menu.Bounds, reference.Bounds));
        rect.Inflate(8, 8);
        rect.Intersect(Screen.FromControl(window).Bounds);
        Check(rect.Width > 0 && rect.Height > 0, "No visible autocomplete area to capture.");
        // Mapped X11 windows and WinForms Visible are not sufficient: on a
        // headless Mono runner a popup can be mapped but not yet *painted*.
        // The original screenshot check could pass on the form's own colors
        // while the actual menu and reference remained invisible on screen.
        var watch = Stopwatch.StartNew();
        var menuPainted = false;
        var helpPainted = false;
        using (var image = new Bitmap(rect.Width, rect.Height))
        {
            do
            {
                Application.DoEvents();
                using (var graphics = Graphics.FromImage(image))
                    NativeDesktopCapture.CopyFromScreen(graphics, rect.Location, Point.Empty, rect.Size, window);
                menuPainted = HasNativePopupPixels(image, rect.Location, menu.Bounds);
                helpPainted = HasNativePopupPixels(image, rect.Location, reference.Bounds);
                if (menuPainted && helpPainted)
                    break;
                // Request another actual WinForms/X11 paint. Never draw an
                // artificial popup onto the screenshot to satisfy acceptance.
                menu.Invalidate(true);
                reference.Invalidate(true);
                menu.Refresh();
                reference.Refresh();
                PumpX11();
            }
            while (watch.Elapsed < TimeSpan.FromSeconds(5));

            image.Save(Path.Combine(outputDirectory, name), ImageFormat.Png);
        }
        Check(menuPainted && helpPainted,
            "The Mono autocomplete windows are mapped but their real X11 pixels are missing: "
            + "completion painted=" + menuPainted + ", help painted=" + helpPainted
            + ". See " + name + " for the captured failure.");
    }

    private static bool HasNativePopupPixels(Bitmap capture, Point origin, Rectangle popupBounds)
    {
        var region = Rectangle.Intersect(
            new Rectangle(popupBounds.X - origin.X, popupBounds.Y - origin.Y,
                popupBounds.Width, popupBounds.Height),
            new Rectangle(Point.Empty, capture.Size));
        if (region.Width < 20 || region.Height < 20)
            return false;
        // The shipped Mono completion and read-only reference controls have
        // white item/text surfaces. Sample native screen pixels *inside* each
        // popup rather than counting the parent window's unrelated colors.
        // The Xvfb form background is beige and the X11 root is black;
        // neither can satisfy the white-surface threshold.
        var light = 0;
        var total = 0;
        for (var y = region.Top + 4; y < region.Bottom - 4; y += 4)
            for (var x = region.Left + 4; x < region.Right - 4; x += 4)
            {
                var pixel = capture.GetPixel(x, y);
                if (pixel.R >= 245 && pixel.G >= 245 && pixel.B >= 245) light++;
                total++;
            }
        return total > 0 && light * 4 > total;
    }
}
