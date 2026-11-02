using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Linq;
using Computator.NET.Core.Autocompletion;
using Computator.NET.DataTypes.Utility;

namespace Computator.NET.Desktop.Controls.AutocompleteMenu
{
    // A deliberately small reader for the checked-in reference vocabulary, not
    // a browser. Markup never creates controls, executes code or opens a URI.
    internal sealed class MonoReferenceDocument : IDisposable
    {
        internal sealed class Run
        {
            internal string Text;
            internal FontStyle Style;
            internal bool Code, Inline, Plot;
            internal float Scale = 1;
            internal float Shift;
            internal bool PlainOnly;
            internal Image Image;
            internal int Width, Height, Depth;
        }
        internal sealed class Block
        {
            internal readonly List<Run> Runs = new List<Run>();
            internal string Kind;
            internal bool PreserveWhitespace;
        }
        private readonly List<Block> blocks = new List<Block>();
        private readonly Dictionary<string, Image> images = new Dictionary<string, Image>(StringComparer.Ordinal);
        internal string PlainText { get; private set; }
        internal IReadOnlyCollection<string> ImageSources => images.Keys;
        internal IReadOnlyList<Block> Blocks => blocks;

        internal static MonoReferenceDocument Parse(FunctionInfo info, string staticDirectory = null)
        {
            if (info == null) throw new ArgumentNullException(nameof(info));
            var document = new MonoReferenceDocument();
            try
            {
                var settings = new XmlReaderSettings { DtdProcessing = DtdProcessing.Prohibit, XmlResolver = null, MaxCharactersInDocument = 262144 };
                XElement root;
                using (var input = new StringReader("<root>" + info.Description + "</root>"))
                using (var reader = XmlReader.Create(input, settings)) root = XElement.Load(reader, LoadOptions.PreserveWhitespace);
                if (root.Descendants().Take(8193).Count() > 8192 || root.Descendants().Any(e => e.Ancestors().Take(33).Count() > 32))
                    throw new InvalidDataException("The reference is too deeply nested or too large.");
                var title = new Block { Kind = "title" };
                title.Runs.Add(new Run { Text = info.Title ?? "", Style = FontStyle.Bold, Scale = 1.4f });
                document.blocks.Add(title);
                var catalog = new MonoReferenceAssets(staticDirectory ?? PathUtility.GetFullPath("Static"));
                document.ReadNodes(root, null, FontStyle.Regular, false, catalog);
                document.PlainText = string.Join(Environment.NewLine + Environment.NewLine, document.blocks.Select(b =>
                    string.Concat(b.Runs.Select(r => r.Text ?? "")))).Trim();
                return document;
            }
            catch { document.Dispose(); throw; }
        }

