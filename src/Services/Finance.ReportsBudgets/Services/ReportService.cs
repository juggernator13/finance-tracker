using System.Text.Json;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;

namespace Finance.ReportsBudgets;

public class ReportService(ReportsDbContext db, IDistributedCache cache, ILogger logger)
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public async Task<List<CategoryReportItem>> GetByCategoryAsync(Guid userId, int year, int month)
    {
        if (year < 2000 || year > 2100)
        {
            throw new ArgumentException("Year must be from 2000 to 2100");
        }

        if (month < 1 || month > 12)
        {
            throw new ArgumentException("Month must be from 1 to 12");
        }

        var key = ReportCacheKeys.ByCategory(userId, year, month);

        try
        {
            var cached = await cache.GetStringAsync(key);
            if (cached != null)
            {
                return JsonSerializer.Deserialize<List<CategoryReportItem>>(cached, JsonOptions)!;
            }
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Redis read failed");
        }

        var result = await db.MonthlyTotals
            .AsNoTracking()
            .Where(t => t.UserId == userId && t.Year == year && t.Month == month)
            .OrderByDescending(t => t.Expense)
            .Select(t => new CategoryReportItem(t.CategoryId, t.CategoryName, t.Expense, t.Income))
            .ToListAsync();

        try
        {
            await cache.SetStringAsync(key, JsonSerializer.Serialize(result, JsonOptions),
                new DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5) });
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Redis write failed");
        }

        return result;
    }

    public async Task<List<MonthlyReportItem>> GetMonthlyAsync(Guid userId, int year)
    {
        if (year < 2000 || year > 2100)
        {
            throw new ArgumentException("Year must be from 2000 to 2100");
        }

        var rows = await db.MonthlyTotals
            .AsNoTracking()
            .Where(t => t.UserId == userId && t.Year == year)
            .GroupBy(t => t.Month)
            .Select(g => new
            {
                Month = g.Key,
                Expense = g.Sum(t => t.Expense),
                Income = g.Sum(t => t.Income)
            })
            .OrderBy(r => r.Month)
            .ToListAsync();

        return rows
            .Select(r => new MonthlyReportItem(year, r.Month, r.Expense, r.Income, r.Income - r.Expense))
            .ToList();
    }
}