namespace Finance.ReportsBudgets;

public class MonthlyTotal
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public Guid CategoryId { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public int Year { get; set; }
    public int Month { get; set; }
    public decimal Expense { get; set; }
    public decimal Income { get; set; }
}
