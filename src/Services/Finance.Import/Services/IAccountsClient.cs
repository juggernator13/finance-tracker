namespace Finance.Import;

public interface IAccountsClient
{
    Task<bool> IsOwnerAsync(Guid userId, Guid accountId);

    Task<ImportResult> ImportAsync(Guid batchId, Guid userId, Guid accountId, List<NormalizedRow> rows);
}
