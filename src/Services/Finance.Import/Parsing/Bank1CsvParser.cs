namespace Finance.Import;

public class Bank1CsvParser : IStatementParser
{
    public string Format
    {
        get
        {
            return "bank1";
        }
    }

    public bool CanParse(string headerLine)
    {
        return headerLine.Contains("Дата операции");
    }

    public List<NormalizedRow> Parse(string csvText)
    {
        var rows = new List<NormalizedRow>();

        foreach (var (number, text) in CsvValueNormalizer.GetDataLines(csvText))
        {
            try
            {
                var f = CsvValueNormalizer.SplitLine(text, ';');

                if (f.Count < 3)
                {
                    throw new ArgumentException("Expected 3 columns");
                }

                rows.Add(new NormalizedRow(
                    CsvValueNormalizer.ParseDate(f[0], "dd.MM.yyyy"),
                    CsvValueNormalizer.ParseDecimal(f[2]),
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