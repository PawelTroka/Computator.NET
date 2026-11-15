using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using Computator.NET.Core.Configuration;
using Computator.NET.Core.Model;
using Computator.NET.DataTypes.Text;
using Computator.NET.Desktop.Bootstrapping;
using Computator.NET.Desktop.Controls;
using Computator.NET.Desktop.Controls.CodeEditors;

internal static partial class Program
{
    private static void MathematicalFontRendering()
    {
        using (var math = CustomFonts.GetMathFont(24))
        using (var script = CustomFonts.GetScriptingFont(24))
        using (var image = new Bitmap(1280, 560))
        using (var graphics = Graphics.FromImage(image))
        {
            graphics.Clear(Color.White);
            var symbols = SpecialSymbols.SuperscriptsWithoutSpace;
            var y = 8;
            foreach (var font in new[] { math, script })
            {
                graphics.DrawString("2¹ᱸ² + 2ˣ˸²   " + font.Name, font, Brushes.Black, 8, y);
                y += 50;
                foreach (var part in new[] { symbols.Substring(0, symbols.Length / 2), symbols.Substring(symbols.Length / 2) })
                {
                    graphics.DrawString(part, font, Brushes.Black, 8, y);
                    y += 50;
                }
                TextRenderer.DrawText(graphics, "2¹ᱸ² + 2ˣ˸²", font, new Point(8, y), Color.Black);
                y += 75;
            }
            image.Save(Path.Combine(outputDirectory, "mono-font-glyphs.png"), ImageFormat.Png);
            foreach (var font in new[] { math, script })
                AssertMathematicalGlyphShapes(font);
        }
    }

    private static void MathematicalFontSettings()
    {
        // Production startup has registered files without creating a FontFamily.
        // This case runs first: GDI+ is now active, but neither saved Noto names
        // nor default app fonts have been requested. Restoration must not cache
        // a missing family before resolving the saved name.
        using (var existing = new Font(FontFamily.GenericSansSerif, 12))
        using (var image = new Bitmap(80, 40))
        using (var graphics = Graphics.FromImage(image))
        {
            graphics.DrawString("Before", existing, Brushes.Black, 0, 0);
            foreach (var name in new[] { "Noto Serif", "Noto Sans Mono" })
            using (var restored = new FontDocument { Name = name, Size = 24 }.ToFont())
            {
                Equal(restored.Name, name);
                AssertMathematicalGlyphShapes(restored);
            }
            graphics.DrawString("After", existing, Brushes.Black, 0, 15);
        }

        var directory = Path.Combine(outputDirectory, "font-settings");
        Directory.CreateDirectory(directory);
        var index = 0;
        foreach (var style in new[] { FontStyle.Regular, FontStyle.Bold, FontStyle.Italic, FontStyle.Bold | FontStyle.Italic })
        foreach (var unit in new[] { GraphicsUnit.Point, GraphicsUnit.Pixel })
        {
            var document = new SettingsDocument
            {
                ScriptingDirectory = directory, CustomFunctionsDirectory = directory, WorkingDirectory = directory,
                ExpressionFont = new FontDocument { Name = "Cambria", Size = 19.25f, Style = style, Unit = unit },
                ScriptingFont = new FontDocument { Name = "Consolas", Size = 17.5f, Style = style, Unit = unit }
            };
            var file = Path.Combine(directory, "fonts-" + index++ + ".settings");
            using (var math = document.ExpressionFont.ToFont())
            using (var script = document.ScriptingFont.ToFont())
            {
                document.ExpressionFont = FontDocument.FromFont(math);
                document.ScriptingFont = FontDocument.FromFont(script);
                SettingsPersistence.Save(file, document);
                var restored = SettingsPersistence.Load(file);
                using (var restoredMath = restored.ExpressionFont.ToFont())
                using (var restoredScript = restored.ScriptingFont.ToFont())
                {
                    Check(restoredMath.Style == style && restoredScript.Style == style, "Font style was lost after JSON reload.");
                    Check(restoredMath.Unit == unit && restoredScript.Unit == unit, "Font unit was lost after JSON reload.");
                    Near(restoredMath.Size, 19.25f, 0);
                    Near(restoredScript.Size, 17.5f, 0);
                    Equal(FontDocument.FromFont(restoredMath).Name, document.ExpressionFont.Name);
                    Equal(FontDocument.FromFont(restoredScript).Name, document.ScriptingFont.Name);
                    AssertMathematicalGlyphShapes(restoredMath);
                    AssertMathematicalGlyphShapes(restoredScript);
                }
            }
        }

        // Explicit alternate font choices remain exact, including their style
        // and units; opening a document must not replace them with app defaults.
        using (var custom = new Font(FontFamily.GenericMonospace, 18, FontStyle.Italic, GraphicsUnit.Pixel))
        {
            Same(CustomFonts.GetFontFromFont(custom), custom, "A custom family was replaced with the default font.");
            var document = FontDocument.FromFont(custom);
            using (var restored = document.ToFont())
            {
                Equal(restored.Name, custom.Name);
                Check(restored.Style == custom.Style && restored.Unit == custom.Unit, "Custom font settings changed.");
                Near(restored.Size, custom.Size, 0);
            }
        }
    }

