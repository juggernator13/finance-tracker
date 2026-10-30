using Microsoft.EntityFrameworkCore;

namespace Finance.AccountsTransactions;

public class AccountService(AccountsDbContext db)
{
    private async Task<Account> GetOwnAsync(Guid userId, Guid id)
    {
        Account? account = await db.Accounts.FirstOrDefaultAsync(a => a.Id == id && a.UserId == userId);

        if (account == null)
        {
            throw new KeyNotFoundException("Account not found");
        }

        return account;
    }

    public async Task<AccountDto> CreateAsync(Guid userId, string name, string currency)
    {
        if (string.IsNullOrEmpty(name))
        {
            throw new ArgumentException("Incorrect Name");
        }

        if (string.IsNullOrEmpty(currency))
        {
            currency = "RUB";
        }

        Account account = new Account()
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Name = name.Trim(),
            Currency = currency.Trim().ToUpper()
        };

        db.Accounts.Add(account);
        await db.SaveChangesAsync();

        return new AccountDto(account.Id, account.Name, account.Currency);
    }

    public async Task<List<AccountDto>> GetAllAsync(Guid userId)
    {
        return await db.Accounts
            .Where(a => a.UserId == userId)
            .OrderBy(a => a.Name)
            .Select(a => new AccountDto(a.Id, a.Name, a.Currency))
            .ToListAsync();
    }

    public async Task<AccountDto> GetAsync(Guid userId, Guid id)
    {
        Account account = await GetOwnAsync(userId, id);

        return new AccountDto(account.Id, account.Name, account.Currency);
    }

    public async Task<AccountDto> UpdateAsync(Guid userId, Guid id, string name)
    {
        Account account = await GetOwnAsync(userId, id);

        account.Name = name.Trim();

        await db.SaveChangesAsync();

        return new AccountDto(account.Id, account.Name, account.Currency);
    }

    public async Task DeleteAsync(Guid userId, Guid id)
    {
        Account account = await GetOwnAsync(userId, id);

        var transactions = await db.Transactions.Where(t => t.AccountId == id).ToListAsync();

        db.Transactions.RemoveRange(transactions);

        db.Accounts.Remove(account);

        await db.SaveChangesAsync();
    }

    public async Task<bool> IsOwnerAsync(Guid userId, Guid accountId)
    {
        return await db.Accounts.AnyAsync(a => a.Id == accountId && a.UserId == userId);
    }
}