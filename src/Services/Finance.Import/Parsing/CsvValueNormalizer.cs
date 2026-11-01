using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace Finance.Import;

public static class CsvValueNormalizer
{
    public static string GetHeader(string text)
    {
        string first = text.Split('\n')[0];
        return first.Trim('\r', '\uFEFF');
    }

    public static List<(int Number, string Text)> GetDataLines(string text)
    {
        var result = new List<(int, string)>();
        string[] lines = text.Split('\n');

        for (int i = 1; i < lines.Length; i++)
        {
            string line = lines[i].TrimEnd('\r');
            if (string.IsNullOrWhiteSpace(line))
            {
                continue;
            }
            result.Add((i + 1, line));
        }

        return result;
    }

    public static List<string> SplitLine(string line, char delimiter)
    {
        var fields = new List<string>();
        var current = new StringBuilder();
        bool inQuotes = false;

        foreach (char c in line)
        {
            if (c == '"')
            {
                inQuotes = !inQuotes;
            }
            else if (c == delimiter && !inQuotes)
            {
                fields.Add(current.ToString());
                current.Clear();
            }
            else
            {
                current.Append(c);
            }
        }
        fields.Add(current.ToString());
        return fields;
    }

    public static decimal ParseDecimal(string text)
    {
        string cleaned = text
            .Replace(" ", "")
            .Replace("\u00A0", "")
            .Replace(",", ".");
        bool parse = decimal.TryParse(cleaned, NumberStyles.Float, CultureInfo.InvariantCulture, out decimal value);

        if (parse == false)
        {
            throw new ArgumentException($"Invalid amount '{text}'");
        }

        return value;
    }

    public static DateOnly ParseDate(string text, string format)
    {
        if (!DateOnly.TryParseExact(text.Trim(), format, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateOnly date))
        {
            throw new ArgumentException($"Invalid date '{text}'");
        }

        return date;
    }

    public static string CleanDescription(string text)
    {
        string clean = Regex.Replace(text.Trim(), @"\s+", " ");

        if (clean == "")
        {
            throw new ArgumentException("Description is empty");
        }

        return clean;
    }
}