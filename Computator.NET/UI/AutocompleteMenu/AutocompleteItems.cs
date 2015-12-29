using System;
using System.Drawing;

namespace AutocompleteMenuNS
{
    public class FuzzyAutoCompleteItem : AutocompleteItem
    {
        public FuzzyAutoCompleteItem(string text) : base(text)
        {
        }

        public FuzzyAutoCompleteItem(string name, string addition, string additionWithTypes, string returnTypeName,
            int imageIndex) : base(name,addition,additionWithTypes,returnTypeName,imageIndex)
        {
        }

        public override CompareResult Compare(string fragmentText)
        {
            var lev = Levenshtein(Text, fragmentText);
            if (lev > 0.9)
                return CompareResult.VisibleAndSelected;
            if (lev > 0.5)
                return CompareResult.Visible;
            return CompareResult.Hidden;
        }

        public static float Levenshtein(string src, string dest)
        {
            int[,] d = new int[src.Length + 1, dest.Length + 1];
            int i, j, cost;
            char[] str1 = src.ToCharArray();
            char[] str2 = dest.ToCharArray();

            for (i = 0; i <= str1.Length; i++)
            {
                d[i, 0] = i;
            }
            for (j = 0; j <= str2.Length; j++)
            {
                d[0, j] = j;
            }
            for (i = 1; i <= str1.Length; i++)
            {
                for (j = 1; j <= str2.Length; j++)
                {

                    if (str1[i - 1] == str2[j - 1])
                        cost = 0;
                    else
                        cost = 1;

                    d[i, j] =
                        Math.Min(
                            d[i - 1, j] + 1,              // Deletion
                            Math.Min(
                                d[i, j - 1] + 1,          // Insertion
                                d[i - 1, j - 1] + cost)); // Substitution

                    if ((i > 1) && (j > 1) && (str1[i - 1] ==
                        str2[j - 2]) && (str1[i - 2] == str2[j - 1]))
                    {
                        d[i, j] = Math.Min(d[i, j], d[i - 2, j - 2] + cost);
                    }
                }
            }

            var dist = (float)d[str1.Length, str2.Length];

            return 1 - dist / Math.Max(str1.Length, str2.Length);
        }
    }


    /// <summary>
    ///     This autocomplete item appears after dot
    /// </summary>
    public class MethodAutocompleteItem : AutocompleteItem
    {
        private readonly string lowercaseText;
        private string firstPart;

        public MethodAutocompleteItem(string text)
            : base(text)
        {
            lowercaseText = Text.ToLower();
        }

        public override CompareResult Compare(string fragmentText)
        {
            var i = fragmentText.LastIndexOf('.');
            if (i < 0)
                return CompareResult.Hidden;
            var lastPart = fragmentText.Substring(i + 1);
            firstPart = fragmentText.Substring(0, i);

            if (lastPart == "") return CompareResult.Visible;
            if (Text.StartsWith(lastPart, StringComparison.InvariantCultureIgnoreCase))
                return CompareResult.VisibleAndSelected;
            if (lowercaseText.Contains(lastPart.ToLower()))
                return CompareResult.Visible;

            return CompareResult.Hidden;
        }

        public override string GetTextForReplace()
        {
            return firstPart + "." + Text;
        }
    }

    /// <summary>
    ///     Autocomplete item for code snippets
    /// </summary>
    /// <remarks>Snippet can contain special char ^ for caret position.</remarks>
    public class SnippetAutocompleteItem : AutocompleteItem
    {
        public SnippetAutocompleteItem(string snippet)
        {
            Text = snippet.Replace("\r", "");
            ToolTipTitle = "Code snippet:";
            ToolTipText = Text;
        }

        public override string ToString()
        {
            return MenuText ?? Text.Replace("\n", " ").Replace("^", "");
        }

        public override string GetTextForReplace()
        {
            return Text;
        }

