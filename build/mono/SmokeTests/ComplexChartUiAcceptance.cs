using System;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Threading;
using System.Windows.Forms;
using Computator.NET.Charting.ComplexCharting;
using Computator.NET.Core.Abstract.Controls;
using Computator.NET.Core.Model;
using Computator.NET.DataTypes;
using Computator.NET.DataTypes.Events;
using Computator.NET.Desktop.Bootstrapping;
using Computator.NET.Desktop.Views;

internal static partial class Program
{
    // Run inside the *actual* bootstrapped desktop window, not a detached chart
    // created for GetImage(). Capture the X11 screen itself, not DrawToBitmap.
    private static void VerifyMonoComplexChartVisible(MainView window,
        WinFormsBootstrapper bootstrapper, ISharedViewState state)
    {
        state.CurrentView = ViewName.Charting;
        EventAggregator.Instance.Publish(new CalculationsModeChangedEvent(CalculationsMode.Complex));
        var view = bootstrapper.Create<ChartingView>();
        var chart = (ComplexChart)view.ComplexChart;
        chart.Quality = 25; // Keep interactive acceptance quick on hosted Mono.
        Application.DoEvents();
        Console.WriteLine("Complex UI initial: " + DescribeComplexLayout(chart));
        Check(window.Visible && chart.Visible && chart.IsHandleCreated && chart.Width > 200 && chart.Height > 150,
            "Complex plot does not have a visible, sized, native X11 control: " + DescribeComplexLayout(chart));

        using (var blank = ScreenshotComplexControl(chart, "complex-visible-before.png"))
            Console.WriteLine("Complex UI before draw colored pixels: " + CountDomainColorPixels(blank));

        bootstrapper.Create<ITextProvider>().Text = "z²";
        var draw = Find(window, "addToChartButton") as Button;
        Check(draw != null && draw.Visible && draw.Enabled, "Real desktop Draw button is not available.");
        draw.PerformClick();

        // The production chart uses async sampling. Pump the real message loop
        // until its background calculation and native invalidation have completed.
        var timer = Stopwatch.StartNew();
        Bitmap last = null;
        int colored = 0;
        while (timer.Elapsed < TimeSpan.FromSeconds(25))
        {
            Application.DoEvents();
            if (last != null) last.Dispose();
            last = ScreenshotComplexControl(chart, null);
            colored = CountDomainColorPixels(last);
            if (colored > 200) break;
            Thread.Sleep(60);
        }
        try
        {
            last.Save(System.IO.Path.Combine(outputDirectory, "complex-visible-after.png"), ImageFormat.Png);
            Console.WriteLine("Complex UI after draw: " + DescribeComplexLayout(chart) +
                "; colored pixels: " + colored + "; elapsed: " + timer.Elapsed);
            Check(colored > 200,
                "z² was not painted in the actual Linux/X11 window despite clicking Draw; " +
                "on-screen domain color pixels: " + colored);
        }
        finally { if (last != null) last.Dispose(); }

        using (var export = chart.GetImage(320, 220))
            AssertImageHasVariation(export, 40);
        // Restore the original application mode for existing desktop workflow checks.
        EventAggregator.Instance.Publish(new CalculationsModeChangedEvent(CalculationsMode.Real));
    }

    private static string DescribeComplexLayout(ComplexChart chart)
    {
        if (chart.Parent == null) return "unparented";
        return string.Format("bounds={0} visible={1} handle={2} childIndex={3} siblings=[{4}]",
            chart.Bounds, chart.Visible, chart.IsHandleCreated,
            chart.Parent.Controls.GetChildIndex(chart),
            string.Join("; ", chart.Parent.Controls.Cast<Control>().Select(c =>
                c.GetType().Name + ":" + c.Visible + ":" + c.Bounds).ToArray()));
    }

    private static Bitmap ScreenshotComplexControl(Control chart, string name)
    {
        var rect = chart.RectangleToScreen(chart.ClientRectangle);
        Check(rect.Width > 0 && rect.Height > 0,
            "Complex chart has no X11 rectangle.");
        var bitmap = new Bitmap(rect.Width, rect.Height);
        using (var graphics = Graphics.FromImage(bitmap))
            graphics.CopyFromScreen(rect.Location, Point.Empty, rect.Size);
        if (name != null)
            bitmap.Save(System.IO.Path.Combine(outputDirectory, name), ImageFormat.Png);
        return bitmap;
    }

    private static int CountDomainColorPixels(Bitmap bitmap)
    {
        int count = 0;
        for (int x = 10; x < bitmap.Width - 10; x += 5)
        for (int y = 10; y < bitmap.Height - 10; y += 5)
        {
            var p = bitmap.GetPixel(x,y);
            if (Math.Max(p.R,Math.Max(p.G,p.B)) - Math.Min(p.R,Math.Min(p.G,p.B)) > 45 &&
                Math.Max(p.R,Math.Max(p.G,p.B)) > 85)
                count++;
        }
        return count;
    }
}
