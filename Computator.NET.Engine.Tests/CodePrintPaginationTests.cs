using System.Drawing;
using System.Globalization;
using Computator.NET.Desktop.Controls.CodeEditors.Printing;
using NUnit.Framework;

namespace Computator.NET.Engine.Tests;

[TestFixture]
public class CodePrintPaginationTests
{
    [TestCase("first\r\n\r\nlast\r\n")]
    [TestCase("first\n\nlast\n")]
    [TestCase("first\r\rlast\r")]
    public void LineEndingsPreserveBlankLinesWithoutAnExtraTerminalPage(string text)
    {
        var paginator = new CodePrintPaginator(text);
        NUnit.Framework.Assert.That(paginator.ReadPage(80, 3, Width),
            Is.EqualTo(new[] { "first", "", "last" }));
        NUnit.Framework.Assert.That(paginator.HasMoreLines, Is.False);
        NUnit.Framework.Assert.That(paginator.ReadPage(80, 3, Width), Is.Empty);
    }

    [TestCase("")]
    [TestCase(null)]
    public void EmptyDocumentsHaveNoRows(string? text)
    {
        var paginator = new CodePrintPaginator(text);
        NUnit.Framework.Assert.That(paginator.HasMoreLines, Is.False);
        NUnit.Framework.Assert.That(paginator.ReadPage(80, 10, Width), Is.Empty);
    }

    [Test]
    public void WrappedLinesContinueAcrossPageBoundariesWithoutLosingText()
    {
        var paginator = new CodePrintPaginator("abcdefghij\nnext");
        NUnit.Framework.Assert.That(paginator.ReadPage(3, 2, Width), Is.EqualTo(new[] { "abc", "def" }));
        NUnit.Framework.Assert.That(paginator.HasMoreLines, Is.True);
        NUnit.Framework.Assert.That(paginator.ReadPage(3, 2, Width), Is.EqualTo(new[] { "ghi", "j" }));
        NUnit.Framework.Assert.That(paginator.ReadPage(3, 2, Width), Is.EqualTo(new[] { "nex", "t" }));
        NUnit.Framework.Assert.That(paginator.HasMoreLines, Is.False);
    }

    [Test]
    public void WidthChangesApplyToTheUnprintedRemainder()
    {
        var paginator = new CodePrintPaginator("abcdefghij");
        NUnit.Framework.Assert.That(paginator.ReadPage(3, 1, Width), Is.EqualTo(new[] { "abc" }));
        NUnit.Framework.Assert.That(paginator.ReadPage(7, 1, Width), Is.EqualTo(new[] { "defghij" }));
        NUnit.Framework.Assert.That(paginator.HasMoreLines, Is.False);
    }

    [Test]
    public void WrappingUsesMeasuredWidthAndIncludesTrailingSpaces()
    {
        var paginator = new CodePrintPaginator("WWiiii  W");
        NUnit.Framework.Assert.That(paginator.ReadPage(6, 10,
                text => text.Sum(character => character == 'W' ? 3 : 1)),
            Is.EqualTo(new[] { "WW", "iiii  ", "W" }));
    }

    [Test]
    public void WrappingKeepsSurrogatePairsAndCombiningCharactersTogether()
    {
        var paginator = new CodePrintPaginator("a\u0301😀𝑥β");
        NUnit.Framework.Assert.That(paginator.ReadPage(1, 10, Width),
            Is.EqualTo(new[] { "a\u0301", "😀", "𝑥", "β" }));
    }

    [Test]
    public void TabsUseFourColumnStopsWithoutSplittingUnicodeElements()
    {
        var paginator = new CodePrintPaginator("ab\tc\t\na\u0301😀\tx");
        NUnit.Framework.Assert.That(paginator.ReadPage(80, 10, Width),
            Is.EqualTo(new[] { "ab  c   ", "a\u0301😀  x" }));
    }

