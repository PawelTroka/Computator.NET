using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using CompletionMenu = Computator.NET.Desktop.Controls.AutocompleteMenu.AutocompleteMenu;
using CompletionList = Computator.NET.Desktop.Controls.AutocompleteMenu.AutocompleteListView;

internal static partial class Program
{
    // Extend the existing mapped-X11 popup fixture with real XTEST wheel input.
    // Mono's DoEvents dispatches IMessageFilter before window messages, so this
    // exercises the focus-to-hover route without calling OnMouseWheel directly.
    private static void ReferenceMouseWheelX11(TextBox input, CompletionMenu menu,
        ToolStripDropDown help, Control reference)
    {
        var description = (ScrollableControl)reference.Controls.Find("referenceDescription", true).Single();
        var tabs = reference.Controls.OfType<TabControl>().Single();
        var list = (CompletionList)menu.ListView;
        var originalText = input.Text;
        var originalSelection = menu.SelectedItemIndex;
        var originalListOffset = list.VerticalScroll.Value;
        var canvasWheels = 0;
        var editorWheels = 0;
        var listWheels = 0;
        MouseEventHandler canvasHandler = (_, _) => canvasWheels++;
        MouseEventHandler editorHandler = (_, _) => editorWheels++;
        MouseEventHandler listHandler = (_, _) => listWheels++;
        description.MouseWheel += canvasHandler;
        input.MouseWheel += editorHandler;
        list.MouseWheel += listHandler;
        Action stable = () =>
        {
            Check(menu.Visible && help.Visible, "Reference wheel input dismissed a popup.");
            Check(input.ContainsFocus && input.Text == originalText,
                "Hover scrolling changed editor focus or its text.");
            Check(menu.SelectedItemIndex == originalSelection && list.VerticalScroll.Value == originalListOffset,
                "Reference wheel input moved the completion selection or list viewport.");
        };
        try
        {
            Check(description.VerticalScroll.Visible && input.ContainsFocus,
                "Wheel acceptance requires a painted long reference and the focused editor.");
            Run("mono-reference-wheel-hover-preserves-editor-and-list", () =>
            {
                description.AutoScrollPosition = Point.Empty;
                var before = canvasWheels;
                WheelReferenceX11(description, 5);
                Check(description.AutoScrollPosition.Y < 0, "Hovering the description did not scroll down.");
                Check(canvasWheels == before + 1, "One native wheel notch was not delivered exactly once.");
                stable();
                WheelReferenceX11(description, 4);
                Check(description.AutoScrollPosition.Y == 0 && canvasWheels == before + 2,
                    "One native wheel-up did not return to the starting position exactly once.");
                stable();
            });
            Run("mono-reference-wheel-top-boundary-does-not-leak", () =>
            {
                description.AutoScrollPosition = Point.Empty;
                var before = editorWheels;
                WheelReferenceX11(description, 4, 3);
                Check(description.AutoScrollPosition.Y == 0, "Wheel-up passed the top boundary.");
                WheelReferenceX11(description, 5);
                Check(description.AutoScrollPosition.Y < 0, "Wheel-down failed after reaching the top boundary.");
                WheelReferenceX11(description, 4, 3);
                Check(description.AutoScrollPosition.Y == 0 && editorWheels == before,
                    "Top-boundary wheel input leaked to the editor or failed to clamp.");
                stable();
            });
            Run("mono-reference-wheel-bottom-boundary-does-not-leak", () =>
            {
                description.AutoScrollPosition = new Point(0, description.AutoScrollMinSize.Height);
                PumpX11();
                var bottom = description.AutoScrollPosition.Y;
                var before = editorWheels;
                Check(bottom < 0, "The reference did not expose a bottom scroll boundary.");
                WheelReferenceX11(description, 5, 3);
                Check(description.AutoScrollPosition.Y == bottom, "Wheel-down passed the bottom boundary.");
                WheelReferenceX11(description, 4);
                Check(description.AutoScrollPosition.Y > bottom, "Wheel-up failed after reaching the bottom boundary.");
                WheelReferenceX11(description, 5, 3);
                Check(description.AutoScrollPosition.Y == bottom && editorWheels == before,
                    "Bottom-boundary wheel input leaked to the editor or failed to clamp.");
                stable();
            });
            Run("mono-reference-wheel-over-list-keeps-native-routing", () =>
            {
                var offset = description.AutoScrollPosition.Y;
                var before = editorWheels + listWheels;
                WheelReferenceX11(list, 5);
                Check(description.AutoScrollPosition.Y == offset,
                    "Wheel-over-list scrolled the reference instead.");
                Check(editorWheels + listWheels > before,
                    "The reference intercepted an unrelated list-area wheel event.");
                stable();
            });
            Run("mono-reference-wheel-header-excludes-description", () =>
            {
                var offset = description.AutoScrollPosition.Y;
                var before = canvasWheels;
                var tab = tabs.GetTabRect(0);
                var point = tabs.PointToScreen(new Point(tab.Left + tab.Width / 2, tab.Top + tab.Height / 2));
                WheelReferenceAtX11(point, 5, 1);
                Check(canvasWheels == before && description.AutoScrollPosition.Y == offset,
                    "A wheel event over the tab header scrolled the description.");
                stable();
            });
            Run("mono-reference-wheel-focused-canvas-scrolls-once", () =>
            {
                description.AutoScrollPosition = Point.Empty;
                description.Focus();
                PumpX11();
                Check(description.ContainsFocus, "The focused-canvas wheel precondition was not established.");
                var before = canvasWheels;
                WheelReferenceX11(description, 5);
                Check(description.AutoScrollPosition.Y < 0 && canvasWheels == before + 1,
                    "A native wheel notch at the focused canvas was lost or dispatched twice.");
                WheelReferenceX11(description, 4);
                Check(description.AutoScrollPosition.Y == 0 && canvasWheels == before + 2,
                    "A native wheel-up at the focused canvas was lost or dispatched twice.");
            });
        }
        finally
        {
            description.MouseWheel -= canvasHandler;
            input.MouseWheel -= editorHandler;
            list.MouseWheel -= listHandler;
            description.AutoScrollPosition = Point.Empty;
            input.Focus();
            PumpX11();
        }
        stable();
    }

    private static void WheelReferenceX11(Control target, int button, int count = 1)
    {
        var point = target.PointToScreen(new Point(target.ClientSize.Width / 2, target.ClientSize.Height / 2));
        WheelReferenceAtX11(point, button, count);
        Console.WriteLine("Native reference wheel: target=" + target.Name + " button=" + button +
            " count=" + count + " focus=" + target.ContainsFocus +
            " offset=" + (target as ScrollableControl)?.AutoScrollPosition);
    }

    private static void WheelReferenceAtX11(Point point, int button, int count)
    {
        Xdotool("mousemove " + point.X + " " + point.Y);
        PumpX11();
        Xdotool("click --repeat " + count + " --delay 8 " + button);
        PumpX11();
    }
}
