using Finance.Contracts;
using Finance.Messaging;

using Microsoft.EntityFrameworkCore;

namespace Finance.Import.Tests;

public class HandlerTests
{
    private static ImportDbContext NewDb() =>
        new(new DbContextOptionsBuilder<ImportDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString()).Options);

    private static async Task<ImportBatch> AddBatch(ImportDbContext db, string status)
    {
        var batch = new ImportBatch
        {
            Id = Guid.NewGuid(),
            UserId = Guid.NewGuid(),
            AccountId = Guid.NewGuid(),
            Format = "bank1",
            Status = status,
            TotalRows = 4,
            CreatedAt = DateTime.UtcNow
        };
        db.ImportBatches.Add(batch);
        await db.SaveChangesAsync();
        return batch;
    }

    private static readonly MessageContext Ctx = new(Guid.NewGuid().ToString(), Guid.NewGuid().ToString());

    [Fact]
    public async Task Categorized_SetsCompleted_AndPublishesBatchCompleted()
    {
        var db = NewDb();
        var bus = new FakeMessageBus();
        var batch = await AddBatch(db, ImportStatus.Categorizing);

        await new TransactionsCategorizedHandler(db, bus).HandleAsync(
            new TransactionsCategorized(batch.Id, batch.UserId, new List<CategorizedItem>()), Ctx);

        Assert.Equal(ImportStatus.Completed, (await db.ImportBatches.FindAsync(batch.Id))!.Status);
        Assert.IsType<BatchCompleted>(Assert.Single(bus.Published));
    }

    [Fact]
    public async Task CategorizationFailed_SetsFailedWithReason()
    {
        var db = NewDb();
        var batch = await AddBatch(db, ImportStatus.Categorizing);

        await new CategorizationFailedHandler(db).HandleAsync(
            new CategorizationFailed(batch.Id, batch.UserId, "boom"), Ctx);

        var saved = (await db.ImportBatches.FindAsync(batch.Id))!;
        Assert.Equal(ImportStatus.Failed, saved.Status);
        Assert.Equal("boom", saved.Error);
    }

    [Fact]
    public async Task UnknownBatch_DoesNothing()
    {
        var db = NewDb();
        var bus = new FakeMessageBus();

        await new TransactionsCategorizedHandler(db, bus).HandleAsync(
            new TransactionsCategorized(Guid.NewGuid(), Guid.NewGuid(), new List<CategorizedItem>()), Ctx);

        Assert.Empty(bus.Published);
    }
}