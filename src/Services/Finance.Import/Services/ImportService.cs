using Microsoft.EntityFrameworkCore;

namespace Finance.Import;

public class ImportService(ImportDbContext db, StatementParserFactory factory, IAccountsClient accounts)
{
    public async Task<ImportBatchDto> StartAsync(Guid userId, Guid accountId, string? format, string csvText)
    {
        if (string.IsNullOrWhiteSpace(csvText))
        {
            throw new ArgumentException("File is empty");
        }

        if (!await accounts.IsOwnerAsync(userId, accountId))
        {
            throw new KeyNotFoundException("Account not found");
        }

        IStatementParser parser = factory.Resolve(format, CsvValueNormalizer.GetHeader(csvText));

        var rows = parser.Parse(csvText);

        if (rows.Count == 0)
        {
            throw new ArgumentException("File has no rows");
        }

        rows = OrdinalAssigner.Assign(rows);

        ImportBatch batch = new ImportBatch
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            AccountId = accountId,
            Format = parser.Format,
            Status = ImportStatus.Parsed,
            TotalRows = rows.Count,
            CreatedAt = DateTime.UtcNow
        };

        db.ImportBatches.Add(batch);
        await db.SaveChangesAsync();

        try
        {
            ImportResult result = await accounts.ImportAsync(batch.Id, userId, accountId, rows);
            batch.Created = result.Created;
            batch.Skipped = result.Skipped;
            batch.Status = ImportStatus.Completed;
        }
        catch (HttpRequestException ex)
        {
            batch.Status = ImportStatus.Failed;
            batch.Error = ex.Message;
        }

        await db.SaveChangesAsync();

        return ToDto(batch);
    }

    public async Task<List<ImportBatchDto>> GetAllAsync(Guid userId)
    {
        return await db.ImportBatches
        .Where(b => b.UserId == userId)
        .OrderByDescending(b => b.CreatedAt)
        .Select(b => ToDto(b))
        .ToListAsync();
    }

    public async Task<ImportBatchDto> GetAsync(Guid userId, Guid id)
    {
        ImportBatch? batch = await db.ImportBatches
            .FirstOrDefaultAsync(b => b.Id == id && b.UserId == userId);

        if (batch == null)
        {
            throw new KeyNotFoundException("Import not found");
        }

        return ToDto(batch);
    }

    public List<string> GetFormats()
    {
        return factory.Formats;
    }

    private static ImportBatchDto ToDto(ImportBatch b)
    {
        return new ImportBatchDto(b.Id, b.AccountId, b.Format, b.Status,
            b.TotalRows, b.Created, b.Skipped, b.Error, b.CreatedAt);
    }
}