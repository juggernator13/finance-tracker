using Finance.Contracts;

namespace Finance.Import.Tests;

public class FakeAccountsClient : IAccountsClient
{
    public bool IsOwner { get; set; } = true;
    public Exception? ImportError { get; set; }
    public ImportResult? Result { get; set; }
    public List<NormalizedRow>? ReceivedRows { get; private set; }

    public Task<bool> IsOwnerAsync(Guid userId, Guid accountId) => Task.FromResult(IsOwner);

    public Task<ImportResult> ImportAsync(Guid batchId, Guid userId, Guid accountId, List<NormalizedRow> rows)
    {
        if (ImportError != null) throw ImportError;

        ReceivedRows = rows;
        if (Result != null) return Task.FromResult(Result);

        var items = rows
            .Select(r => new TransactionItem(Guid.NewGuid(), accountId, r.Date, r.Amount, r.Description))
            .ToList();
        return Task.FromResult(new ImportResult(rows.Count, 0, items));
    }
}