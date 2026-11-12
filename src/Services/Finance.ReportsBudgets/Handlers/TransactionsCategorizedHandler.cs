using Finance.Contracts;
using Finance.Messaging;

using Microsoft.Extensions.Caching.Distributed;

namespace Finance.ReportsBudgets;

public class TransactionsCategorizedHandler(
    AggregateService aggregates,
    BudgetService budgetService,
    IMessageBus bus,
    IDistributedCache cache,
    ILogger<TransactionsCategorizedHandler> logger) : IMessageHandler<TransactionsCategorized>
{
    public async Task HandleAsync(TransactionsCategorized message, MessageContext context)
    {
        var changes = await aggregates.ApplyAsync(message.UserId, message.Items);

        await InvalidateCacheAsync(message);

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

    private async Task InvalidateCacheAsync(TransactionsCategorized message)
    {
        var months = message.Items
            .Select(i => (i.Date.Year, i.Date.Month))
            .Distinct();

        foreach (var (year, month) in months)
        {
            try
            {
                await cache.RemoveAsync(ReportCacheKeys.ByCategory(message.UserId, year, month));
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Redis remove failed");
            }
        }
    }
}