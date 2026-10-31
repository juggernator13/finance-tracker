using Finance.Common;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Finance.AccountsTransactions;

[ApiController]
[Route("api/transactions")]
[Authorize]
public class TransactionsController(TransactionService transactions) : ControllerBase
{
    [HttpPut("{id}")]
    public Task<TransactionDto> Update(Guid id, CreateTransactionRequest request)
        => transactions.UpdateAsync(User.GetUserId(), id, request);

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        await transactions.DeleteAsync(User.GetUserId(), id);
        return NoContent();
    }
}