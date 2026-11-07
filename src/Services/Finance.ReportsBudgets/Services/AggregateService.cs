using Finance.Contracts;

using Microsoft.EntityFrameworkCore;

namespace Finance.ReportsBudgets;

public class AggregateService(ReportsDbContext db)
{
    public async Task<List<AggregateChange>> ApplyAsync(Guid userId, IEnumerable<CategorizedItem> items)
    {
        var changes = new List<AggregateChange>();

        var groups = items
            .Where(i => i.Amount != 0)
            .GroupBy(i => (i.CategoryId, i.Date.Year, i.Date.Month));

        foreach (var group in groups)
        {
            var (categoryId, year, month) = group.Key;

            decimal expense = group.Where(i => i.Amount < 0).Sum(i => -i.Amount);
            decimal income = group.Where(i => i.Amount > 0).Sum(i => i.Amount);
            string name = group.First().CategoryName;

            var row = await db.MonthlyTotals.FirstOrDefaultAsync(t =>
                t.UserId == userId && t.CategoryId == categoryId && t.Year == year && t.Month == month);

            if (row == null)
            {
                row = new MonthlyTotal
                {
                    Id = Guid.NewGuid(),
                    UserId = userId,
                    CategoryId = categoryId,
                    Year = year,
                    Month = month
                };
                db.MonthlyTotals.Add(row);
            }

            decimal before = row.Expense;
            row.Expense += expense;
            row.Income += income;
            row.CategoryName = name;

            changes.Add(new AggregateChange(categoryId, name, year, month, before, row.Expense));
        }

        await db.SaveChangesAsync();
        return changes;
    }
}