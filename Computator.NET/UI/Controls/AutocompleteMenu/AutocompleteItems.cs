using System;
using System.Drawing;
using Computator.NET.Data;

namespace Computator.NET.UI.Controls.AutocompleteMenu
{
    public class FuzzyAutoCompleteItem : AutocompleteItem
    {
        private IFunctionsDetails _functionsDetails;
        public FuzzyAutoCompleteItem(string text, IFunctionsDetails functionsDetails) : base(text,functionsDetails)
        {
            _functionsDetails = functionsDetails;
        }

        public FuzzyAutoCompleteItem(string name, string addition, string additionWithTypes, string returnTypeName,
            int imageIndex, IFunctionsDetails functionsDetails) : base(name, addition, additionWithTypes, returnTypeName, imageIndex, functionsDetails)
        {
            _functionsDetails = functionsDetails;
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
    }


    /// <summary>
    ///     This autocomplete item appears after dot
    /// </summary>
    public class MethodAutocompleteItem : AutocompleteItem
    {
        private readonly string lowercaseText;
        private string firstPart;

        public MethodAutocompleteItem(string text, IFunctionsDetails functionsDetails)
            : base(text,functionsDetails)
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
        public SnippetAutocompleteItem(string snippet, IFunctionsDetails functionsDetails) : base(functionsDetails)
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

        public SubstringAutocompleteItem(string text, IFunctionsDetails functionsDetails, bool ignoreCase = true)
            : base(text,functionsDetails)
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
        public MulticolumnAutocompleteItem(string[] menuTextByColumns, string insertingText, IFunctionsDetails functionsDetails, bool compareBySubstring = true, bool ignoreCase = true)
            : base(insertingText, functionsDetails, ignoreCase)
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