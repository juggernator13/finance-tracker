namespace Finance.AccountsTransactions;

public record ImportItem(DateOnly Date, decimal Amount, string Description, int Ordinal);