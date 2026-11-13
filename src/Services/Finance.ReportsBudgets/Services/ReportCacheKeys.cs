namespace Finance.ReportsBudgets;

public static class ReportCacheKeys
{
    public static string ByCategory(Guid userId, int year, int month) => $"report:{userId}:{year}:{month}";
}