using Microsoft.EntityFrameworkCore;

namespace Finance.ReportsBudgets;

public class BudgetService(ReportsDbContext db)
{
    public async Task<List<BudgetDto>> GetAllAsync(Guid userId)
    {
        var budgets = await db.Budgets
            .AsNoTracking()
            .Where(b => b.UserId == userId)
            .OrderBy(b => b.CategoryName)
            .ToListAsync();

        var spent = await GetSpentAsync(userId);

        return budgets
            .Select(b => ToDto(b, spent.GetValueOrDefault(b.CategoryId)))
            .ToList();
    }

    public async Task<BudgetDto> CreateAsync(Guid userId, Guid categoryId, string categoryName, decimal limit)
    {
        CheckLimit(limit);

        if (string.IsNullOrWhiteSpace(categoryName))
        {
            throw new ArgumentException("Category name cannot be empty");
        }

        bool exists = await db.Budgets.AnyAsync(b => b.UserId == userId && b.CategoryId == categoryId);
        if (exists)
        {
            throw new ArgumentException("Budget for this category already exists");
        }

        var budget = new Budget
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            CategoryId = categoryId,
            CategoryName = categoryName.Trim(),
            Limit = limit
        };

        db.Budgets.Add(budget);
        await db.SaveChangesAsync();

        var spent = await GetSpentAsync(userId);
        return ToDto(budget, spent.GetValueOrDefault(categoryId));
    }

    public async Task<BudgetDto> UpdateAsync(Guid userId, Guid id, decimal limit)
    {
        CheckLimit(limit);

        var budget = await db.Budgets.FirstOrDefaultAsync(b => b.Id == id && b.UserId == userId);
        if (budget == null)
        {
            throw new KeyNotFoundException("Budget not found");
        }

        budget.Limit = limit;
        await db.SaveChangesAsync();

        var spent = await GetSpentAsync(userId);
        return ToDto(budget, spent.GetValueOrDefault(budget.CategoryId));
    }

    public async Task DeleteAsync(Guid userId, Guid id)
    {
        var budget = await db.Budgets.FirstOrDefaultAsync(b => b.Id == id && b.UserId == userId);
        if (budget == null)
        {
            throw new KeyNotFoundException("Budget not found");
        }

        db.Budgets.Remove(budget);
        await db.SaveChangesAsync();
    }

    public async Task<List<Budget>> ListEntitiesAsync(Guid userId)
    {
        return await db.Budgets
            .AsNoTracking()
            .Where(b => b.UserId == userId)
            .ToListAsync();
    }

    private async Task<Dictionary<Guid, decimal>> GetSpentAsync(Guid userId)
    {
        var now = DateTime.UtcNow;

        return await db.MonthlyTotals
            .AsNoTracking()
            .Where(t => t.UserId == userId && t.Year == now.Year && t.Month == now.Month)
            .ToDictionaryAsync(t => t.CategoryId, t => t.Expense);
    }

    private static void CheckLimit(decimal limit)
    {
        if (limit <= 0)
        {
            throw new ArgumentException("Limit must be greater than zero");
        }
    }

    private static BudgetDto ToDto(Budget b, decimal spent)
    {
        decimal percent = b.Limit == 0 ? 0 : Math.Round(spent / b.Limit * 100, 1);
        return new BudgetDto(b.Id, b.CategoryId, b.CategoryName, b.Limit, spent, percent, spent > b.Limit);
    }
}