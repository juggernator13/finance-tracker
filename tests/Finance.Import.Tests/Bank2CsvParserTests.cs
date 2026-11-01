namespace Finance.Import.Tests;

public class Bank2CsvParserTests
{
    private readonly Bank2CsvParser _parser = new();

    [Fact]
    public void CanParse_Bank2Header_True()
    {
        Assert.True(_parser.CanParse(TestHelpers.Bank2Header));
    }

    [Fact]
    public void CanParse_Bank1Header_False()
    {
        Assert.False(_parser.CanParse(TestHelpers.Bank1Header));
    }

    [Fact]
    public void Parse_DebitIsNegative_CreditIsPositive()
    {
        var rows = _parser.Parse(TestHelpers.Bank2Csv);

        Assert.Equal(2, rows.Count);
        Assert.Equal(-35.99m, rows[0].Amount);
        Assert.Equal(2500m, rows[1].Amount);
        Assert.Equal(new DateOnly(2026, 10, 22), rows[1].Date);
    }

    [Fact]
    public void Parse_QuotedDescriptionWithComma_IsOneField()
    {
        var rows = _parser.Parse(TestHelpers.Bank2Csv);

        Assert.Equal("Amazon, Inc.", rows[0].Description);
    }

    [Fact]
    public void Parse_BothDebitAndCreditEmpty_Throws()
    {
        string csv = TestHelpers.Bank2Header + "\n2026-10-20,X,,";

        var ex = Assert.Throws<ArgumentException>(() => _parser.Parse(csv));

        Assert.Contains("Line 2", ex.Message);
    }

    [Fact]
    public void Parse_InvalidDate_ThrowsWithLineNumber()
    {
        string csv = TestHelpers.Bank2Header + "\n2026-10-20,A,1,\n2026-13-01,B,1,";

        var ex = Assert.Throws<ArgumentException>(() => _parser.Parse(csv));

        Assert.Contains("Line 3", ex.Message);
    }
}