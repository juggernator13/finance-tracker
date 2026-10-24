namespace Finance.Contracts;

public record CreateTransactions(Guid BatchId, Guid UserId, Guid AccountId, IReadOnlyList<TransactionItem> Items);

public record CategorizeBatch(Guid BatchId, Guid UserId, Guid AccountId, IReadOnlyList<TransactionItem> Items);

public record DeleteBatch(Guid BatchId, Guid UserId);