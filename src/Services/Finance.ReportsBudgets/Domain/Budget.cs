namespace Finance.ReportsBudgets;

public class Budget
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public Guid CategoryId { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public decimal Limit { get; set; }
}