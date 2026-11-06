using Finance.Contracts;
using Finance.Messaging;

using Microsoft.EntityFrameworkCore;

namespace Finance.Categorization;

public class CategorizeBatchHandler(CategorizationDbContext db, IMessageBus bus) : IMessageHandler<CategorizeBatch>
{
    public async Task HandleAsync(CategorizeBatch message, MessageContext context)
    {
        List<CategorizedItem> items;

        try
        {
            var rules = await db.Rules
                .AsNoTracking()
                .Where(r => r.UserId == null || r.UserId == message.UserId)
                .ToListAsync();

            var categories = await db.Categories
                .AsNoTracking()
                .Where(c => c.UserId == null || c.UserId == message.UserId)
                .ToDictionaryAsync(c => c.Id, c => c.Name);

            items = new List<CategorizedItem>();

            foreach (var item in message.Items)
            {
                var rule = RuleMatcher.Match(item.Description, rules);
                Guid categoryId = rule?.CategoryId ?? DbSeeder.OtherId;

                items.Add(new CategorizedItem(
                    item.Id, item.AccountId, item.Date, item.Amount,
                    categoryId, categories[categoryId]));
            }
        }
        catch (Exception ex)
        {
            await bus.PublishAsync(
                new CategorizationFailed(message.BatchId, message.UserId, ex.Message),
                context.CorrelationId);
            return;
        }

        await bus.PublishAsync(
            new TransactionsCategorized(message.BatchId, message.UserId, items),
            context.CorrelationId);
    }
}