using Finance.Contracts;
using Finance.Messaging;

namespace Finance.ReportsBudgets;

public class TransactionsCategorizedHandler(
    AggregateService aggregates,
    BudgetService budgetService,
    IMessageBus bus) : IMessageHandler<TransactionsCategorized>
{
    public async Task HandleAsync(TransactionsCategorized message, MessageContext context)
    {
        var changes = await aggregates.ApplyAsync(message.UserId, message.Items);

        var budgets = await budgetService.ListEntitiesAsync(message.UserId);
        var exceeded = BudgetChecker.FindExceeded(budgets, changes);

        foreach (var (budget, change) in exceeded)
        {
            await bus.PublishAsync(
                new BudgetExceeded(
                    message.UserId,
                    budget.CategoryId,
                    budget.CategoryName,
                    budget.Limit,
                    change.After),
                context.CorrelationId);
        }
    }
}