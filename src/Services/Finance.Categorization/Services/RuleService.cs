using System.Text.RegularExpressions;

using Microsoft.EntityFrameworkCore;

namespace Finance.Categorization;

public class RuleService(CategorizationDbContext db)
{
    public async Task<List<RuleDto>> GetAllAsync(Guid userId)
    {
        return await db.Rules
            .Where(r => r.UserId == null || r.UserId == userId)
            .Select(r => new RuleDto(r.Id, r.CategoryId, r.Pattern, r.IsRegex, r.Priority, r.UserId == null))
            .ToListAsync();
    }

    public async Task<RuleDto> CreateAsync(Guid userId, Guid categoryId, string pattern, bool isRegex, int priority)
    {
        var isVisible = await db.Categories
            .AnyAsync(c => c.UserId == null || c.UserId == userId);

        if (!isVisible)
        {
            throw new KeyNotFoundException("Category is not visible for user");
        }

        if (string.IsNullOrWhiteSpace(pattern))
        {
            throw new ArgumentException("Pattern cannot be empty");
        }

        if (isRegex)
        {
            try
            {
                _ = new Regex(pattern);
            }
            catch (Exception)
            {
                throw new ArgumentException("Invalid regex");
            }
        }
        Rule rule = new Rule()
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            CategoryId = categoryId,
            Pattern = pattern,
            IsRegex = isRegex,
            Priority = priority
        };

        db.Rules.Add(rule);
        await db.SaveChangesAsync();

        return new RuleDto(rule.Id, rule.CategoryId, rule.Pattern, rule.IsRegex, rule.Priority, false);
    }

    public async Task DeleteAsync(Guid userId, Guid id)
    {
        var rule = await db.Rules.FindAsync(id);

        if (rule == null || rule.UserId != userId)
        {
            throw new KeyNotFoundException();
        }

        db.Rules.Remove(rule);
        await db.SaveChangesAsync();
    }

    public async Task<PreviewResult> PreviewAsync(Guid userId, string description)
    {
        if (string.IsNullOrWhiteSpace(description))
        {
            throw new ArgumentException("Description is empty");
        }

        var rules = await db.Rules
            .AsNoTracking()
            .Where(r => r.UserId == null || r.UserId == userId)
            .ToListAsync();

        var rule = RuleMatcher.Match(description, rules);

        Guid categoryId = rule?.CategoryId ?? DbSeeder.OtherId;

        var category = await db.Categories
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == categoryId);

        if (category == null)
        {
            throw new KeyNotFoundException("Category not found");
        }

        return new PreviewResult(category.Id, category.Name, rule?.Id);
    }
}