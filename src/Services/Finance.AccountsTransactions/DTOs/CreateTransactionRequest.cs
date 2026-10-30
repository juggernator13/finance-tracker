namespace Finance.AccountsTransactions;

public record CreateTransactionRequest(DateOnly Date, decimal Amount, string Description);