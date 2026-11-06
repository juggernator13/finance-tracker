using Microsoft.EntityFrameworkCore;

namespace Finance.Categorization.Tests;

public class RuleServiceTests
{
    private readonly Guid _userId = Guid.NewGuid();

    [Fact]
    public async Task GetAll_ReturnsSharedAndOwn_NotOthers()
    {
        var db = TestHelpers.CreateDb();
        var category = TestHelpers.AddCategory(db, "Кафе");
        TestHelpers.AddRule(db, category, "кофе");                      // общее
        TestHelpers.AddRule(db, category, "латте", _userId);            // моё
        TestHelpers.AddRule(db, category, "эспрессо", Guid.NewGuid());  // чужое
        var service = new RuleService(db);

        var result = await service.GetAllAsync(_userId);

        Assert.Equal(2, result.Count);
        Assert.DoesNotContain(result, r => r.Pattern == "эспрессо");
    }

    [Fact]
    public async Task Create_WithSharedCategory_SavesPersonalRule()
    {
        var db = TestHelpers.CreateDb();
        var category = TestHelpers.AddCategory(db, "Кафе");
        var service = new RuleService(db);

        var dto = await service.CreateAsync(_userId, category.Id, "латте", false, 50);

        Assert.Equal("латте", dto.Pattern);
        Assert.False(dto.IsShared);
        var saved = await db.Rules.AsNoTracking().SingleAsync();
        Assert.Equal(_userId, saved.UserId);
        Assert.Equal(50, saved.Priority);
    }

    [Fact]
    public async Task Create_WithOtherUsersCategory_Throws()
    {
        var db = TestHelpers.CreateDb();
        var foreign = TestHelpers.AddCategory(db, "Чужая", Guid.NewGuid());
        var service = new RuleService(db);

        await Assert.ThrowsAsync<KeyNotFoundException>(() =>
            service.CreateAsync(_userId, foreign.Id, "x", false, 10));
    }

    [Fact]
    public async Task Create_InvalidRegex_Throws()
    {
        var db = TestHelpers.CreateDb();
        var category = TestHelpers.AddCategory(db, "Кафе");
        var service = new RuleService(db);

        await Assert.ThrowsAsync<ArgumentException>(() =>
            service.CreateAsync(_userId, category.Id, "[abc", true, 10));
    }

    [Fact]
    public async Task Create_EmptyPattern_Throws()
    {
        var db = TestHelpers.CreateDb();
        var category = TestHelpers.AddCategory(db, "Кафе");
        var service = new RuleService(db);

        await Assert.ThrowsAsync<ArgumentException>(() =>
            service.CreateAsync(_userId, category.Id, "  ", false, 10));
    }

    [Fact]
    public async Task Delete_OwnRule_RemovesIt()
    {
        var db = TestHelpers.CreateDb();
        var category = TestHelpers.AddCategory(db, "Кафе");
        var rule = TestHelpers.AddRule(db, category, "латте", _userId);
        var service = new RuleService(db);

        await service.DeleteAsync(_userId, rule.Id);

        Assert.Equal(0, await db.Rules.CountAsync());
    }

    [Fact]
    public async Task Delete_SharedRule_Throws()
    {
        var db = TestHelpers.CreateDb();
        var category = TestHelpers.AddCategory(db, "Кафе");
        var shared = TestHelpers.AddRule(db, category, "кофе");
        var service = new RuleService(db);

        await Assert.ThrowsAsync<KeyNotFoundException>(() => service.DeleteAsync(_userId, shared.Id));
    }

    [Fact]
    public async Task Preview_FindsCategoryBySubstring()
    {
        var db = TestHelpers.CreateDb();
        var cafe = TestHelpers.AddCategory(db, "Кафе");
        var rule = TestHelpers.AddRule(db, cafe, "кофе");
        var service = new RuleService(db);

        var result = await service.PreviewAsync(_userId, "Кофе с собой");

        Assert.Equal(cafe.Id, result.CategoryId);
        Assert.Equal("Кафе", result.CategoryName);
        Assert.Equal(rule.Id, result.RuleId);
    }

    [Fact]
    public async Task Preview_NoMatch_ReturnsOther()
    {
        var db = TestHelpers.CreateDb();
        TestHelpers.AddCategory(db, "Прочее", id: DbSeeder.OtherId);
        var cafe = TestHelpers.AddCategory(db, "Кафе");
        TestHelpers.AddRule(db, cafe, "кофе");
        var service = new RuleService(db);

        var result = await service.PreviewAsync(_userId, "абракадабра");

        Assert.Equal(DbSeeder.OtherId, result.CategoryId);
        Assert.Equal("Прочее", result.CategoryName);
        Assert.Null(result.RuleId);
    }

    [Fact]
    public async Task Preview_PersonalRuleWithHigherPriority_Wins()
    {
        var db = TestHelpers.CreateDb();
        var cafe = TestHelpers.AddCategory(db, "Кафе");
        var favorite = TestHelpers.AddCategory(db, "Любимое кафе", _userId);
        TestHelpers.AddRule(db, cafe, "кофе", priority: 10);
        TestHelpers.AddRule(db, favorite, "кофе", _userId, priority: 100);
        var service = new RuleService(db);

        var result = await service.PreviewAsync(_userId, "Кофе");

        Assert.Equal(favorite.Id, result.CategoryId);
    }
}