    [Test]
    public void VeryLongLinesMeasureOnlyABoundedPrefixOfEachPrintedRow()
    {
        var text = new string('x', 100000);
        var paginator = new CodePrintPaginator(text);
        var longestMeasurement = 0;
        var rows = paginator.ReadPage(80, 30, value =>
        {
            longestMeasurement = Math.Max(longestMeasurement, value.Length);
            return value.Length;
        });
        NUnit.Framework.Assert.That(rows.Count, Is.EqualTo(30));
        NUnit.Framework.Assert.That(rows.All(row => row == new string('x', 80)), Is.True);
        NUnit.Framework.Assert.That(longestMeasurement, Is.LessThanOrEqualTo(160),
            "A very long source line must not be measured in full for every printed row.");
        NUnit.Framework.Assert.That(paginator.HasMoreLines, Is.True);
    }

    [TestCase(0)]
    [TestCase(-1)]
    [TestCase(double.NaN)]
    [TestCase(double.PositiveInfinity)]
    public void InvalidPageWidthsAreRejected(double width)
    {
        NUnit.Framework.Assert.Throws<ArgumentOutOfRangeException>(() =>
            new CodePrintPaginator("a").ReadPage(width, 1, Width));
    }

    [TestCase(0)]
    [TestCase(-1)]
    public void NoUsableRowsAreRejected(int rows)
    {
        NUnit.Framework.Assert.Throws<ArgumentOutOfRangeException>(() =>
            new CodePrintPaginator("a").ReadPage(80, rows, Width));
    }

    [TestCase(-1)]
    [TestCase(double.NaN)]
    [TestCase(double.PositiveInfinity)]
    public void InvalidPrinterMeasurementsAreRejected(double measurement)
    {
        NUnit.Framework.Assert.Throws<CodePrintLayoutException>(() =>
            new CodePrintPaginator("a").ReadPage(80, 1, _ => measurement));
    }

    [Test]
    public void AnOversizedTextElementFailsWithoutProducingEndlessEmptyPages()
    {
        var paginator = new CodePrintPaginator("😀");
        NUnit.Framework.Assert.Throws<CodePrintLayoutException>(() => paginator.ReadPage(1, 1, _ => 2));
        NUnit.Framework.Assert.That(paginator.ReadPage(2, 1, _ => 2), Is.EqualTo(new[] { "😀" }));
        NUnit.Framework.Assert.That(paginator.HasMoreLines, Is.False);
    }

    [Test]
    public void UserMarginsInsidePrintableAreaUseTheUserMarginOrigin()
    {
        NUnit.Framework.Assert.That(CodePrintPageLayout.ContentBounds(
            new Rectangle(100, 100, 650, 900), new RectangleF(25, 50, 800, 1025)),
            Is.EqualTo(new RectangleF(0, 0, 650, 900)));
    }

    [Test]
    public void AsymmetricFractionalHardMarginsClipAllFourEdges()
    {
        NUnit.Framework.Assert.That(CodePrintPageLayout.ContentBounds(
            new Rectangle(10, 20, 800, 1000), new RectangleF(25.5f, 40.25f, 760.75f, 960.5f)),
            Is.EqualTo(new RectangleF(15.5f, 20.25f, 760.75f, 960.5f)));
    }

    [Test]
    public void LandscapeGeometryIntersectsPhysicalCoordinatesBeforeChangingOrigin()
    {
        NUnit.Framework.Assert.That(CodePrintPageLayout.ContentBounds(
            new Rectangle(50, 100, 1000, 650), new RectangleF(25, 125, 980, 700)),
            Is.EqualTo(new RectangleF(0, 25, 955, 625)));
    }

    [Test]
    public void MarginsOutsideThePrintableAreaAreRejected()
    {
        NUnit.Framework.Assert.Throws<CodePrintLayoutException>(() => CodePrintPageLayout.ContentBounds(
            new Rectangle(500, 500, 10, 10), new RectangleF(25, 25, 100, 100)));
    }

    [TestCase(0)]
    [TestCase(-1)]
    [TestCase(float.NaN)]
    [TestCase(float.PositiveInfinity)]
    public void InvalidPrinterAreaIsRejected(float width)
    {
        NUnit.Framework.Assert.Throws<CodePrintLayoutException>(() => CodePrintPageLayout.ContentBounds(
            new Rectangle(10, 10, 80, 100), new RectangleF(0, 0, width, 100)));
    }

    private static double Width(string text) => StringInfo.ParseCombiningCharacters(text).Length;
}
