namespace Finance.AccountsTransactions;

public record TransactionDto(Guid Id, Guid AccountId, DateOnly Date, decimal Amount, string Description, Guid? CategoryId);