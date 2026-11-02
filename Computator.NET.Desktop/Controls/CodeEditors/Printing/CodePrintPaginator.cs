#nullable enable
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;

namespace Computator.NET.Desktop.Controls.CodeEditors.Printing
{
    // The caller measures with the same graphics and font used to draw each
    // page. Pagination does not depend on an editor or a printer being present.
    internal sealed class CodePrintPaginator
    {
        private readonly List<string> _lines = new List<string>();
        private int _lineIndex;
        private int _elementIndex;
        private int[]? _elements;

        internal CodePrintPaginator(string? text)
        {
            using (var reader = new StringReader(text ?? string.Empty))
            {
                string? line;
                while ((line = reader.ReadLine()) != null)
                    _lines.Add(ExpandTabs(line));
            }
        }

        internal bool HasMoreLines => _lineIndex < _lines.Count;

        internal IReadOnlyList<string> ReadPage(double width, int rows, Func<string, double> measure)
        {
            if (!(width > 0) || double.IsInfinity(width))
                throw new ArgumentOutOfRangeException(nameof(width));
            if (rows < 1) throw new ArgumentOutOfRangeException(nameof(rows));
            if (measure == null) throw new ArgumentNullException(nameof(measure));

            var page = new List<string>();
            while (page.Count < rows && HasMoreLines)
            {
                var line = _lines[_lineIndex];
                if (line.Length == 0)
                {
                    page.Add(string.Empty);
                    NextLine();
                    continue;
                }

                if (_elements == null) _elements = StringInfo.ParseCombiningCharacters(line);
                var start = _elements[_elementIndex];
                var remaining = _elements.Length - _elementIndex;
                var count = 0;
                var high = 1;
                // First bracket the width exponentially. Measuring half of a
                // million-character line on every row would be quadratic even
                // though only a few dozen characters fit on each printed row.
                while (Fits(high))
                {
                    count = high;
                    if (high == remaining) break;
                    high = high <= remaining / 2 ? high * 2 : remaining;
                }
                var low = count + 1;
                if (count != remaining) high--;
                // Search only within the bracket and only at text-element
                // boundaries, preserving surrogate pairs and combining marks.
                while (low <= high)
                {
                    var candidate = low + (high - low) / 2;
                    if (Fits(candidate))
                    {
                        count = candidate;
                        low = candidate + 1;
                    }
                    else high = candidate - 1;
                }

                if (count == 0)
                    throw new CodePrintLayoutException("The printable page is too narrow for the document font. Increase the paper size or reduce the margins.");
                var next = _elementIndex + count;
                var length = (next == _elements.Length ? line.Length : _elements[next]) - start;
                page.Add(line.Substring(start, length));
                _elementIndex = next;
                if (_elementIndex == _elements.Length) NextLine();

                bool Fits(int length)
                {
                    var end = _elementIndex + length == _elements.Length
                        ? line.Length : _elements[_elementIndex + length];
                    var measured = measure(line.Substring(start, end - start));
                    if (double.IsNaN(measured) || double.IsInfinity(measured) || measured < 0)
                        throw new CodePrintLayoutException("The printer returned an invalid text measurement.");
                    return measured <= width;
                }
            }
            return page;
        }

        private void NextLine()
        {
            _lineIndex++;
            _elementIndex = 0;
            _elements = null;
        }

        private static string ExpandTabs(string line)
        {
            var expanded = new StringBuilder();
            var elements = StringInfo.GetTextElementEnumerator(line);
            var column = 0;
            while (elements.MoveNext())
            {
                var element = elements.GetTextElement();
                if (element == "\t")
                {
                    var spaces = 4 - column % 4;
                    expanded.Append(' ', spaces);
                    column += spaces;
                }
                else
                {
                    expanded.Append(element);
                    column++;
                }
            }
            return expanded.ToString();
        }
    }
}
