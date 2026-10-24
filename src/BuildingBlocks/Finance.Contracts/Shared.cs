namespace Finance.Contracts;

public record TransactionItem(Guid Id, DateOnly Date, decimal Amount, string Description, int Ordinal);

public record CategorizedItem(
    Guid TransactionId,
    Guid AccountId,
    DateOnly Date,
    decimal Amount,
    Guid CategoryId,
    string CategoryName);