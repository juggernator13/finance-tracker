using Microsoft.AspNetCore.Mvc;

namespace Finance.AccountsTransactions;

[ApiController]
[Route("internal/accounts")]
public class InternalAccountsController(AccountService accounts) : ControllerBase
{
    [HttpGet("{id}")]
    public async Task<IActionResult> Check(Guid id, Guid userId)
    {
        bool isOwner = await accounts.IsOwnerAsync(userId, id);
        return isOwner ? Ok() : NotFound();
    }
}