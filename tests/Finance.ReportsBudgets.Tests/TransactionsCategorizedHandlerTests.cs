using Finance.Contracts;
using Finance.Messaging;

using Microsoft.EntityFrameworkCore;

namespace Finance.ReportsBudgets.Tests;

public class TransactionsCategorizedHandlerTests
{
    private readonly Guid _userId = Guid.NewGuid();
    private readonly Guid _cat = Guid.NewGuid();

    private static TransactionsCategorizedHandler CreateHandler(ReportsDbContext db, FakeMessageBus bus)
    {
        return new TransactionsCategorizedHandler(new AggregateService(db), new BudgetService(db), bus);
    }

    private TransactionsCategorized Message(decimal amount)
    {
        return new TransactionsCategorized(Guid.NewGuid(), _userId, new List<CategorizedItem> { TestHelpers.Item(amount, _cat) });
    }

    [Fact]
    public async Task Handle_LimitCrossed_PublishesBudgetExceeded()
    {
        var db = TestHelpers.CreateDb();
        TestHelpers.AddBudget(db, _userId, _cat, 200);
        var bus = new FakeMessageBus();

        await CreateHandler(db, bus).HandleAsync(Message(-250), new MessageContext("1", null));

        var sent = Assert.IsType<BudgetExceeded>(Assert.Single(bus.Published));
        Assert.Equal(_userId, sent.UserId);
        Assert.Equal(_cat, sent.CategoryId);
        Assert.Equal(200, sent.Limit);
        Assert.Equal(250, sent.Spent);
    }

    [Fact]
    public async Task Handle_BelowLimit_PublishesNothing_ButSavesTotals()
    {
        var db = TestHelpers.CreateDb();
        TestHelpers.AddBudget(db, _userId, _cat, 200);
        var bus = new FakeMessageBus();

        await CreateHandler(db, bus).HandleAsync(Message(-100), new MessageContext("1", null));

        Assert.Empty(bus.Published);
        Assert.Equal(100, (await db.MonthlyTotals.SingleAsync()).Expense);
    }
}