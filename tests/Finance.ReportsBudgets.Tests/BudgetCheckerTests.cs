namespace Finance.ReportsBudgets.Tests;

public class BudgetCheckerTests
{
    private readonly Guid _cat = Guid.NewGuid();

    [Theory]
    [InlineData(100, 250, 1)]
    [InlineData(250, 300, 0)]
    [InlineData(100, 150, 0)]
    [InlineData(100, 200, 0)]
    public void FindExceeded_OnlyWhenLimitIsCrossed(int before, int after, int expected)
    {
        var budgets = new[] { new Budget { CategoryId = _cat, Limit = 200 } };
        var changes = new[] { new AggregateChange(_cat, "Кафе", 2026, 10, before, after) };

        Assert.Equal(expected, BudgetChecker.FindExceeded(budgets, changes).Count);
    }

    [Fact]
    public void FindExceeded_NoBudgetForCategory_ReturnsEmpty()
    {
        var changes = new[] { new AggregateChange(_cat, "Кафе", 2026, 10, 0, 1000) };

        Assert.Empty(BudgetChecker.FindExceeded(new List<Budget>(), changes));
    }
}