        public override void OnSelected(SelectedEventArgs e)
        {
            var tb = Parent.TargetControlWrapper;
            //
            if (!Text.Contains("^"))
                return;
            var text = tb.Text;
            for (var i = Parent.Fragment.Start; i < text.Length; i++)
                if (text[i] == '^')
                {
                    tb.SelectionStart = i;
                    tb.SelectionLength = 1;
                    tb.SelectedText = "";
                    return;
                }
        }

        /// <summary>
        ///     Compares fragment text with this item
        /// </summary>
        public override CompareResult Compare(string fragmentText)
        {
            if (Text.StartsWith(fragmentText, StringComparison.InvariantCultureIgnoreCase) &&
                Text != fragmentText)
                return CompareResult.Visible;

            return CompareResult.Hidden;
        }
    }

    /// <summary>
    ///     This class finds items by substring
    /// </summary>
    public class SubstringAutocompleteItem : AutocompleteItem
    {
        protected readonly bool ignoreCase;
        protected readonly string lowercaseText;

        public SubstringAutocompleteItem(string text, bool ignoreCase = true)
            : base(text)
        {
            this.ignoreCase = ignoreCase;
            if (ignoreCase)
                lowercaseText = text.ToLower();
        }

        public override CompareResult Compare(string fragmentText)
        {
            if (ignoreCase)
            {
                if (lowercaseText.Contains(fragmentText.ToLower()))
                    return CompareResult.Visible;
            }
            else
            {
                if (Text.Contains(fragmentText))
                    return CompareResult.Visible;
            }

            return CompareResult.Hidden;
        }
    }

    /// <summary>
    ///     This item draws multicolumn menu
    /// </summary>
    public class MulticolumnAutocompleteItem : SubstringAutocompleteItem
    {
        public MulticolumnAutocompleteItem(string[] menuTextByColumns, string insertingText,
            bool compareBySubstring = true, bool ignoreCase = true)
            : base(insertingText, ignoreCase)
        {
            CompareBySubstring = compareBySubstring;
            MenuTextByColumns = menuTextByColumns;
        }

        public bool CompareBySubstring { get; set; }
        public string[] MenuTextByColumns { get; set; }
        public int[] ColumnWidth { get; set; }

        public override CompareResult Compare(string fragmentText)
        {
            if (CompareBySubstring)
                return base.Compare(fragmentText);

            if (ignoreCase)
            {
                if (Text.StartsWith(fragmentText, StringComparison.InvariantCultureIgnoreCase))
                    return CompareResult.VisibleAndSelected;
            }
            else if (Text.StartsWith(fragmentText))
                return CompareResult.VisibleAndSelected;

            return CompareResult.Hidden;
        }

        public override void OnPaint(PaintItemEventArgs e)
        {
            if (ColumnWidth != null && ColumnWidth.Length != MenuTextByColumns.Length)
                throw new Exception("ColumnWidth.Length != MenuTextByColumns.Length");

            var columnWidth = ColumnWidth;
            if (columnWidth == null)
            {
                columnWidth = new int[MenuTextByColumns.Length];
                var step = e.TextRect.Width/MenuTextByColumns.Length;
                for (var i = 0; i < MenuTextByColumns.Length; i++)
                    columnWidth[i] = (int) step;
            }

            //draw columns
            var pen = Pens.Silver;
            //var brush = Brushes.Black;
            var x = e.TextRect.X;
            e.StringFormat.FormatFlags = e.StringFormat.FormatFlags | StringFormatFlags.NoWrap;

            using (var brush = new SolidBrush(e.IsSelected ? e.Colors.SelectedForeColor : e.Colors.ForeColor))
                for (var i = 0; i < MenuTextByColumns.Length; i++)
            {
                var width = columnWidth[i];
                var rect = new RectangleF(x, e.TextRect.Top, width, e.TextRect.Height);
                e.Graphics.DrawLine(pen, new PointF(x, e.TextRect.Top),
                    new PointF(x, e.TextRect.Bottom));
                e.Graphics.DrawString(MenuTextByColumns[i], e.Font, brush, rect, e.StringFormat);
                x += width;
            }
        }
    }
}