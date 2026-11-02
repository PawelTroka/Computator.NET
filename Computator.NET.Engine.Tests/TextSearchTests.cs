using Computator.NET.Desktop.Controls.CodeEditors;
using NUnit.Framework;

namespace Computator.NET.Engine.Tests;

[TestFixture]
public class TextSearchTests
{
    [TestCase("", "x", new int[0])]
    [TestCase("abc", "", new int[0])]
    [TestCase("abc", "abcd", new int[0])]
    [TestCase("abc", "missing", new int[0])]
    [TestCase("abc", "abc", new[] { 0 })]
    [TestCase("aaaaa", "aa", new[] { 0, 2 })]
    [TestCase("aba aba", "aba", new[] { 0, 4 })]
    [TestCase("a.*b .* c", ".*", new[] { 1, 5 })]
    [TestCase("a\r\na\na", "a", new[] { 0, 3, 5 })]
    [TestCase("😀x😀x", "x", new[] { 2, 5 })]
    [TestCase("x😀x😀", "😀", new[] { 1, 4 })]
    public void FindAllReturnsNonOverlappingLiteralUtf16Offsets(string text, string query, int[] expected)
    {
        NUnit.Framework.Assert.That(TextSearch.FindAll(text, query, true, false), Is.EqualTo(expected));
    }

    [TestCase(false, 0, 0)]
    [TestCase(false, 1, 4)]
    [TestCase(false, 4, 4)]
    [TestCase(false, 5, 0)]
    [TestCase(false, 7, 0)]
    [TestCase(true, -1, 4)]
    [TestCase(true, 0, 0)]
    [TestCase(true, 3, 0)]
    [TestCase(true, 4, 4)]
    [TestCase(true, 7, 4)]
    public void DirectionalSearchWrapsOnceAndIncludesItsStartingOffset(bool backwards, int start, int expected)
    {
        NUnit.Framework.Assert.That(TextSearch.Find("aba aba", "aba", start, backwards, true, false), Is.EqualTo(expected));
    }

    [TestCase("x", true)]
    [TestCase("(x)", true)]
    [TestCase("😀x😀", true)]
    [TestCase("𐐀x", false)]
    [TestCase("x𐐀", false)]
    [TestCase("αx", false)]
    [TestCase("xα", false)]
    [TestCase("x2", false)]
    [TestCase("_x", false)]
    [TestCase("x_y", false)]
    [TestCase("x\u0301", false)]
    [TestCase("\u0301x", false)]
    [TestCase("x\u20DD", false)]
    [TestCase("\u20DDx", false)]
    public void WholeWordUsesUnicodeCharactersAtBothBoundaries(string text, bool expected)
    {
        var matches = TextSearch.FindAll(text, "x", true, true);
        NUnit.Framework.Assert.That(matches.Count, Is.EqualTo(expected ? 1 : 0));
        if (expected) NUnit.Framework.Assert.That(matches[0], Is.EqualTo(text.IndexOf('x')));
    }

    [Test]
    public void WholeWordSkipsPartialCandidatesInBothDirectionsAndAfterWrapping()
    {
        const string text = "prefix x x2 x_suffix x";
        NUnit.Framework.Assert.That(TextSearch.FindAll(text, "x", true, true), Is.EqualTo(new[] { 7, 21 }));
        NUnit.Framework.Assert.That(TextSearch.Find(text, "x", 8, false, true, true), Is.EqualTo(21));
        NUnit.Framework.Assert.That(TextSearch.Find(text, "x", 20, true, true, true), Is.EqualTo(7));
        NUnit.Framework.Assert.That(TextSearch.Find(text, "x", 22, false, true, true), Is.EqualTo(7));
        NUnit.Framework.Assert.That(TextSearch.Find(text, "x", 6, true, true, true), Is.EqualTo(21));
        NUnit.Framework.Assert.That(TextSearch.Find("xx", "x", 0, false, true, true), Is.EqualTo(-1));
        NUnit.Framework.Assert.That(TextSearch.Find("xx", "x", 0, true, true, true), Is.EqualTo(-1));
    }

    [Test]
    public void WholeWordSearchDoesNotSplitEnclosingCombiningMarks()
    {
        const string text = "x\u20DD x x\u20E3";
        NUnit.Framework.Assert.That(TextSearch.FindAll(text, "x", true, true), Is.EqualTo(new[] { 3 }));
        NUnit.Framework.Assert.That(TextSearch.FindAll(text, "x", true, false), Is.EqualTo(new[] { 0, 3, 5 }));
        NUnit.Framework.Assert.That(TextSearch.Find(text, "x", 0, false, true, true), Is.EqualTo(3));
        NUnit.Framework.Assert.That(TextSearch.Find(text, "x", text.Length, true, true, true), Is.EqualTo(3));
    }

    [Test, SetCulture("tr-TR")]
    public void OrdinalCaseMatchingDoesNotDependOnTheUiCulture()
    {
        NUnit.Framework.Assert.That(TextSearch.FindAll("i I İ ı", "i", false, false), Is.EqualTo(new[] { 0, 2 }));
        NUnit.Framework.Assert.That(TextSearch.FindAll("i I İ ı", "i", true, false), Is.EqualTo(new[] { 0 }));
        NUnit.Framework.Assert.That(TextSearch.Find("i I İ ı", "i", 1, false, false, false), Is.EqualTo(2));
        NUnit.Framework.Assert.That(TextSearch.Find("i I İ ı", "i", 1, true, false, false), Is.EqualTo(0));
    }

    [Test]
    public void DirectionalSearchMatchesIndependentCandidateEnumerationAtEveryBoundary()
    {
        foreach (var text in new[] { "", "a", "ab", "aaaaa", "ababa", "aba bba aba", "😀a😀a", "a\r\na\na" })
        foreach (var query in new[] { "", "a", "b", "aa", "aba", "😀", "missing" })
        {
            var candidates = query.Length == 0 || query.Length > text.Length
                ? Array.Empty<int>()
                : Enumerable.Range(0, text.Length - query.Length + 1)
                    .Where(index => text.AsSpan(index, query.Length).SequenceEqual(query.AsSpan())).ToArray();
            foreach (var start in Enumerable.Range(-2, text.Length + 5).Concat(new[] { int.MinValue, int.MaxValue }))
            {
                var expectedNext = candidates.Where(index => index >= Math.Max(0, start)).DefaultIfEmpty(-1).First();
                if (expectedNext == -1) expectedNext = candidates.DefaultIfEmpty(-1).First();
                var expectedPrevious = candidates.Where(index => index <= start).DefaultIfEmpty(-1).Last();
                if (expectedPrevious == -1) expectedPrevious = candidates.DefaultIfEmpty(-1).Last();
                NUnit.Framework.Assert.That(TextSearch.Find(text, query, start, false, true, false),
                    Is.EqualTo(expectedNext), $"Forward: '{text}', '{query}', {start}");
                NUnit.Framework.Assert.That(TextSearch.Find(text, query, start, true, true, false),
                    Is.EqualTo(expectedPrevious), $"Backward: '{text}', '{query}', {start}");
            }
        }
    }
}
