namespace Finance.Import;

public class Bank2CsvParser : IStatementParser
{
    public string Format
    {
        get
        {
            return "bank2";
        }
    }

    public bool CanParse(string headerLine)
    {
        return headerLine.StartsWith("Date,", StringComparison.OrdinalIgnoreCase);
    }

    public List<NormalizedRow> Parse(string csvText)
    {
        var rows = new List<NormalizedRow>();

        foreach (var (number, text) in CsvValueNormalizer.GetDataLines(csvText))
        {
            try
            {
                var f = CsvValueNormalizer.SplitLine(text, ',');

                if (f.Count < 4)
                {
                    throw new ArgumentException("Expected 4 columns");
                }

                if (string.IsNullOrWhiteSpace(f[2]) && string.IsNullOrWhiteSpace(f[3]))
                {
                    throw new ArgumentException("Both Debit and Credit are empty");
                }

                decimal debit = string.IsNullOrWhiteSpace(f[2]) ? 0 : CsvValueNormalizer.ParseDecimal(f[2]);
                decimal credit = string.IsNullOrWhiteSpace(f[3]) ? 0 : CsvValueNormalizer.ParseDecimal(f[3]);

                rows.Add(new NormalizedRow(
                    CsvValueNormalizer.ParseDate(f[0], "yyyy-MM-dd"),
                    credit - debit,
                    CsvValueNormalizer.CleanDescription(f[1])));
            }
            catch (ArgumentException ex)
            {
                throw new ArgumentException($"Line {number}: {ex.Message}");
            }
        }

        return rows;
    }
}