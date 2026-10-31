namespace Finance.AccountsTransactions;

public class Transaction
{
    public Guid Id { get; set; }
    public Guid AccountId { get; set; }
    public Guid? BatchId { get; set; }
    public DateOnly Date { get; set; }
    public decimal Amount { get; set; }
    public string Description { get; set; } = string.Empty;
    public string Hash { get; set; } = string.Empty;
    public Guid CategoryId { get; set; }
}
