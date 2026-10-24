namespace Finance.Contracts;

public record TransactionsCreated(Guid BatchId, Guid UserId, int Created, int Skipped);

public record TransactionsCreationFailed(Guid BatchId, Guid UserId, string Reason);

public record TransactionsCategorized(Guid BatchId, Guid UserId, IReadOnlyList<CategorizedItem> Items);

public record CategorizationFailed(Guid BatchId, Guid UserId, string Reason);

public record BatchDeleted(Guid BatchId, Guid UserId);

public record ImportStatusChanged(Guid BatchId, Guid UserId, string Status, string? Details);

public record BatchCompleted(Guid BatchId, Guid UserId);

public record BudgetExceeded(Guid UserId, Guid CategoryId, string CategoryName, decimal Limit, decimal Spent);