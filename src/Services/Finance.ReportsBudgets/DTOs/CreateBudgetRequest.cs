namespace Finance.ReportsBudgets;

public record CreateBudgetRequest(Guid CategoryId, string CategoryName, decimal Limit);