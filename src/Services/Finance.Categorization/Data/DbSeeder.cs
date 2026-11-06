namespace Finance.Categorization;

public static class DbSeeder
{
    public static readonly Guid OtherId = Guid.Parse("20ad71f7-5a9d-4414-8191-ee541e2bd76d");

    public static async Task SeedAsync(CategorizationDbContext db)
    {
        if (db.Categories.Any())
        {
            return;
        }

        var categories = new List<Category>
        {
            new Category { Name = "Продукты", Id = Guid.Parse("9061f60f-3ec3-47ed-b53a-f52a96ebf86e"), UserId = null },
            new Category { Name = "Транспорт", Id = Guid.Parse("e571cce0-365d-4921-adb0-8cbb6fb1e0cd"), UserId = null },
            new Category { Name = "Кафе и рестораны", Id = Guid.Parse("6048768a-96e7-4bf6-9d4c-22aaaaab5de7"), UserId = null },
            new Category { Name = "Жильё", Id = Guid.Parse("92a22574-d3cc-4b1b-95d6-fce33a70032b"), UserId = null },
            new Category { Name = "Зарплата", Id = Guid.Parse("0d9eb9ef-4fe3-4f8c-b5a4-bf7d0e5dba9b"), UserId = null },
            new Category { Name = "Покупки", Id = Guid.Parse("0f5446c6-0d4d-40b6-8ab2-102fae17f715"), UserId = null },
            new Category { Name = "Прочее", Id = OtherId, UserId = null }
        };

        int defaultPriority = 10;

        var rules = new List<Rule>();

        void AddRule(Guid categoryId, params string[] patterns)
        {
            foreach (var p in patterns)
            {
                rules.Add(new Rule
                {
                    UserId = null,
                    CategoryId = categoryId,
                    Pattern = p,
                    IsRegex = false,
                    Priority = defaultPriority
                });
            }
        }

        AddRule(categories[0].Id, "пятёрочка", "магнит", "перекрёсток");
        AddRule(categories[1].Id, "такси", "общественный транспорт", "uber");
        AddRule(categories[2].Id, "кофе", "кафе", "ресторан");
        AddRule(categories[3].Id, "аренда", "жкх");
        AddRule(categories[4].Id, "зарплата", "salary");
        AddRule(categories[5].Id, "amazon");

        db.Categories.AddRange(categories);
        db.Rules.AddRange(rules);
        await db.SaveChangesAsync();
    }
}