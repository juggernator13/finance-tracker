namespace Finance.Import;

public interface IStatementParser
{
    string Format { get; }
    bool CanParse(string headerLine);
    List<NormalizedRow> Parse(string csvText);
}