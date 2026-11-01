namespace Finance.Import.Tests;

public class StatementParserFactoryTests
{
    private static StatementParserFactory Create()
    {
        return new StatementParserFactory(new IStatementParser[] { new Bank1CsvParser(), new Bank2CsvParser() });
    }

    [Fact]
    public void Resolve_ByNameIgnoringCase()
    {
        var parser = Create().Resolve("BANK1", "");

        Assert.Equal("bank1", parser.Format);
    }

    [Fact]
    public void Resolve_NoFormat_DetectsBank1ByHeader()
    {
        var parser = Create().Resolve(null, TestHelpers.Bank1Header);

        Assert.Equal("bank1", parser.Format);
    }

    [Fact]
    public void Resolve_NoFormat_DetectsBank2ByHeader()
    {
        var parser = Create().Resolve("  ", TestHelpers.Bank2Header);

        Assert.Equal("bank2", parser.Format);
    }

    [Fact]
    public void Resolve_UnknownName_Throws()
    {
        Assert.Throws<ArgumentException>(() => Create().Resolve("bank99", ""));
    }

    [Fact]
    public void Resolve_UnknownHeader_Throws()
    {
        Assert.Throws<ArgumentException>(() => Create().Resolve(null, "abc"));
    }

    [Fact]
    public void Formats_ReturnsAllNames()
    {
        Assert.Equal(new[] { "bank1", "bank2" }, Create().Formats);
    }
}