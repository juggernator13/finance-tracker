namespace Finance.ReportsBudgets;

public record CategoryReportItem(Guid CategoryId, string CategoryName, decimal Expense, decimal Income);