using Microsoft.EntityFrameworkCore;

namespace Finance.AccountsTransactions;

public class TransactionService(AccountsDbContext db)
{
    private async Task<string> MakeHashAsync(Guid accountId, DateOnly date, decimal amount, string description, Guid? exceptId = null)
    {
        int ordinal = 0;

        while (true)
        {
            string hash = TransactionHasher.Compute(accountId, date, amount, description, ordinal);

            bool taken = await db.Transactions.AnyAsync(t => t.AccountId == accountId && t.Hash == hash && t.Id != exceptId);

            if (taken)
            {
                ordinal++;
            }
            else
            {
                return hash;
            }
        }
    }

    private async Task CheckAccountAsync(Guid userId, Guid accountId)
    {
        bool exists = await db.Accounts.AnyAsync(a => a.Id == accountId && a.UserId == userId);

        if (exists == false)
        {
            throw new KeyNotFoundException("Account not found");
        }
    }

    private async Task<Transaction> GetOwnAsync(Guid userId, Guid id)
    {
        var transaction = await db.Transactions.FirstOrDefaultAsync(t => t.Id == id
            && db.Accounts.Any(a => a.Id == t.AccountId && a.UserId == userId));

        if (transaction == null)
        {
            throw new KeyNotFoundException("Transaction not found");
        }

        return transaction;
    }

    public async Task<TransactionDto> AddAsync(Guid userId, Guid accountId, CreateTransactionRequest request)
    {
        await CheckAccountAsync(userId, accountId);

        string? description = request.Description?.Trim();

        if (description == null || description == "")
        {
            throw new ArgumentException("Description is required");
        }

        Transaction transaction = new Transaction()
        {
            Id = Guid.NewGuid(),
            AccountId = accountId,
            BatchId = null,
            Date = request.Date,
            Amount = request.Amount,
            Description = description,
            Hash = await MakeHashAsync(accountId, request.Date, request.Amount, description)
        };

        db.Transactions.Add(transaction);
        await db.SaveChangesAsync();

        return new TransactionDto(transaction.Id, transaction.AccountId, transaction.Date, transaction.Amount, transaction.Description, transaction.CategoryId);
    }

    public async Task<ImportResult> AddBatchAsync(Guid userId, Guid accountId, Guid batchId, List<ImportItem> items)
    {
        await CheckAccountAsync(userId, accountId);

        var hashes = items
            .Select(i => TransactionHasher.Compute(accountId, i.Date, i.Amount, i.Description.Trim(), i.Ordinal))
            .ToList();

        var existing = (await db.Transactions
            .Where(t => t.AccountId == accountId && hashes.Contains(t.Hash))
            .Select(t => t.Hash)
            .ToListAsync()).ToHashSet();

        int created = 0;

        for (int i = 0; i < items.Count; i++)
        {
            if (!existing.Add(hashes[i]))
            {
                continue;
            }

            db.Transactions.Add(new Transaction
            {
                Id = Guid.NewGuid(),
                AccountId = accountId,
                Date = items[i].Date,
                Amount = items[i].Amount,
                Description = items[i].Description.Trim(),
                Hash = hashes[i],
                BatchId = batchId
            });
            created++;
        }

        await db.SaveChangesAsync();
        return new ImportResult(created, items.Count - created);
    }

    public async Task<List<TransactionDto>> GetListAsync(Guid userId, Guid accountId, DateOnly? from, DateOnly? to, int page, int size)
    {
        await CheckAccountAsync(userId, accountId);

        if (page < 1 || size < 1 || size > 200)
        {
            throw new ArgumentException("Page must be at least 1 and size must be from 1 to 200");
        }

        var query = db.Transactions.Where(t => t.AccountId == accountId);

        if (from != null)
        {
            query = query.Where(t => t.Date >= from.Value);
        }

        if (to != null)
        {
            query = query.Where(t => t.Date <= to.Value);
        }

        return await query
            .OrderByDescending(t => t.Date)
            .Skip((page - 1) * size)
            .Take(size)
            .Select(t => new TransactionDto(t.Id, t.AccountId, t.Date, t.Amount, t.Description, t.CategoryId))
            .ToListAsync();
    }

    public async Task<TransactionDto> UpdateAsync(Guid userId, Guid id, CreateTransactionRequest request)
    {
        Transaction transaction = await GetOwnAsync(userId, id);

        string description = transaction.Description.Trim();

        transaction.Description = description;
        transaction.Date = request.Date;
        transaction.Amount = request.Amount;
        transaction.Hash = await MakeHashAsync(transaction.AccountId, request.Date, request.Amount, description, id);

        await db.SaveChangesAsync();

        return new TransactionDto(transaction.Id, transaction.AccountId, transaction.Date, transaction.Amount, transaction.Description, transaction.CategoryId);
    }

    public async Task DeleteAsync(Guid userId, Guid id)
    {
        Transaction transaction = await GetOwnAsync(userId, id);

        db.Transactions.Remove(transaction);
        await db.SaveChangesAsync();
    }
}
