using Finance.Contracts;
using Finance.Messaging;

using Microsoft.EntityFrameworkCore;

namespace Finance.ReportsBudgets.Tests;

public class FakeMessageBus : IMessageBus
{
    public List<object> Published { get; } = new();

    public Task PublishAsync<T>(T message, string? correlationId = null)
    {
        Published.Add(message!);
        return Task.CompletedTask;
    }
}

public static class TestHelpers
{
    public static ReportsDbContext CreateDb()
    {
        var options = new DbContextOptionsBuilder<ReportsDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new ReportsDbContext(options);
    }

    public static CategorizedItem Item(decimal amount, Guid categoryId, int month = 10)
    {
        return new CategorizedItem(Guid.NewGuid(), Guid.NewGuid(), new DateOnly(2026, month, 15), amount, categoryId, "Кафе");
    }

    public static MonthlyTotal AddTotal(ReportsDbContext db, Guid userId, Guid categoryId,
        int year, int month, decimal expense, decimal income = 0)
    {
        var total = new MonthlyTotal
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            CategoryId = categoryId,
            CategoryName = "Кафе",
            Year = year,
            Month = month,
            Expense = expense,
            Income = income
        };
        db.MonthlyTotals.Add(total);
        db.SaveChanges();
        return total;
    }

    public static Budget AddBudget(ReportsDbContext db, Guid userId, Guid categoryId, decimal limit)
    {
        var budget = new Budget
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            CategoryId = categoryId,
            CategoryName = "Кафе",
            Limit = limit
        };
        db.Budgets.Add(budget);
        db.SaveChanges();
        return budget;
    }
}