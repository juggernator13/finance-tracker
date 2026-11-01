namespace Finance.Import.Tests;

public class CsvValueNormalizerTests
{
    [Theory]
    [InlineData("-1 250,50", -1250.50)]
    [InlineData("2500.00", 2500)]
    [InlineData("1\u00A0250,50", 1250.50)]
    [InlineData("0,99", 0.99)]
    public void ParseDecimal_ValidValues(string text, double expected)
    {
        Assert.Equal((decimal)expected, CsvValueNormalizer.ParseDecimal(text));
    }

    [Theory]
    [InlineData("abc")]
    [InlineData("")]
    [InlineData("12,3,4")]
    public void ParseDecimal_Invalid_Throws(string text)
    {
        Assert.Throws<ArgumentException>(() => CsvValueNormalizer.ParseDecimal(text));
    }

    [Fact]
    public void ParseDate_ValidFormat()
    {
        var date = CsvValueNormalizer.ParseDate("20.10.2026", "dd.MM.yyyy");
        Assert.Equal(new DateOnly(2026, 10, 20), date);
    }

    [Fact]
    public void ParseDate_WrongFormat_Throws()
    {
        Assert.Throws<ArgumentException>(() => CsvValueNormalizer.ParseDate("2026-10-20", "dd.MM.yyyy"));
    }

    [Fact]
    public void CleanDescription_TrimsAndCollapsesSpaces()
    {
        Assert.Equal("Кофе в кафе", CsvValueNormalizer.CleanDescription("  Кофе   в   кафе "));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void CleanDescription_Empty_Throws(string text)
    {
        Assert.Throws<ArgumentException>(() => CsvValueNormalizer.CleanDescription(text));
    }

    [Fact]
    public void SplitLine_KeepsDelimiterInsideQuotes()
    {
        var fields = CsvValueNormalizer.SplitLine("a,\"b,c\",d", ',');

        Assert.Equal(new[] { "a", "b,c", "d" }, fields);
    }

    [Fact]
    public void SplitLine_EmptyLastField()
    {
        var fields = CsvValueNormalizer.SplitLine("a,b,,", ',');

        Assert.Equal(4, fields.Count);
        Assert.Equal("", fields[3]);
    }

    [Fact]
    public void GetHeader_ReturnsFirstLineWithoutCarriageReturn()
    {
        Assert.Equal("H1;H2", CsvValueNormalizer.GetHeader("H1;H2\r\nrow"));
    }

    [Fact]
    public void GetDataLines_SkipsHeaderAndEmptyLines_AndNumbersFromFileStart()
    {
        var lines = CsvValueNormalizer.GetDataLines("H\r\na\r\n\r\nb");

        Assert.Equal(2, lines.Count);
        Assert.Equal((2, "a"), lines[0]);
        Assert.Equal((4, "b"), lines[1]);
    }
}