    private static void MathematicalEditorDocuments()
    {
        const string expressionSource = "2¹ᱸ² + 2ˣ˸²";
        const string scriptSource = "write(2¹ᱸ²);\r\nwrite(2ˣ˸²);";
        const string customSource = "static real glyphs(real x) {\r\n    return 2¹ᱸ² + 2ˣ˸²;\r\n}";
        var bootstrapper = new WinFormsBootstrapper();
        using (bootstrapper.Container)
        using (var window = new Form { ClientSize = new Size(1200, 700), StartPosition = FormStartPosition.Manual, Location = new Point(5, 5) })
        using (var expression = bootstrapper.Create<ExpressionTextBox>())
        using (var script = bootstrapper.Create<TextEditorCodeEditor>())
        using (var custom = bootstrapper.Create<TextEditorCodeEditor>())
        using (var math = CustomFonts.GetMathFont(24))
        using (var code = CustomFonts.GetScriptingFont(24))
        {
            expression.SetFont(math);
            expression.Text = expressionSource;
            expression.Multiline = true;
            expression.Dock = DockStyle.Top;
            expression.Height = 85;
            script.SetFont(code);
            custom.SetFont(code);
            script.NewDocument("glyphs.tsl", scriptSource);
            custom.NewDocument("glyphs.tslf", customSource);
            foreach (var editor in new[] { script, custom }) { editor.Dock = DockStyle.Top; editor.Height = 250; }
            window.Controls.Add(expression);
            window.Controls.Add(script);
            window.Controls.Add(custom);
            window.Show();
            Application.DoEvents();
            Capture(window, "mono-font-editors.png");
            AssertMathematicalGlyphShapes(expression.Font);

            foreach (var editor in new[] { script, custom })
            {
                // Inspect the actual drawing fonts used by the legacy editor,
                // not only Control.Font (which is not its document font cache).
                AssertMathematicalGlyphShapes(editor.TextEditorProperties.FontContainer.RegularFont);
                AssertMathematicalGlyphShapes(editor.TextEditorProperties.FontContainer.BoldFont);
                AssertMathematicalGlyphShapes(editor.TextEditorProperties.FontContainer.ItalicFont);
                AssertMathematicalGlyphShapes(editor.TextEditorProperties.FontContainer.BoldItalicFont);
                var first = editor == script ? "glyphs.tsl" : "glyphs.tslf";
                var original = editor.Text;
                editor.NewDocument("second.tsl", "2¹ᱸ²");
                AssertMathematicalGlyphShapes(editor.TextEditorProperties.FontContainer.RegularFont);
                editor.SwitchDocument(first);
                Equal(editor.Text, original);

                var changed = new FontDocument { Name = "Consolas", Size = 22.5f, Style = FontStyle.Bold | FontStyle.Italic, Unit = GraphicsUnit.Pixel };
                using (var restored = changed.ToFont())
                {
                    editor.SetFont(restored);
                    foreach (var document in new[] { "second.tsl", first })
                    {
                        editor.SwitchDocument(document);
                        var actual = editor.TextEditorProperties.Font;
                        Check(actual.Style == changed.Style && actual.Unit == changed.Unit, "A document switch lost the configured style/unit.");
                        Near(actual.Size, changed.Size, 0);
                        AssertMathematicalGlyphShapes(editor.TextEditorProperties.FontContainer.RegularFont);
                    }
                }
                Equal(editor.Text, original);
                using (var alternative = new Font(FontFamily.GenericMonospace, 18, FontStyle.Italic, GraphicsUnit.Pixel))
                {
                    editor.SetFont(alternative);
                    editor.NewDocument("custom-font.tsl", "unchanged custom font document");
                    foreach (var document in new[] { "second.tsl", first, "custom-font.tsl", first })
                    {
                        editor.SwitchDocument(document);
                        var actual = editor.TextEditorProperties.Font;
                        Equal(actual.Name, alternative.Name);
                        Check(actual.Style == alternative.Style && actual.Unit == alternative.Unit, "A custom font changed when switching documents.");
                        Near(actual.Size, alternative.Size, 0);
                    }
                }
                Equal(editor.Text, original);
            }
            Equal(expression.Text, expressionSource);
            Equal(script.Text, scriptSource);
            Equal(custom.Text, customSource);
            window.Close();
        }
    }

