using Microsoft.EntityFrameworkCore;

namespace Finance.ReportsBudgets.Tests;

public class AggregateServiceTests
{
    private readonly Guid _userId = Guid.NewGuid();
    private readonly Guid _cat = Guid.NewGuid();

    [Fact]
    public async Task Apply_SumsExpenseAndIncomeSeparately_InOneRow()
    {
        var db = TestHelpers.CreateDb();
        var service = new AggregateService(db);

        var changes = await service.ApplyAsync(_userId, new[]
        {
            TestHelpers.Item(-150, _cat), TestHelpers.Item(-100, _cat), TestHelpers.Item(1000, _cat)
        });

        var row = await db.MonthlyTotals.SingleAsync();
        Assert.Equal(250, row.Expense);
        Assert.Equal(1000, row.Income);
        Assert.Equal(0, changes.Single().Before);
        Assert.Equal(250, changes.Single().After);
    }

    [Fact]
    public async Task Apply_SecondBatch_AddsToExistingRow()
    {
        var db = TestHelpers.CreateDb();
        var service = new AggregateService(db);

        await service.ApplyAsync(_userId, new[] { TestHelpers.Item(-100, _cat) });
        var changes = await service.ApplyAsync(_userId, new[] { TestHelpers.Item(-150, _cat) });

        Assert.Equal(250, (await db.MonthlyTotals.SingleAsync()).Expense);
        Assert.Equal(100, changes.Single().Before);
        Assert.Equal(250, changes.Single().After);
    }

    [Fact]
    public async Task Apply_DifferentMonths_GiveSeparateRows()
    {
        var db = TestHelpers.CreateDb();
        var service = new AggregateService(db);

        await service.ApplyAsync(_userId, new[] { TestHelpers.Item(-100, _cat, 10), TestHelpers.Item(-50, _cat, 11) });

        Assert.Equal(2, await db.MonthlyTotals.CountAsync());
    }

    [Fact]
    public async Task Apply_DifferentUsers_AreNotMixed()
    {
        var db = TestHelpers.CreateDb();
        var service = new AggregateService(db);

        await service.ApplyAsync(_userId, new[] { TestHelpers.Item(-100, _cat) });
        await service.ApplyAsync(Guid.NewGuid(), new[] { TestHelpers.Item(-100, _cat) });

        Assert.Equal(2, await db.MonthlyTotals.CountAsync());
    }
}