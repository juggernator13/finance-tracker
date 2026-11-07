using Finance.Contracts;
using Finance.Messaging;

using Microsoft.EntityFrameworkCore;

namespace Finance.AccountsTransactions.Tests;

public class CategorizedHandlerTests
{
    private static readonly MessageContext Ctx = new(Guid.NewGuid().ToString(), Guid.NewGuid().ToString());

    private static AccountsDbContext NewDb() =>
        new(new DbContextOptionsBuilder<AccountsDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString()).Options);

    private static async Task<(Guid userId, Transaction tx)> AddTx(AccountsDbContext db)
    {
        var userId = Guid.NewGuid();
        var account = new Account { Id = Guid.NewGuid(), UserId = userId, Name = "Main" };
        var tx = new Transaction
        {
            Id = Guid.NewGuid(),
            AccountId = account.Id,
            Date = new DateOnly(2026, 10, 1),
            Amount = -150m,
            Description = "Кофе",
            Hash = Guid.NewGuid().ToString()
        };
        db.Accounts.Add(account);
        db.Transactions.Add(tx);
        await db.SaveChangesAsync();
        return (userId, tx);
    }

    [Fact]
    public async Task SetsCategory_ForOwnTransactions()
    {
        var db = NewDb();
        var (userId, tx) = await AddTx(db);
        var categoryId = Guid.NewGuid();

        await new TransactionsCategorizedHandler(db).HandleAsync(
            new TransactionsCategorized(Guid.NewGuid(), userId,
                new List<CategorizedItem> { Item(tx, categoryId) }), Ctx);

        Assert.Equal(categoryId, (await db.Transactions.FindAsync(tx.Id))!.CategoryId);
    }

    private static CategorizedItem Item(Transaction tx, Guid categoryId) =>
        new(tx.Id, tx.AccountId, tx.Date, tx.Amount, categoryId, "Test");

    [Fact]
    public async Task IgnoresTransactions_OfAnotherUser()
    {
        var db = NewDb();
        var (_, tx) = await AddTx(db);

        await new TransactionsCategorizedHandler(db).HandleAsync(
            new TransactionsCategorized(Guid.NewGuid(), Guid.NewGuid(),
                new List<CategorizedItem> { Item(tx, Guid.NewGuid()) }), Ctx);

        Assert.Null((await db.Transactions.FindAsync(tx.Id))!.CategoryId);
    }
}