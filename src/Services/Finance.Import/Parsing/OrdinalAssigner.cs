namespace Finance.Import;

public static class OrdinalAssigner
{
    public static List<NormalizedRow> Assign(List<NormalizedRow> rows)
    {
        var counters = new Dictionary<(DateOnly, decimal, string), int>();
        var result = new List<NormalizedRow>();

        foreach (var row in rows)
        {
            var key = (row.Date, row.Amount, row.Description);
            counters.TryGetValue(key, out int n);   // если ключа нет, n = 0

            result.Add(row with { Ordinal = n });
            counters[key] = n + 1;
        }

        return result;
    }
}