using Microsoft.EntityFrameworkCore;

namespace Finance.Categorization.Tests;

public static class TestHelpers
{
    public static CategorizationDbContext CreateDb()
    {
        var options = new DbContextOptionsBuilder<CategorizationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new CategorizationDbContext(options);
    }

    public static Category AddCategory(CategorizationDbContext db, string name, Guid? userId = null, Guid? id = null)
    {
        var category = new Category { Id = id ?? Guid.NewGuid(), UserId = userId, Name = name };
        db.Categories.Add(category);
        db.SaveChanges();
        return category;
    }

    public static Rule AddRule(CategorizationDbContext db, Category category, string pattern,
        Guid? userId = null, int priority = 10, bool isRegex = false)
    {
        var rule = new Rule
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            CategoryId = category.Id,
            Pattern = pattern,
            IsRegex = isRegex,
            Priority = priority
        };
        db.Rules.Add(rule);
        db.SaveChanges();
        return rule;
    }
}