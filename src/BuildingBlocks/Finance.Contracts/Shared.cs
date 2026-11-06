namespace Finance.Contracts;

public record TransactionItem(Guid Id, Guid AccountId, DateOnly Date, decimal Amount, string Description);

public record CategorizedItem(
    Guid TransactionId,
    Guid AccountId,
    DateOnly Date,
    decimal Amount,
    Guid CategoryId,
    string CategoryName);