    private static void AssertMathematicalGlyphShapes(Font font)
    {
        foreach (var textRenderer in new[] { false, true })
        {
            var zero = GlyphInkBounds("0", font, textRenderer);
            var decimalMark = GlyphInkBounds(SpecialSymbols.DecimalSeparatorSuperscript, font, textRenderer);
            var division = GlyphInkBounds("˸", font, textRenderer);
            // The existing decimal is a raised dot, and division is a narrow
            // raised colon. A hex-filled missing-glyph box violates these
            // shape bounds even when MeasureString reports a positive width.
            Check(decimalMark.Width > 0 && decimalMark.Height > 0 && decimalMark.Height * 2 < zero.Height && decimalMark.Width < zero.Width,
                "Superscript decimal rendered as a missing/incorrect glyph: " + font.Name + " " + decimalMark + " zero=" + zero);
            Check(division.Width > 0 && division.Width * 2 < zero.Width && division.Height < zero.Height,
                "Superscript division rendered as a missing/incorrect glyph: " + font.Name + " " + division + " zero=" + zero);
        }
    }

    private static Rectangle GlyphInkBounds(string text, Font font, bool textRenderer)
    {
        using (var image = new Bitmap(160, 160))
        {
            using (var graphics = Graphics.FromImage(image))
            {
                graphics.Clear(Color.White);
                if (textRenderer) TextRenderer.DrawText(graphics, text, font, new Point(15, 15), Color.Black);
                else graphics.DrawString(text, font, Brushes.Black, 15, 15);
            }
            var left = image.Width;
            var top = image.Height;
            var right = -1;
            var bottom = -1;
            for (var x = 0; x < image.Width; x++)
            for (var y = 0; y < image.Height; y++)
            {
                var pixel = image.GetPixel(x, y);
                if (pixel.R > 192 || pixel.G > 192 || pixel.B > 192) continue;
                left = Math.Min(left, x); top = Math.Min(top, y);
                right = Math.Max(right, x); bottom = Math.Max(bottom, y);
            }
            return right < left ? Rectangle.Empty : Rectangle.FromLTRB(left, top, right + 1, bottom + 1);
        }
    }
}