        private void ReadNodes(XElement parent, Block block, FontStyle style, bool code, MonoReferenceAssets assets, string decoration = null)
        {
            foreach (var node in parent.Nodes())
            {
                if (node is XText text)
                {
                    if (block == null) { if (string.IsNullOrWhiteSpace(text.Value)) continue; block = AddBlock(""); }
                    block.Runs.Add(new Run { Text = block.PreserveWhitespace ? text.Value : Regex.Replace(text.Value, @"\s+", " "), Style = style, Code = code });
                    continue;
                }
                if (!(node is XElement element)) continue;
                var tag = element.Name.LocalName;
                var classes = ((string)element.Attribute("class") ?? "").Split((char[])null, StringSplitOptions.RemoveEmptyEntries);
                if (classes.Contains("reference-signature") || new[] { "script", "style", "iframe", "object", "embed", "link", "base", "svg" }.Contains(tag)) continue;
                if (tag == "div" || tag == "section")
                {
                    var panel = classes.FirstOrDefault(c => c == "reference-worked-example" || c == "reference-note" || c == "reference-formula");
                    ReadNodes(element, null, style, code, assets, panel ?? decoration);
                    block = null;
                    continue;
                }
                if (tag == "p" || tag == "pre" || tag == "li" || Regex.IsMatch(tag, @"\Ah[1-6]\z"))
                {
                    var child = AddBlock(decoration ?? classes.FirstOrDefault(c => c != "function-reference") ?? tag);
                    child.PreserveWhitespace = tag == "pre";
                    if (tag == "li") child.Runs.Add(new Run { Text = "• " });
                    ReadNodes(element, child, style, code || tag == "pre", assets);
                    block = null;
                    continue;
                }
                if (block == null) block = AddBlock("");
                if (tag == "br") { block.Runs.Add(new Run { Text = "\n" }); continue; }
                if (tag == "sup" || tag == "sub")
                {
                    block.Runs.Add(new Run { Text = tag == "sup" ? "^(" : "_(", PlainOnly = true });
                    var start = block.Runs.Count;
                    ReadNodes(element, block, style, code, assets);
                    foreach (var run in block.Runs.Skip(start)) { run.Scale *= .7f; run.Shift += tag == "sup" ? .4f : -.2f; }
                    block.Runs.Add(new Run { Text = ")", PlainOnly = true });
                    continue;
                }
                if (tag == "img")
                {
                    var run = assets.Read(element, images);
                    block.Runs.Add(run);
                    continue;
                }
                // Unknown inline markup is inert text. Attributes, CSS, events,
                // hrefs and arbitrary image protocols are never interpreted.
                var nextStyle = style | (tag == "strong" || tag == "b" ? FontStyle.Bold : FontStyle.Regular)
                    | (tag == "em" || tag == "i" ? FontStyle.Italic : FontStyle.Regular)
                    | (tag == "a" ? FontStyle.Underline : FontStyle.Regular);
                ReadNodes(element, block, nextStyle, code || tag == "code", assets);
                if (tag == "a" && !string.IsNullOrEmpty((string)element.Attribute("href")))
                    block.Runs.Add(new Run { Text = " (" + (string)element.Attribute("href") + ")", Style = FontStyle.Underline });
            }
        }
        private Block AddBlock(string kind)
        {
            if (blocks.Count >= 4096) throw new InvalidDataException("The reference has too many paragraphs.");
            var block = new Block { Kind = kind }; blocks.Add(block); return block;
        }
        internal MonoReferenceLayout Layout(Graphics graphics, Font font, int width) => new MonoReferenceLayout(this, graphics, font, width);
        public void Dispose() { foreach (var image in images.Values) image.Dispose(); images.Clear(); }
    }

