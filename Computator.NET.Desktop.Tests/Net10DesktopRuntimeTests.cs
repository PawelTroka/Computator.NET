using System;
using System.Drawing;
using System.Linq;
using System.Threading;
using System.Windows.Forms;
using System.Windows.Forms.Integration;
using Computator.NET.Charting.Chart3D.UI;
using Computator.NET.Charting.ComplexCharting;
using Computator.NET.Charting.Controls;
using Computator.NET.Core.Abstract.Controls;
using Computator.NET.Core.Bootstrapping;
using Computator.NET.Core.Natives;
using Computator.NET.DataTypes.SettingsTypes;
using Computator.NET.Desktop.Controls.CodeEditors;
using Computator.NET.Desktop.Controls.CodeEditors.AvalonEdit;
using NUnit.Framework;

namespace Computator.NET.Desktop.Tests
{
    [TestFixture]
    [Apartment(ApartmentState.STA)]
    public class Net10DesktopRuntimeTests
    {
        [TestCase(CodeEditorType.Scintilla)]
        [TestCase(CodeEditorType.AvalonEdit)]
        [TestCase(CodeEditorType.TextEditor)]
        public void EveryEditorPreservesUnicodeAndDocumentsOnNet10(CodeEditorType kind)
        {
            NUnit.Framework.Assert.That(Environment.Version.Major, Is.EqualTo(10));
            GSLInitializer.Initialize();
            var bootstrapper = new CoreBootstrapper();
            using (bootstrapper.Container)
            {
                ICodeEditorControl editor = kind switch
                {
                    CodeEditorType.Scintilla => bootstrapper.Create<ScintillaCodeEditorControl>(),
                    CodeEditorType.AvalonEdit => bootstrapper.Create<AvalonEditCodeEditor>(),
                    _ => bootstrapper.Create<TextEditorCodeEditor>()
                };
                using (var host = new ElementHost())
                using (var font = new Font("Consolas", 12f))
                {
                    try
                    {
                        editor.SetFont(font);
                        if (editor is System.Windows.UIElement wpf)
                        {
                            host.Child = wpf;
                            NUnit.Framework.Assert.That(host.Handle, Is.Not.EqualTo(IntPtr.Zero));
                        }
                        else
                        {
                            NUnit.Framework.Assert.That(((Control)editor).Handle, Is.Not.EqualTo(IntPtr.Zero));
                        }
                        const string first = "var α = π + 2·3;";
                        editor.NewDocument("first.tsl");
                        editor.Text = first;
                        editor.NewDocument("second.tsl");
                        editor.Text = "write(42);";
                        editor.SwitchDocument("first.tsl");
                        NUnit.Framework.Assert.That(editor.Text, Is.EqualTo(first));
                        editor.RenameDocument("first.tsl", "renamed.tsl");
                        NUnit.Framework.Assert.That(editor.ContainsDocument("renamed.tsl"), Is.True);
                        NUnit.Framework.Assert.That(editor.ContainsDocument("first.tsl"), Is.False);
                        editor.SwitchDocument("second.tsl");
                        NUnit.Framework.Assert.That(editor.Text, Is.EqualTo("write(42);"));
                    }
                    finally
                    {
                        host.Child = null;
                        (editor as IDisposable)?.Dispose();
                    }
                }
            }
        }

        [Test]
        public void ComplexChartDialogUsesStandardWinFormsColorControls()
        {
            using (var dialog = new EditComplexChartWindow())
            {
                NUnit.Framework.Assert.That(dialog.Handle, Is.Not.EqualTo(IntPtr.Zero));
                var swatches = Descendants(dialog).OfType<ColorSwatch>().ToArray();
                NUnit.Framework.Assert.That(swatches.Length, Is.EqualTo(3));
                NUnit.Framework.Assert.That(swatches.All(s => s.TabStop), Is.True);
                swatches[0].FillColor = Color.Coral;
                NUnit.Framework.Assert.That(swatches[0].BackColor, Is.EqualTo(Color.Coral));
            }
        }

        [Test]
        public void Wpf3DChartSettingsDialogLoadsWithoutPowerPacks()
        {
            var chart = new Chart3DControl();
            using (var host = new ElementHost { Child = chart, BackColor = Color.White })
            using (var dialog = new EditChart3DWindow(chart, host))
            {
                NUnit.Framework.Assert.That(dialog.Handle, Is.Not.EqualTo(IntPtr.Zero));
                NUnit.Framework.Assert.That(Descendants(dialog).OfType<ColorSwatch>().Count(), Is.EqualTo(3));
            }
        }

        private static System.Collections.Generic.IEnumerable<Control> Descendants(Control parent)
        {
            foreach (Control child in parent.Controls)
            {
                yield return child;
                foreach (var descendant in Descendants(child)) yield return descendant;
            }
        }
    }
}
