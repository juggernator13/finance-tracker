using Finance.Contracts;
using Finance.Messaging;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

namespace Finance.ReportsBudgets.Tests;

public class ReportCacheTests
{
    private static readonly Guid UserId = Guid.NewGuid();
    private static readonly Guid FoodId = Guid.NewGuid();
    private static readonly MessageContext Ctx = new(Guid.NewGuid().ToString(), Guid.NewGuid().ToString());

    private static ReportsDbContext NewDb() =>
        new(new DbContextOptionsBuilder<ReportsDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString()).Options);

    private static IDistributedCache NewCache() =>
        new MemoryDistributedCache(Options.Create(new MemoryDistributedCacheOptions()));

    private static async Task AddTotal(ReportsDbContext db, int year, int month, decimal expense)
    {
        db.MonthlyTotals.Add(new MonthlyTotal
        {
            Id = Guid.NewGuid(),
            UserId = UserId,
            Year = year,
            Month = month,
            CategoryId = FoodId,
            CategoryName = "Продукты",
            Expense = expense,
            Income = 0
        });
        await db.SaveChangesAsync();
    }

    private static ReportService NewReportService(ReportsDbContext db, IDistributedCache cache) =>
        new(db, cache, NullLogger<ReportService>.Instance);

    private static TransactionsCategorizedHandler NewHandler(ReportsDbContext db, IDistributedCache cache) =>
        new(new AggregateService(db), new BudgetService(db), new FakeMessageBus(), cache,
            NullLogger<TransactionsCategorizedHandler>.Instance);

    private static TransactionsCategorized Message(DateOnly date, decimal amount) =>
        new(Guid.NewGuid(), UserId, new List<CategorizedItem>
        {
            new(Guid.NewGuid(), Guid.NewGuid(), date, amount, FoodId, "Продукты")
        });

    [Fact]
    public async Task SecondRequest_ReturnsCachedValue()
    {
        var db = NewDb();
        await AddTotal(db, 2026, 10, 100m);
        var service = NewReportService(db, NewCache());

        var first = await service.GetByCategoryAsync(UserId, 2026, 10);

        db.MonthlyTotals.First().Expense = 999m;
        await db.SaveChangesAsync();
        var second = await service.GetByCategoryAsync(UserId, 2026, 10);

        Assert.Equal(100m, first[0].Expense);
        Assert.Equal(100m, second[0].Expense);
    }

    [Fact]
    public async Task Handler_InvalidatesCache_ForAffectedMonth()
    {
        var db = NewDb();
        await AddTotal(db, 2026, 10, 100m);
        var cache = NewCache();
        var service = NewReportService(db, cache);
        var before = (await service.GetByCategoryAsync(UserId, 2026, 10))[0].Expense;

        await NewHandler(db, cache).HandleAsync(Message(new DateOnly(2026, 10, 5), -50m), Ctx);

        var after = (await service.GetByCategoryAsync(UserId, 2026, 10))[0].Expense;
        Assert.True(after > before);
    }

    [Fact]
    public async Task Handler_DoesNotInvalidate_OtherMonth()
    {
        var db = NewDb();
        await AddTotal(db, 2026, 11, 70m);
        var cache = NewCache();
        var service = NewReportService(db, cache);
        await service.GetByCategoryAsync(UserId, 2026, 11);

        await NewHandler(db, cache).HandleAsync(Message(new DateOnly(2026, 10, 5), -50m), Ctx);

        db.MonthlyTotals.First(t => t.Month == 11).Expense = 999m;
        await db.SaveChangesAsync();
        var november = await service.GetByCategoryAsync(UserId, 2026, 11);

        Assert.Equal(70m, november[0].Expense);
    }

    [Fact]
    public async Task BrokenCache_StillReturnsReportFromDatabase()
    {
        var db = NewDb();
        await AddTotal(db, 2026, 10, 100m);
        var service = NewReportService(db, new BrokenCache());

        var report = await service.GetByCategoryAsync(UserId, 2026, 10);

        Assert.Equal(100m, report[0].Expense);
    }
}

public class BrokenCache : IDistributedCache
{
    private static Exception Fail() => new InvalidOperationException("Redis is down");

    public byte[]? Get(string key) => throw Fail();
    public Task<byte[]?> GetAsync(string key, CancellationToken token = default) => throw Fail();
    public void Refresh(string key) => throw Fail();
    public Task RefreshAsync(string key, CancellationToken token = default) => throw Fail();
    public void Remove(string key) => throw Fail();
    public Task RemoveAsync(string key, CancellationToken token = default) => throw Fail();
    public void Set(string key, byte[] value, DistributedCacheEntryOptions options) => throw Fail();
    public Task SetAsync(string key, byte[] value, DistributedCacheEntryOptions options, CancellationToken token = default) => throw Fail();
}