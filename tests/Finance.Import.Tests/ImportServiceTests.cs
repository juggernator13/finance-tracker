using Microsoft.EntityFrameworkCore;

namespace Finance.Import.Tests;

public class ImportServiceTests
{
    private readonly Guid _userId = Guid.NewGuid();
    private readonly Guid _accountId = Guid.NewGuid();

    private static (ImportService Service, ImportDbContext Db, FakeAccountsClient Accounts) Create()
    {
        var db = TestHelpers.CreateDb();
        var accounts = new FakeAccountsClient();
        var factory = new StatementParserFactory(new IStatementParser[] { new Bank1CsvParser(), new Bank2CsvParser() });
        return (new ImportService(db, factory, accounts), db, accounts);
    }

    [Fact]
    public async Task Start_Success_SavesCompletedBatch()
    {
        var (service, db, _) = Create();

        var dto = await service.StartAsync(_userId, _accountId, null, TestHelpers.Bank1Csv);

        Assert.Equal(ImportStatus.Completed, dto.Status);
        Assert.Equal("bank1", dto.Format);
        Assert.Equal(3, dto.TotalRows);
        Assert.Equal(3, dto.Created);

        var saved = await db.ImportBatches.AsNoTracking().SingleAsync();
        Assert.Equal(_userId, saved.UserId);
        Assert.Equal(_accountId, saved.AccountId);
        Assert.Equal(ImportStatus.Completed, saved.Status);
    }

    [Fact]
    public async Task Start_Bank2_WorksWithExplicitFormat()
    {
        var (service, _, _) = Create();

        var dto = await service.StartAsync(_userId, _accountId, "bank2", TestHelpers.Bank2Csv);

        Assert.Equal("bank2", dto.Format);
        Assert.Equal(2, dto.TotalRows);
    }

    [Fact]
    public async Task Start_SendsRowsWithOrdinalsToAccounts()
    {
        var (service, _, accounts) = Create();

        await service.StartAsync(_userId, _accountId, null, TestHelpers.Bank1Csv);

        Assert.NotNull(accounts.ReceivedRows);
        Assert.Equal(0, accounts.ReceivedRows![1].Ordinal);
        Assert.Equal(1, accounts.ReceivedRows[2].Ordinal);
    }

    [Fact]
    public async Task Start_NotOwner_ThrowsAndSavesNothing()
    {
        var (service, db, accounts) = Create();
        accounts.IsOwner = false;

        await Assert.ThrowsAsync<KeyNotFoundException>(() =>
            service.StartAsync(_userId, _accountId, null, TestHelpers.Bank1Csv));

        Assert.Equal(0, await db.ImportBatches.CountAsync());
    }

    [Fact]
    public async Task Start_InvalidFile_ThrowsAndSavesNothing()
    {
        var (service, db, _) = Create();
        string csv = TestHelpers.Bank1Header + "\n32.10.2026;A;-1";

        await Assert.ThrowsAsync<ArgumentException>(() =>
            service.StartAsync(_userId, _accountId, null, csv));

        Assert.Equal(0, await db.ImportBatches.CountAsync());
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public async Task Start_EmptyFile_Throws(string csv)
    {
        var (service, _, _) = Create();

        await Assert.ThrowsAsync<ArgumentException>(() =>
            service.StartAsync(_userId, _accountId, null, csv));
    }

    [Fact]
    public async Task Start_HeaderOnly_Throws()
    {
        var (service, _, _) = Create();

        await Assert.ThrowsAsync<ArgumentException>(() =>
            service.StartAsync(_userId, _accountId, null, TestHelpers.Bank1Header));
    }

    [Fact]
    public async Task Start_UnknownFormat_Throws()
    {
        var (service, _, _) = Create();

        await Assert.ThrowsAsync<ArgumentException>(() =>
            service.StartAsync(_userId, _accountId, "bank99", TestHelpers.Bank1Csv));
    }

    [Fact]
    public async Task Start_AccountsDown_SavesFailedBatch()
    {
        var (service, db, accounts) = Create();
        accounts.ImportError = new HttpRequestException("down");

        var dto = await service.StartAsync(_userId, _accountId, null, TestHelpers.Bank1Csv);

        Assert.Equal(ImportStatus.Failed, dto.Status);
        Assert.Equal("down", dto.Error);

        var saved = await db.ImportBatches.AsNoTracking().SingleAsync();
        Assert.Equal(ImportStatus.Failed, saved.Status);
    }

    [Fact]
    public async Task GetAll_ReturnsOnlyOwnBatches_NewestFirst()
    {
        var (service, db, _) = Create();
        var old = new ImportBatch { Id = Guid.NewGuid(), UserId = _userId, AccountId = _accountId, Format = "bank1", Status = ImportStatus.Completed, CreatedAt = DateTime.UtcNow.AddDays(-1) };
        var recent = new ImportBatch { Id = Guid.NewGuid(), UserId = _userId, AccountId = _accountId, Format = "bank1", Status = ImportStatus.Completed, CreatedAt = DateTime.UtcNow };
        var foreign = new ImportBatch { Id = Guid.NewGuid(), UserId = Guid.NewGuid(), AccountId = _accountId, Format = "bank1", Status = ImportStatus.Completed, CreatedAt = DateTime.UtcNow };
        db.ImportBatches.AddRange(old, recent, foreign);
        await db.SaveChangesAsync();

        var list = await service.GetAllAsync(_userId);

        Assert.Equal(2, list.Count);
        Assert.Equal(recent.Id, list[0].Id);
        Assert.Equal(old.Id, list[1].Id);
    }

    [Fact]
    public async Task Get_OwnBatch_ReturnsIt()
    {
        var (service, _, _) = Create();
        var started = await service.StartAsync(_userId, _accountId, null, TestHelpers.Bank1Csv);

        var dto = await service.GetAsync(_userId, started.Id);

        Assert.Equal(started.Id, dto.Id);
    }

    [Fact]
    public async Task Get_OtherUsersBatch_Throws()
    {
        var (service, _, _) = Create();
        var started = await service.StartAsync(_userId, _accountId, null, TestHelpers.Bank1Csv);

        await Assert.ThrowsAsync<KeyNotFoundException>(() =>
            service.GetAsync(Guid.NewGuid(), started.Id));
    }

    [Fact]
    public void GetFormats_ReturnsBothBanks()
    {
        var (service, _, _) = Create();

        Assert.Equal(new[] { "bank1", "bank2" }, service.GetFormats());
    }
}