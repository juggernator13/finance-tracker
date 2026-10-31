using Microsoft.EntityFrameworkCore;

namespace Finance.AccountsTransactions.Tests;

public class AccountServiceTests
{
    [Fact]
    public async Task Create_saves_account_for_user()
    {
        var db = TestHelpers.CreateDb();
        var service = new AccountService(db);
        var userId = Guid.NewGuid();

        var dto = await service.CreateAsync(userId, "Card", "rub");

        var saved = await db.Accounts.AsNoTracking().SingleAsync();
        Assert.Equal(dto.Id, saved.Id);
        Assert.Equal(userId, saved.UserId);
        Assert.Equal("Card", saved.Name);
    }

    [Fact]
    public async Task Create_with_empty_currency_uses_rub()
    {
        var service = new AccountService(TestHelpers.CreateDb());

        var dto = await service.CreateAsync(Guid.NewGuid(), "Card", "");

        Assert.Equal("RUB", dto.Currency);
    }

    [Fact]
    public async Task Create_with_empty_name_throws()
    {
        var service = new AccountService(TestHelpers.CreateDb());

        await Assert.ThrowsAsync<ArgumentException>(() => service.CreateAsync(Guid.NewGuid(), "   ", "RUB"));
    }

    [Fact]
    public async Task GetAll_returns_only_own_accounts_sorted_by_name()
    {
        var service = new AccountService(TestHelpers.CreateDb());
        var me = Guid.NewGuid();
        await service.CreateAsync(me, "B account", "RUB");
        await service.CreateAsync(me, "A account", "RUB");
        await service.CreateAsync(Guid.NewGuid(), "Someone else", "RUB");

        var list = await service.GetAllAsync(me);

        Assert.Equal(2, list.Count);
        Assert.Equal("A account", list[0].Name);
        Assert.Equal("B account", list[1].Name);
    }

    [Fact]
    public async Task Get_other_users_account_throws()
    {
        var service = new AccountService(TestHelpers.CreateDb());
        var account = await service.CreateAsync(Guid.NewGuid(), "Card", "RUB");

        await Assert.ThrowsAsync<KeyNotFoundException>(() => service.GetAsync(Guid.NewGuid(), account.Id));
    }

    [Fact]
    public async Task Update_changes_name_in_database()
    {
        var db = TestHelpers.CreateDb();
        var service = new AccountService(db);
        var userId = Guid.NewGuid();
        var account = await service.CreateAsync(userId, "Old", "RUB");

        await service.UpdateAsync(userId, account.Id, "New");

        var saved = await db.Accounts.AsNoTracking().SingleAsync();
        Assert.Equal("New", saved.Name);
    }

    [Fact]
    public async Task Update_with_empty_name_throws()
    {
        var service = new AccountService(TestHelpers.CreateDb());
        var userId = Guid.NewGuid();
        var account = await service.CreateAsync(userId, "Card", "RUB");

        await Assert.ThrowsAsync<ArgumentException>(() => service.UpdateAsync(userId, account.Id, ""));
    }

    [Fact]
    public async Task Update_other_users_account_throws()
    {
        var service = new AccountService(TestHelpers.CreateDb());
        var account = await service.CreateAsync(Guid.NewGuid(), "Card", "RUB");

        await Assert.ThrowsAsync<KeyNotFoundException>(() => service.UpdateAsync(Guid.NewGuid(), account.Id, "Hacked"));
    }

    [Fact]
    public async Task Delete_removes_account_and_its_transactions()
    {
        var db = TestHelpers.CreateDb();
        var accounts = new AccountService(db);
        var transactions = new TransactionService(db);
        var userId = Guid.NewGuid();
        var account = await accounts.CreateAsync(userId, "Card", "RUB");
        await transactions.AddAsync(userId, account.Id, TestHelpers.Request());
        await transactions.AddAsync(userId, account.Id, TestHelpers.Request(description: "Tea"));

        await accounts.DeleteAsync(userId, account.Id);

        Assert.Equal(0, await db.Accounts.CountAsync());
        Assert.Equal(0, await db.Transactions.CountAsync());
    }

    [Fact]
    public async Task Delete_other_users_account_throws_and_keeps_it()
    {
        var db = TestHelpers.CreateDb();
        var service = new AccountService(db);
        var account = await service.CreateAsync(Guid.NewGuid(), "Card", "RUB");

        await Assert.ThrowsAsync<KeyNotFoundException>(() => service.DeleteAsync(Guid.NewGuid(), account.Id));

        Assert.Equal(1, await db.Accounts.CountAsync());
    }

    [Fact]
    public async Task IsOwner_is_true_for_owner_and_false_for_others()
    {
        var service = new AccountService(TestHelpers.CreateDb());
        var owner = Guid.NewGuid();
        var account = await service.CreateAsync(owner, "Card", "RUB");

        Assert.True(await service.IsOwnerAsync(owner, account.Id));
        Assert.False(await service.IsOwnerAsync(Guid.NewGuid(), account.Id));
        Assert.False(await service.IsOwnerAsync(owner, Guid.NewGuid()));
    }
}