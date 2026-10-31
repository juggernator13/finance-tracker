using Microsoft.EntityFrameworkCore;

namespace Finance.AccountsTransactions.Tests;

public class TransactionServiceTests
{
    private static async Task<(TransactionService Service, AccountsDbContext Db, Guid UserId, Guid AccountId)> Create()
    {
        var db = TestHelpers.CreateDb();
        var userId = Guid.NewGuid();
        var account = await new AccountService(db).CreateAsync(userId, "Card", "RUB");
        return (new TransactionService(db), db, userId, account.Id);
    }

    [Fact]
    public async Task Add_saves_transaction_with_hash()
    {
        var (service, db, userId, accountId) = await Create();

        var dto = await service.AddAsync(userId, accountId, TestHelpers.Request());

        var saved = await db.Transactions.AsNoTracking().SingleAsync();
        Assert.Equal(dto.Id, saved.Id);
        Assert.Equal(-150.50m, saved.Amount);
        Assert.Equal("Coffee", saved.Description);
        Assert.False(string.IsNullOrEmpty(saved.Hash));
    }

    [Fact]
    public async Task Two_identical_transactions_are_saved_with_different_hashes()
    {
        var (service, db, userId, accountId) = await Create();

        await service.AddAsync(userId, accountId, TestHelpers.Request());
        await service.AddAsync(userId, accountId, TestHelpers.Request());

        var hashes = await db.Transactions.AsNoTracking().Select(t => t.Hash).ToListAsync();
        Assert.Equal(2, hashes.Count);
        Assert.NotEqual(hashes[0], hashes[1]);
    }

    [Fact]
    public async Task Add_to_other_users_account_throws()
    {
        var (service, _, _, accountId) = await Create();

        await Assert.ThrowsAsync<KeyNotFoundException>(() =>
            service.AddAsync(Guid.NewGuid(), accountId, TestHelpers.Request()));
    }

    [Fact]
    public async Task Add_with_empty_description_throws()
    {
        var (service, _, userId, accountId) = await Create();

        await Assert.ThrowsAsync<ArgumentException>(() =>
            service.AddAsync(userId, accountId, TestHelpers.Request(description: "  ")));
    }

    [Fact]
    public async Task GetList_filters_by_date_range()
    {
        var (service, _, userId, accountId) = await Create();
        await service.AddAsync(userId, accountId, TestHelpers.Request("2026-10-01", -1, "First"));
        await service.AddAsync(userId, accountId, TestHelpers.Request("2026-10-15", -2, "Second"));
        await service.AddAsync(userId, accountId, TestHelpers.Request("2026-10-30", -3, "Third"));

        var list = await service.GetListAsync(userId, accountId, new DateOnly(2026, 10, 10), new DateOnly(2026, 10, 20), 1, 50);

        Assert.Single(list);
        Assert.Equal("Second", list[0].Description);
    }

    [Fact]
    public async Task GetList_returns_newest_first()
    {
        var (service, _, userId, accountId) = await Create();
        await service.AddAsync(userId, accountId, TestHelpers.Request("2026-10-01", -1, "Old"));
        await service.AddAsync(userId, accountId, TestHelpers.Request("2026-10-30", -2, "New"));

        var list = await service.GetListAsync(userId, accountId, null, null, 1, 50);

        Assert.Equal("New", list[0].Description);
        Assert.Equal("Old", list[1].Description);
    }

    [Fact]
    public async Task GetList_pages_results()
    {
        var (service, _, userId, accountId) = await Create();
        for (int i = 1; i <= 5; i++)
        {
            await service.AddAsync(userId, accountId, TestHelpers.Request($"2026-10-{i:00}", -i, $"Item {i}"));
        }

        var page1 = await service.GetListAsync(userId, accountId, null, null, 1, 2);
        var page3 = await service.GetListAsync(userId, accountId, null, null, 3, 2);

        Assert.Equal(2, page1.Count);
        Assert.Single(page3);
    }

    [Fact]
    public async Task GetList_with_invalid_page_or_size_throws()
    {
        var (service, _, userId, accountId) = await Create();

        await Assert.ThrowsAsync<ArgumentException>(() => service.GetListAsync(userId, accountId, null, null, 0, 10));
        await Assert.ThrowsAsync<ArgumentException>(() => service.GetListAsync(userId, accountId, null, null, 1, 0));
        await Assert.ThrowsAsync<ArgumentException>(() => service.GetListAsync(userId, accountId, null, null, 1, 500));
    }

    [Fact]
    public async Task GetList_for_other_users_account_throws()
    {
        var (service, _, _, accountId) = await Create();

        await Assert.ThrowsAsync<KeyNotFoundException>(() =>
            service.GetListAsync(Guid.NewGuid(), accountId, null, null, 1, 10));
    }

    [Fact]
    public async Task Update_changes_amount_and_recalculates_hash()
    {
        var (service, db, userId, accountId) = await Create();
        var added = await service.AddAsync(userId, accountId, TestHelpers.Request());
        var oldHash = (await db.Transactions.AsNoTracking().SingleAsync()).Hash;

        await service.UpdateAsync(userId, added.Id, TestHelpers.Request(amount: -200m));

        var saved = await db.Transactions.AsNoTracking().SingleAsync();
        Assert.Equal(-200m, saved.Amount);
        Assert.NotEqual(oldHash, saved.Hash);
    }

    [Fact]
    public async Task Update_other_users_transaction_throws()
    {
        var (service, _, userId, accountId) = await Create();
        var added = await service.AddAsync(userId, accountId, TestHelpers.Request());

        await Assert.ThrowsAsync<KeyNotFoundException>(() =>
            service.UpdateAsync(Guid.NewGuid(), added.Id, TestHelpers.Request(amount: -1m)));
    }

    [Fact]
    public async Task Delete_removes_transaction()
    {
        var (service, db, userId, accountId) = await Create();
        var added = await service.AddAsync(userId, accountId, TestHelpers.Request());

        await service.DeleteAsync(userId, added.Id);

        Assert.Equal(0, await db.Transactions.CountAsync());
    }

    [Fact]
    public async Task Delete_other_users_transaction_throws_and_keeps_it()
    {
        var (service, db, userId, accountId) = await Create();
        var added = await service.AddAsync(userId, accountId, TestHelpers.Request());

        await Assert.ThrowsAsync<KeyNotFoundException>(() => service.DeleteAsync(Guid.NewGuid(), added.Id));

        Assert.Equal(1, await db.Transactions.CountAsync());
    }
}