using Finance.Contracts;

using Microsoft.EntityFrameworkCore;

namespace Finance.Import.Tests;

public class ImportServiceTests
{
    private static readonly Guid UserId = Guid.NewGuid();
    private static readonly Guid AccountId = Guid.NewGuid();

    private static (ImportService Service, ImportDbContext Db, FakeAccountsClient Accounts, FakeMessageBus Bus) Create()
    {
        var db = TestHelpers.CreateDb();
        var accounts = new FakeAccountsClient();
        var bus = new FakeMessageBus();
        var factory = new StatementParserFactory(new IStatementParser[] { new Bank1CsvParser(), new Bank2CsvParser() });
        return (new ImportService(db, factory, accounts, bus), db, accounts, bus);
    }

    [Fact]
    public async Task Start_Success_SetsCategorizing_AndPublishesBatch()
    {
        var (service, db, _, bus) = Create();

        var dto = await service.StartAsync(UserId, AccountId, "bank1", TestHelpers.Bank1Csv);

        Assert.Equal(ImportStatus.Categorizing, dto.Status);
        Assert.Equal(3, dto.TotalRows);
        Assert.Equal(1, await db.ImportBatches.AsNoTracking().CountAsync());
        var message = Assert.IsType<CategorizeBatch>(Assert.Single(bus.Published));
        Assert.Equal(dto.Id, message.BatchId);
        Assert.Equal(UserId, message.UserId);
        Assert.Equal(3, message.Items.Count);
    }

    [Fact]
    public async Task Start_SendsOrdinalsToAccounts()
    {
        var (service, _, accounts, _) = Create();

        await service.StartAsync(UserId, AccountId, "bank1", TestHelpers.Bank1Csv);

        var coffee = accounts.ReceivedRows!.Where(r => r.Description == "Кофе").ToList();
        Assert.Equal(new[] { 0, 1 }, coffee.Select(r => r.Ordinal).ToArray());
    }

    [Fact]
    public async Task Start_NoCreatedItems_SetsCompleted_AndPublishesNothing()
    {
        var (service, _, accounts, bus) = Create();
        accounts.Result = new ImportResult(0, 3, new List<TransactionItem>());

        var dto = await service.StartAsync(UserId, AccountId, "bank1", TestHelpers.Bank1Csv);

        Assert.Equal(ImportStatus.Completed, dto.Status);
        Assert.Empty(bus.Published);
    }

    [Fact]
    public async Task Start_BrokerDown_SetsFailed()
    {
        var (service, _, _, bus) = Create();
        bus.Error = new Exception("down");

        var dto = await service.StartAsync(UserId, AccountId, "bank1", TestHelpers.Bank1Csv);

        Assert.Equal(ImportStatus.Failed, dto.Status);
        Assert.False(string.IsNullOrEmpty(dto.Error));
    }

    [Fact]
    public async Task Start_AccountsDown_SetsFailed_AndPublishesNothing()
    {
        var (service, _, accounts, bus) = Create();
        accounts.ImportError = new HttpRequestException("down");

        var dto = await service.StartAsync(UserId, AccountId, "bank1", TestHelpers.Bank1Csv);

        Assert.Equal(ImportStatus.Failed, dto.Status);
        Assert.False(string.IsNullOrEmpty(dto.Error));
        Assert.Empty(bus.Published);
    }

    [Fact]
    public async Task Start_ForeignAccount_Throws_AndSavesNothing()
    {
        var (service, db, accounts, _) = Create();
        accounts.IsOwner = false;

        await Assert.ThrowsAsync<KeyNotFoundException>(
            () => service.StartAsync(UserId, AccountId, "bank1", TestHelpers.Bank1Csv));

        Assert.Equal(0, await db.ImportBatches.AsNoTracking().CountAsync());
    }

    [Fact]
    public async Task Start_UnknownFileFormat_Throws_AndSavesNothing()
    {
        var (service, db, _, bus) = Create();

        await Assert.ThrowsAsync<ArgumentException>(
            () => service.StartAsync(UserId, AccountId, null, "foo,bar\n1,2"));

        Assert.Equal(0, await db.ImportBatches.AsNoTracking().CountAsync());
        Assert.Empty(bus.Published);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("Дата операции;Описание;Сумма")]
    public async Task Start_EmptyOrNoRows_Throws(string csv)
    {
        var (service, _, _, _) = Create();

        await Assert.ThrowsAsync<ArgumentException>(
            () => service.StartAsync(UserId, AccountId, "bank1", csv));
    }

    [Fact]
    public async Task GetAll_ReturnsOnlyOwn_NewestFirst()
    {
        var (service, db, _, _) = Create();
        db.ImportBatches.AddRange(
            NewBatch(UserId, new DateTime(2026, 10, 1)),
            NewBatch(UserId, new DateTime(2026, 10, 3)),
            NewBatch(Guid.NewGuid(), new DateTime(2026, 10, 2)));
        await db.SaveChangesAsync();

        var list = await service.GetAllAsync(UserId);

        Assert.Equal(2, list.Count);
        Assert.True(list[0].CreatedAt > list[1].CreatedAt);
    }

    [Fact]
    public async Task Get_ForeignBatch_Throws()
    {
        var (service, db, _, _) = Create();
        var batch = NewBatch(Guid.NewGuid(), DateTime.UtcNow);
        db.ImportBatches.Add(batch);
        await db.SaveChangesAsync();

        await Assert.ThrowsAsync<KeyNotFoundException>(() => service.GetAsync(UserId, batch.Id));
    }

    private static ImportBatch NewBatch(Guid userId, DateTime createdAt) => new()
    {
        Id = Guid.NewGuid(),
        UserId = userId,
        AccountId = AccountId,
        Format = "bank1",
        Status = ImportStatus.Completed,
        TotalRows = 1,
        CreatedAt = createdAt
    };
}