namespace Finance.ReportsBudgets.Tests;

public class ReportServiceTests
{
    private readonly Guid _userId = Guid.NewGuid();

    [Fact]
    public async Task ByCategory_FiltersMonthAndSortsByExpense()
    {
        var db = TestHelpers.CreateDb();
        TestHelpers.AddTotal(db, _userId, Guid.NewGuid(), 2026, 10, 100);
        TestHelpers.AddTotal(db, _userId, Guid.NewGuid(), 2026, 10, 300);
        TestHelpers.AddTotal(db, _userId, Guid.NewGuid(), 2026, 11, 999);        // другой месяц
        TestHelpers.AddTotal(db, Guid.NewGuid(), Guid.NewGuid(), 2026, 10, 888); // чужой
        var service = new ReportService(db);

        var result = await service.GetByCategoryAsync(_userId, 2026, 10);

        Assert.Equal(new decimal[] { 300, 100 }, result.Select(r => r.Expense));
    }

    [Fact]
    public async Task Monthly_SumsCategoriesAndCalculatesBalance()
    {
        var db = TestHelpers.CreateDb();
        TestHelpers.AddTotal(db, _userId, Guid.NewGuid(), 2026, 10, 100, 0);
        TestHelpers.AddTotal(db, _userId, Guid.NewGuid(), 2026, 10, 50, 1000);
        TestHelpers.AddTotal(db, _userId, Guid.NewGuid(), 2025, 10, 777);        // другой год
        var service = new ReportService(db);

        var result = await service.GetMonthlyAsync(_userId, 2026);

        var october = Assert.Single(result);
        Assert.Equal(150, october.Expense);
        Assert.Equal(1000, october.Income);
        Assert.Equal(850, october.Balance);
    }

    [Fact]
    public async Task InvalidMonthOrYear_Throws()
    {
        var service = new ReportService(TestHelpers.CreateDb());

        await Assert.ThrowsAsync<ArgumentException>(() => service.GetByCategoryAsync(_userId, 2026, 13));
        await Assert.ThrowsAsync<ArgumentException>(() => service.GetMonthlyAsync(_userId, 1900));
    }
}