    internal sealed class MonoReferenceAssets
    {
        private readonly string directory;
        private readonly Dictionary<string, Asset> entries = new Dictionary<string, Asset>(StringComparer.Ordinal);
        private sealed class Asset { internal string Hash; internal int Width, Height, Depth; internal bool Inline, Plot; }
        internal MonoReferenceAssets(string directory)
        {
            this.directory = Path.GetFullPath(directory);
            ReadManifest("reference-equations", "equations", false);
            ReadManifest("reference-figures", "figures", true);
        }
        private void ReadManifest(string folder, string collection, bool plot)
        {
            var path = Path.Combine(directory, folder, "manifest.json");
            RejectLink(directory); RejectLink(Path.GetDirectoryName(path)); RejectLink(path);
            var bytes = ReadBounded(path, 2 * 1024 * 1024);
            using (var manifest = JsonDocument.Parse(bytes, new JsonDocumentOptions { MaxDepth = 12 }))
                foreach (var item in manifest.RootElement.GetProperty(collection).EnumerateObject())
                {
                    if (!Regex.IsMatch(item.Name, @"\A[a-f0-9]{64}\z", RegexOptions.CultureInvariant)) throw new InvalidDataException("Invalid reference asset key.");
                    var value = item.Value;
                    var entry = new Asset { Hash = value.GetProperty("sha256").GetString(), Width = value.GetProperty("width").GetInt32(), Height = value.GetProperty("height").GetInt32(), Plot = plot };
                    if (!Regex.IsMatch(entry.Hash ?? "", @"\A[a-f0-9]{64}\z") || entry.Width <= 0 || entry.Width > 2400 || entry.Height <= 0 || entry.Height > 800) throw new InvalidDataException("Invalid reference asset metadata.");
                    if (value.TryGetProperty("mode", out var mode)) entry.Inline = mode.GetString() == "inline";
                    if (value.TryGetProperty("depth", out var depth)) entry.Depth = depth.GetInt32();
                    entries.Add(folder + "/" + item.Name + ".png", entry);
                }
        }
        internal MonoReferenceDocument.Run Read(XElement element, Dictionary<string, Image> images)
        {
            var source = (string)element.Attribute("src") ?? "";
            var kind = (string)element.Attribute("class") ?? "";
            if (!Regex.IsMatch(source, @"\Areference-(?:equations|figures)/[a-f0-9]{64}\.png\z", RegexOptions.CultureInvariant) || !entries.TryGetValue(source, out var entry))
                throw new InvalidDataException("The reference image is not a known offline asset.");
            if (kind != (entry.Plot ? "reference-plot" : entry.Inline ? "reference-math-inline" : "reference-math") ||
                (string)element.Attribute("width") != entry.Width.ToString(CultureInfo.InvariantCulture) ||
                (string)element.Attribute("height") != entry.Height.ToString(CultureInfo.InvariantCulture) ||
                string.IsNullOrWhiteSpace((string)element.Attribute("alt"))) throw new InvalidDataException("Reference image metadata differs from its catalog.");
            var depth = (string)element.Attribute("data-depth");
            if (entry.Inline ? depth != entry.Depth.ToString(CultureInfo.InvariantCulture) : depth != null)
                throw new InvalidDataException("Invalid inline reference baseline.");
            if (entry.Depth < -1000 || entry.Depth >= entry.Height * 50) throw new InvalidDataException("The inline reference baseline is out of range.");
            if (!images.TryGetValue(source, out var image))
            {
                if (images.Count >= 64) throw new InvalidDataException("The reference has too many images.");
                var path = Path.Combine(directory, source.Replace('/', Path.DirectorySeparatorChar)); RejectLink(path);
                var bytes = ReadBounded(path, 512000);
                using (var hash = SHA256.Create())
                    if (BitConverter.ToString(hash.ComputeHash(bytes)).Replace("-", "").ToLowerInvariant() != entry.Hash)
                        throw new InvalidDataException("The reference image failed its catalog hash.");
                var signature = new byte[] { 137, 80, 78, 71, 13, 10, 26, 10 };
                if (bytes.Length < 24 || !bytes.Take(8).SequenceEqual(signature)) throw new InvalidDataException("The reference image is not a PNG.");
                Func<int, uint> integer = offset => ((uint)bytes[offset] << 24) | ((uint)bytes[offset + 1] << 16) | ((uint)bytes[offset + 2] << 8) | bytes[offset + 3];
                var width = integer(16); var height = integer(20);
                if (width == 0 || height == 0 || width > 9600 || height > 3200 || (ulong)width * height > 20000000) throw new InvalidDataException("The reference PNG dimensions are out of range.");
                using (var stream = new MemoryStream(bytes, false))
                using (var decoded = Image.FromStream(stream, false, true))
                {
                    if (decoded.Width != width || decoded.Height != height) throw new InvalidDataException("The reference PNG has inconsistent dimensions.");
                    image = new Bitmap(decoded);
                }
                images.Add(source, image);
            }
            return new MonoReferenceDocument.Run { Text = (string)element.Attribute("alt"), Image = image, Width = entry.Width, Height = entry.Height, Depth = entry.Depth, Inline = entry.Inline, Plot = entry.Plot };
        }
        private static void RejectLink(string path)
        {
            if ((File.GetAttributes(path) & FileAttributes.ReparsePoint) != 0) throw new InvalidDataException("Reference assets cannot be symbolic links.");
        }
        private static byte[] ReadBounded(string path, int limit)
        {
            using (var file = File.OpenRead(path))
            {
                if (file.Length <= 0 || file.Length > limit) throw new InvalidDataException("Reference asset is too large.");
                var bytes = new byte[(int)file.Length]; var offset = 0;
                while (offset < bytes.Length) { var read = file.Read(bytes, offset, bytes.Length - offset); if (read == 0) throw new EndOfStreamException(); offset += read; }
                return bytes;
            }
        }
    }

