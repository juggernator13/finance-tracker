using Microsoft.AspNetCore.Mvc;

namespace Finance.AccountsTransactions;

[ApiController]
[Route("internal/transactions")]
public class InternalTransactionsController(TransactionService transactions) : ControllerBase
{
    [HttpPost("import")]
    public async Task<ImportResult> Import(ImportRequest request)
    {
        return await transactions.AddBatchAsync(
            request.UserId, request.AccountId, request.BatchId, request.Items);
    }
}