namespace Finance.ReportsBudgets;

public record AggregateChange(Guid CategoryId, string CategoryName, int Year, int Month, decimal Before, decimal After);