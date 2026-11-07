using Microsoft.EntityFrameworkCore;

namespace Finance.AccountsTransactions.Tests;

public class BatchItemsTests
{
    private static AccountsDbContext NewDb() =>
        new(new DbContextOptionsBuilder<AccountsDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString()).Options);

    private static async Task<(TransactionService svc, AccountsDbContext db, Guid userId, Guid accountId)> Setup()
    {
        var db = NewDb();
        var userId = Guid.NewGuid();
        var account = new Account { Id = Guid.NewGuid(), UserId = userId, Name = "Main" };
        db.Accounts.Add(account);
        await db.SaveChangesAsync();
        return (new TransactionService(db), db, userId, account.Id);
    }

    private static List<ImportItem> Items() => new()
    {
        new ImportItem(new DateOnly(2026, 10, 1), -150m, "Кофе", 0),
        new ImportItem(new DateOnly(2026, 10, 1), -150m, "Кофе", 1),
        new ImportItem(new DateOnly(2026, 10, 2), 85000m, "Зарплата", 0)
    };

    [Fact]
    public async Task AddBatch_ReturnsCreatedItems_WithRealIds()
    {
        var (svc, db, userId, accountId) = await Setup();

        var result = await svc.AddBatchAsync(userId, accountId, Guid.NewGuid(), Items());

        Assert.Equal(3, result.Created);
        Assert.Equal(3, result.Items.Count);
        var ids = await db.Transactions.Select(t => t.Id).ToListAsync();
        Assert.All(result.Items, i => Assert.Contains(i.Id, ids));
        Assert.All(result.Items, i => Assert.Equal(accountId, i.AccountId));
    }

    [Fact]
    public async Task AddBatch_Repeat_ReturnsNoItems()
    {
        var (svc, db, userId, accountId) = await Setup();
        await svc.AddBatchAsync(userId, accountId, Guid.NewGuid(), Items());

        var result = await svc.AddBatchAsync(userId, accountId, Guid.NewGuid(), Items());

        Assert.Equal(0, result.Created);
        Assert.Equal(3, result.Skipped);
        Assert.Empty(result.Items);
        Assert.Equal(3, await db.Transactions.CountAsync());
    }

    [Fact]
    public async Task AddBatch_NewTransactions_HaveNoCategory()
    {
        var (svc, db, userId, accountId) = await Setup();

        await svc.AddBatchAsync(userId, accountId, Guid.NewGuid(), Items());

        Assert.All(await db.Transactions.ToListAsync(), t => Assert.Null(t.CategoryId));
    }
}