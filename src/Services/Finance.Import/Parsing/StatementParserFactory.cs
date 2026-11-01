namespace Finance.Import;

public class StatementParserFactory(IEnumerable<IStatementParser> parsers)
{
    private readonly List<IStatementParser> _parsers = parsers.ToList();

    public List<string> Formats => _parsers.Select(p => p.Format).ToList();

    public IStatementParser Resolve(string? format, string headerLine)
    {
        if (!string.IsNullOrWhiteSpace(format))
        {
            var byName = _parsers.FirstOrDefault(p =>
                p.Format.Equals(format.Trim(), StringComparison.OrdinalIgnoreCase));

            if (byName == null)
            {
                throw new ArgumentException($"Unknown format '{format}'");
            }

            return byName;
        }

        var detected = _parsers.FirstOrDefault(p => p.CanParse(headerLine));

        if (detected == null)
        {
            throw new ArgumentException("Cannot detect format");
        }

        return detected;
    }
}