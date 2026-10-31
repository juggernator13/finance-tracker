namespace Finance.AccountsTransactions;

public record ImportRequest(Guid BatchId, Guid UserId, Guid AccountId, List<ImportItem> Items);