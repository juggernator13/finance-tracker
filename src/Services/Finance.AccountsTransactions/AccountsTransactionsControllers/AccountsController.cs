using Finance.Common;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Finance.AccountsTransactions;

[ApiController]
[Route("api/accounts")]
[Authorize]
public class AccountsController(AccountService accounts, TransactionService transactions) : ControllerBase
{
    [HttpGet]
    public Task<List<AccountDto>> GetAll()
        => accounts.GetAllAsync(User.GetUserId());

    [HttpPost]
    public Task<AccountDto> Create(CreateAccountRequest request)
        => accounts.CreateAsync(User.GetUserId(), request.Name, request.Currency);

    [HttpGet("{id}")]
    public Task<AccountDto> Get(Guid id)
        => accounts.GetAsync(User.GetUserId(), id);

    [HttpPut("{id}")]
    public Task<AccountDto> Update(Guid id, UpdateAccountRequest request)
        => accounts.UpdateAsync(User.GetUserId(), id, request.Name);

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        await accounts.DeleteAsync(User.GetUserId(), id);
        return NoContent();
    }

    [HttpGet("{id}/transactions")]
    public Task<List<TransactionDto>> GetTransactions(Guid id, DateOnly? from, DateOnly? to, int page = 1, int size = 50)
        => transactions.GetListAsync(User.GetUserId(), id, from, to, page, size);

    [HttpPost("{id}/transactions")]
    public Task<TransactionDto> AddTransaction(Guid id, CreateTransactionRequest request)
        => transactions.AddAsync(User.GetUserId(), id, request);
}