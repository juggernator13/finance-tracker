namespace Finance.Import.Tests;

public class Bank1CsvParserTests
{
    private readonly Bank1CsvParser _parser = new();

    [Fact]
    public void CanParse_Bank1Header_True()
    {
        Assert.True(_parser.CanParse(TestHelpers.Bank1Header));
    }

    [Fact]
    public void CanParse_Bank2Header_False()
    {
        Assert.False(_parser.CanParse(TestHelpers.Bank2Header));
    }

    [Fact]
    public void Parse_ReadsAllRows()
    {
        var rows = _parser.Parse(TestHelpers.Bank1Csv);

        Assert.Equal(3, rows.Count);
        Assert.Equal(new DateOnly(2026, 10, 20), rows[0].Date);
        Assert.Equal(-1250.50m, rows[0].Amount);
        Assert.Equal("Пятёрочка", rows[0].Description);
    }

    [Fact]
    public void Parse_IncomeIsPositive_ExpenseIsNegative()
    {
        var rows = _parser.Parse(TestHelpers.Bank1Header + "\n21.10.2026;Зарплата;85 000,00\n21.10.2026;Кофе;-150,00");

        Assert.Equal(85000m, rows[0].Amount);
        Assert.Equal(-150m, rows[1].Amount);
    }

    [Fact]
    public void Parse_InvalidDate_ThrowsWithLineNumber()
    {
        string csv = TestHelpers.Bank1Header + "\n20.10.2026;A;-1\n32.10.2026;B;-2";

        var ex = Assert.Throws<ArgumentException>(() => _parser.Parse(csv));

        Assert.Contains("Line 3", ex.Message);
    }

    [Fact]
    public void Parse_NotEnoughColumns_Throws()
    {
        string csv = TestHelpers.Bank1Header + "\n20.10.2026;A";

        var ex = Assert.Throws<ArgumentException>(() => _parser.Parse(csv));

        Assert.Contains("Line 2", ex.Message);
    }

    [Fact]
    public void Parse_WindowsLineEndings_WorkTheSame()
    {
        var rows = _parser.Parse(TestHelpers.Bank1Csv.Replace("\n", "\r\n"));

        Assert.Equal(3, rows.Count);
        Assert.Equal(-150m, rows[2].Amount);
    }
}