using Finance.Contracts;
using Finance.Messaging;

using Microsoft.EntityFrameworkCore;

namespace Finance.Import;

public class TransactionsCategorizedHandler(ImportDbContext db, IMessageBus bus)
    : IMessageHandler<TransactionsCategorized>
{
    public async Task HandleAsync(TransactionsCategorized message, MessageContext context)
    {
        var batch = await db.ImportBatches
            .FirstOrDefaultAsync(b => b.Id == message.BatchId && b.UserId == message.UserId);
        if (batch == null) return;

        batch.Status = ImportStatus.Completed;
        await db.SaveChangesAsync();

        await bus.PublishAsync(new BatchCompleted(batch.Id, batch.UserId), context.CorrelationId);
    }
}