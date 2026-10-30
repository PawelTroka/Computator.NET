using System;
using System.Collections.Generic;
using System.Globalization;

namespace Computator.NET.Desktop.Controls.CodeEditors
{
    // Literal searches use document (UTF-16) offsets, never byte offsets or regex replacement syntax.
    internal static class TextSearch
    {
        internal static List<int> FindAll(string text, string query, bool matchCase, bool wholeWord)
        {
            var matches = new List<int>();
            if (string.IsNullOrEmpty(query)) return matches;
            var comparison = matchCase ? StringComparison.Ordinal : StringComparison.OrdinalIgnoreCase;
            var start = 0;
            while (start <= text.Length - query.Length)
            {
                var index = text.IndexOf(query, start, comparison);
                if (index < 0) break;
                if (!wholeWord || IsWholeWord(text, index, query.Length))
                {
                    matches.Add(index);
                    start = index + query.Length;
                }
                else start = index + 1;
            }
            return matches;
        }

        internal static int Find(string text, string query, int start, bool backwards,
            bool matchCase, bool wholeWord)
        {
            if (string.IsNullOrEmpty(query) || query.Length > text.Length) return -1;
            var comparison = matchCase ? StringComparison.Ordinal : StringComparison.OrdinalIgnoreCase;
            // Two bounded passes implement wrapping without revisiting a failed candidate.
            start = Math.Max(-1, Math.Min(start, text.Length));
            if (backwards)
            {
                var index = FindBackwards(text, query, Math.Min(start, text.Length - query.Length), 0,
                    comparison, wholeWord);
                return index >= 0 ? index : FindBackwards(text, query, text.Length - query.Length,
                    start + 1, comparison, wholeWord);
            }
            var forward = FindForwards(text, query, Math.Max(0, start), text.Length - query.Length,
                comparison, wholeWord);
            return forward >= 0 ? forward : FindForwards(text, query, 0, start - 1, comparison, wholeWord);
        }

        private static int FindForwards(string text, string query, int start, int last,
            StringComparison comparison, bool wholeWord)
        {
            while (start <= last)
            {
                var index = text.IndexOf(query, start, comparison);
                if (index < 0 || index > last) return -1;
                if (!wholeWord || IsWholeWord(text, index, query.Length)) return index;
                start = index + 1;
            }
            return -1;
        }

        private static int FindBackwards(string text, string query, int start, int first,
            StringComparison comparison, bool wholeWord)
        {
            while (start >= first)
            {
                // LastIndexOf's start is the final character that may participate in a match.
                var index = text.LastIndexOf(query, start + query.Length - 1, comparison);
                if (index < first) return -1;
                if (!wholeWord || IsWholeWord(text, index, query.Length)) return index;
                start = index - 1;
            }
            return -1;
        }

        internal static bool IsWholeWord(string text, int start, int length) =>
            (start == 0 || !IsWordCharacter(text, start - 1)) &&
            (start + length == text.Length || !IsWordCharacter(text, start + length));

        private static bool IsWordCharacter(string text, int index)
        {
            // The preceding UTF-16 offset can point at a low surrogate. Classify
            // the complete scalar: an astral letter is a word character, an emoji is not.
            if (index > 0 && char.IsLowSurrogate(text[index]) && char.IsHighSurrogate(text[index - 1])) index--;
            var category = char.GetUnicodeCategory(text, index);
            return char.IsLetterOrDigit(text, index) ||
                category == UnicodeCategory.ConnectorPunctuation ||
                category == UnicodeCategory.NonSpacingMark || category == UnicodeCategory.SpacingCombiningMark;
        }
    }
}
