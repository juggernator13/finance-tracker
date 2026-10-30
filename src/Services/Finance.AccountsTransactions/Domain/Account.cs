namespace Finance.AccountsTransactions;

public class Account
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Currency { get; set; } = "RUB";
}
