namespace Finance.ReportsBudgets;

public record BudgetDto(Guid Id, Guid CategoryId, string CategoryName, decimal Limit, decimal Spent, decimal Percent, bool IsExceeded);