using Microsoft.EntityFrameworkCore;

namespace Finance.Categorization;

public class CategoryService(CategorizationDbContext db)
{
    public async Task<List<CategoryDto>> GetAllAsync(Guid userId)
    {
        return await db.Categories
            .Where(c => c.UserId == null || c.UserId == userId)
            .OrderBy(c => c.Name)
            .Select(c => new CategoryDto(c.Id, c.Name, c.UserId == null))
            .ToListAsync();
    }

    public async Task<CategoryDto> CreateAsync(Guid userId, string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Category name cannot be empty");
        }

        bool isNameExists = await db.Categories
            .AnyAsync(c => (c.UserId == null || c.UserId == userId)
                && c.Name.ToLower() == name.Trim().ToLower());

        if (isNameExists)
        {
            throw new ArgumentException($"Category {name} already exist");
        }

        Category category = new Category()
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Name = name.Trim()
        };

        await db.Categories.AddAsync(category);
        await db.SaveChangesAsync();

        return new CategoryDto(category.Id, category.Name, false);
    }

    public async Task DeleteAsync(Guid userId, Guid id)
    {
        var category = await db.Categories
            .FirstOrDefaultAsync(c => c.Id == id && c.UserId == userId);

        if (category == null)
        {
            throw new KeyNotFoundException("Category not found");
        }

        var categoryRules = db.Rules.Where(r => r.CategoryId == id);

        db.Rules.RemoveRange(categoryRules);
        db.Categories.Remove(category);

        await db.SaveChangesAsync();
    }
}
