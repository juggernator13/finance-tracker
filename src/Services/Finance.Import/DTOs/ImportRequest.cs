namespace Finance.Import;

public record ImportRequest(Guid BatchId, Guid UserId, Guid AccountId, List<ImportItem> Items);