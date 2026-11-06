using Microsoft.EntityFrameworkCore;

namespace Finance.ReportsBudgets.Tests;

public class BudgetServiceTests
{
    private readonly Guid _userId = Guid.NewGuid();
    private readonly Guid _cat = Guid.NewGuid();

    [Fact]
    public async Task GetAll_ShowsSpentAndPercentForCurrentMonth()
    {
        var db = TestHelpers.CreateDb();
        var now = DateTime.UtcNow;
        TestHelpers.AddTotal(db, _userId, _cat, now.Year, now.Month, 150);
        TestHelpers.AddTotal(db, _userId, _cat, now.Year - 1, now.Month, 999);   // другой год не считается
        var service = new BudgetService(db);

        await service.CreateAsync(_userId, _cat, "Кафе", 200);
        var budget = (await service.GetAllAsync(_userId)).Single();

        Assert.Equal(150, budget.Spent);
        Assert.Equal(75, budget.Percent);
        Assert.False(budget.IsExceeded);
    }

    [Fact]
    public async Task Create_InvalidLimitOrDuplicate_Throws()
    {
        var service = new BudgetService(TestHelpers.CreateDb());
        await service.CreateAsync(_userId, _cat, "Кафе", 200);

        await Assert.ThrowsAsync<ArgumentException>(() => service.CreateAsync(_userId, Guid.NewGuid(), "Еда", 0));
        await Assert.ThrowsAsync<ArgumentException>(() => service.CreateAsync(_userId, _cat, "Кафе", 300));
    }

    [Fact]
    public async Task GetAll_ReturnsOnlyOwnBudgets()
    {
        var db = TestHelpers.CreateDb();
        TestHelpers.AddBudget(db, _userId, _cat, 200);
        TestHelpers.AddBudget(db, Guid.NewGuid(), _cat, 500);
        var service = new BudgetService(db);

        var list = await service.GetAllAsync(_userId);

        Assert.Single(list);
        Assert.Equal(200, list[0].Limit);
    }

    [Fact]
    public async Task UpdateAndDelete_OtherUsersBudget_Throw_OwnDeleteWorks()
    {
        var db = TestHelpers.CreateDb();
        var foreign = TestHelpers.AddBudget(db, Guid.NewGuid(), _cat, 200);
        var own = TestHelpers.AddBudget(db, _userId, Guid.NewGuid(), 200);
        var service = new BudgetService(db);

        await Assert.ThrowsAsync<KeyNotFoundException>(() => service.UpdateAsync(_userId, foreign.Id, 300));
        await Assert.ThrowsAsync<KeyNotFoundException>(() => service.DeleteAsync(_userId, foreign.Id));

        await service.DeleteAsync(_userId, own.Id);
        Assert.Equal(1, await db.Budgets.CountAsync());
    }
}