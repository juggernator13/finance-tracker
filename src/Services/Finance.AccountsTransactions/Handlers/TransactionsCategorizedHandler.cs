using Finance.Contracts;
using Finance.Messaging;

using Microsoft.EntityFrameworkCore;

namespace Finance.AccountsTransactions;

public class TransactionsCategorizedHandler(AccountsDbContext db)
    : IMessageHandler<TransactionsCategorized>
{
    public async Task HandleAsync(TransactionsCategorized message, MessageContext context)
    {
        var map = message.Items.ToDictionary(i => i.TransactionId, i => i.CategoryId);
        var ids = map.Keys.ToList();

        var accountIds = db.Accounts
            .Where(a => a.UserId == message.UserId)
            .Select(a => a.Id);

        var rows = await db.Transactions
            .Where(t => ids.Contains(t.Id) && accountIds.Contains(t.AccountId))
            .ToListAsync();

        foreach (var t in rows)
            t.CategoryId = map[t.Id];

        await db.SaveChangesAsync();
    }
}