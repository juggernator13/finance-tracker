using Finance.Contracts;
using Finance.Messaging;

using Microsoft.EntityFrameworkCore;

namespace Finance.Import;

public class CategorizationFailedHandler(ImportDbContext db)
    : IMessageHandler<CategorizationFailed>
{
    public async Task HandleAsync(CategorizationFailed message, MessageContext context)
    {
        var batch = await db.ImportBatches
            .FirstOrDefaultAsync(b => b.Id == message.BatchId && b.UserId == message.UserId);
        if (batch == null) return;

        batch.Status = ImportStatus.Failed;
        batch.Error = message.Reason;
        await db.SaveChangesAsync();
    }
}