    internal sealed class MonoReferenceLayout : IDisposable
    {
        internal sealed class Item
        {
            internal RectangleF Bounds;
            internal string Text;
            internal Font Font;
            internal Image Image;
            internal Color Background;
            internal bool Inline;
            internal float Ascent, Descent;
        }
        private readonly List<Item> items = new List<Item>();
        private readonly List<Font> fonts = new List<Font>();
        private readonly Dictionary<string, Font> fontCache = new Dictionary<string, Font>();
        private readonly StringFormat format;
        internal IReadOnlyList<Item> Items => items;
        internal Size ContentSize { get; private set; }
        internal MonoReferenceLayout(MonoReferenceDocument document, Graphics graphics, Font baseFont, int width)
        {
            if (graphics == null || baseFont == null) throw new ArgumentNullException();
            if (width < 64 || width > 16384) throw new ArgumentOutOfRangeException(nameof(width));
            format = (StringFormat)StringFormat.GenericTypographic.Clone();
            format.FormatFlags |= StringFormatFlags.MeasureTrailingSpaces | StringFormatFlags.NoWrap;
            var em = baseFont.SizeInPoints * graphics.DpiY / 72f;
            var padding = Math.Min(em, Math.Max(4, width / 10f)); var y = padding; var maximumRight = (float)width;
            try
            {
                foreach (var block in document.Blocks)
                {
                    var decorated = block.Kind == "reference-note" || block.Kind == "reference-formula" || block.Kind == "reference-equation" || block.Kind == "reference-worked-example";
                    var inset = decorated ? Math.Min(em * .7f, Math.Max(0, (width - padding * 2 - 1) / 4)) : 0;
                    var left = padding + inset; var right = width - padding - inset;
                    var top = y; var startIndex = items.Count; if (decorated) y += em * .6f;
                    var x = left; var line = new List<Item>();
                    Action flush = () =>
                    {
                        if (line.Count == 0) { y += baseFont.GetHeight(graphics) * 1.4f; x = left; return; }
                        var ascent = line.Max(i => i.Ascent); var descent = line.Max(i => i.Descent);
                        foreach (var item in line) { item.Bounds = new RectangleF(item.Bounds.X, y + ascent - item.Ascent, item.Bounds.Width, item.Bounds.Height); items.Add(item); maximumRight = Math.Max(maximumRight, item.Bounds.Right + padding); }
                        y += Math.Max(baseFont.GetHeight(graphics) * 1.4f, ascent + descent + em * .2f); x = left; line.Clear();
                    };
                    Action<Item> append = item =>
                    {
                        if (x > left && x + item.Bounds.Width > right) flush();
                        item.Bounds = new RectangleF(x, 0, item.Bounds.Width, item.Bounds.Height); x += item.Bounds.Width; line.Add(item);
                    };
                    foreach (var run in block.Runs)
                    {
                        if (run.PlainOnly) continue;
                        if (run.Image != null)
                        {
                            var scale = em / 20f;
                            if (run.Plot) scale = Math.Min(scale, Math.Max(1, right - left) / run.Width);
                            var image = new Item { Image = run.Image, Inline = run.Inline, Bounds = new RectangleF(0, 0, run.Width * scale, run.Height * scale) };
                            var depth = run.Depth * em / 1000f;
                            image.Ascent = run.Inline ? image.Bounds.Height - depth : image.Bounds.Height; image.Descent = run.Inline ? Math.Max(0, depth) : 0;
                            if (!run.Inline && line.Count > 0) flush(); append(image); if (!run.Inline) flush();
                            continue;
                        }
                        var font = GetFont(baseFont, run); var size = font.SizeInPoints * graphics.DpiY / 72f;
                        var ascent = font.FontFamily.GetCellAscent(font.Style) * size / font.FontFamily.GetEmHeight(font.Style);
                        var descent = font.FontFamily.GetCellDescent(font.Style) * size / font.FontFamily.GetEmHeight(font.Style);
                        foreach (Match token in Regex.Matches(run.Text ?? "", @"\n|[^\S\n]+|[^\s]+"))
                        {
                            if (token.Value == "\n") { flush(); continue; }
                            var value = token.Value;
                            if (string.IsNullOrWhiteSpace(value)) { if (x == left && !block.PreserveWhitespace) continue; value = block.PreserveWhitespace ? value.Replace("\t", "    ") : " "; }
                            var measured = graphics.MeasureString(value, font, PointF.Empty, format);
                            if (measured.Width <= right - left)
                                append(new Item { Text = value, Font = font, Bounds = new RectangleF(0, 0, measured.Width, font.GetHeight(graphics)), Ascent = ascent + run.Shift * em, Descent = Math.Max(0, descent - run.Shift * em) });
                            else
                            {
                                // Break long literal code/URLs at grapheme boundaries,
                                // retaining surrogate pairs and combining marks.
                                var iterator = StringInfo.GetTextElementEnumerator(value);
                                while (iterator.MoveNext())
                                {
                                    var character = iterator.GetTextElement(); var cell = graphics.MeasureString(character, font, PointF.Empty, format);
                                    append(new Item { Text = character, Font = font, Bounds = new RectangleF(0, 0, cell.Width, font.GetHeight(graphics)), Ascent = ascent + run.Shift * em, Descent = Math.Max(0, descent - run.Shift * em) });
                                }
                            }
                        }
                    }
                    if (line.Count > 0) flush();
                    if (decorated) { y += em * .6f; items.Insert(startIndex, new Item { Bounds = new RectangleF(padding, top, Math.Max(1, Math.Max(width - padding * 2, maximumRight - padding * 2)), y - top), Background = block.Kind == "reference-note" ? Color.FromArgb(255, 249, 235) : Color.FromArgb(244, 246, 248) }); }
                    y += em * .6f;
                    if (y > 200000 || items.Count > 100000) throw new InvalidDataException("Reference layout is too large.");
                }
                ContentSize = new Size((int)Math.Ceiling(maximumRight), (int)Math.Ceiling(y + padding));
            }
            catch { Dispose(); throw; }
        }
        private Font GetFont(Font source, MonoReferenceDocument.Run run)
        {
            var key = run.Style + ":" + run.Scale.ToString(CultureInfo.InvariantCulture) + ":" + run.Code;
            if (!fontCache.TryGetValue(key, out var font))
            {
                font = new Font(run.Code ? FontFamily.GenericMonospace : source.FontFamily, source.SizeInPoints * run.Scale, run.Style, GraphicsUnit.Point);
                fonts.Add(font); fontCache.Add(key, font);
            }
            return font;
        }
        internal void Draw(Graphics graphics, Point offset)
        {
            var state = graphics.Save();
            try
            {
                graphics.TranslateTransform(offset.X, offset.Y);
                graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;
                foreach (var item in items)
                {
                    if (item.Background.A != 0) { using (var brush = new SolidBrush(item.Background)) graphics.FillRectangle(brush, item.Bounds); using (var pen = new Pen(Color.FromArgb(117, 135, 155), 2)) graphics.DrawLine(pen, item.Bounds.Left, item.Bounds.Top, item.Bounds.Left, item.Bounds.Bottom); }
                    else if (item.Image != null) graphics.DrawImage(item.Image, item.Bounds);
                    else if (item.Text != null) graphics.DrawString(item.Text, item.Font, Brushes.Black, item.Bounds.Location, format);
                }
            }
            finally { graphics.Restore(state); }
        }
        public void Dispose() { foreach (var font in fonts) font.Dispose(); fonts.Clear(); fontCache.Clear(); format?.Dispose(); }
    }
}
