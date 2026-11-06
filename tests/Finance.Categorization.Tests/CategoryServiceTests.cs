using Microsoft.EntityFrameworkCore;

namespace Finance.Categorization.Tests;

public class CategoryServiceTests
{
    private readonly Guid _userId = Guid.NewGuid();

    [Fact]
    public async Task GetAll_ReturnsSharedAndOwn_NotOthers()
    {
        var db = TestHelpers.CreateDb();
        TestHelpers.AddCategory(db, "Общая");
        TestHelpers.AddCategory(db, "Моя", _userId);
        TestHelpers.AddCategory(db, "Чужая", Guid.NewGuid());
        var service = new CategoryService(db);

        var result = await service.GetAllAsync(_userId);

        Assert.Equal(2, result.Count);
        Assert.DoesNotContain(result, c => c.Name == "Чужая");
    }

    [Fact]
    public async Task Create_SavesPersonalCategory()
    {
        var db = TestHelpers.CreateDb();
        var service = new CategoryService(db);

        var dto = await service.CreateAsync(_userId, "Хобби");

        Assert.Equal("Хобби", dto.Name);
        Assert.False(dto.IsShared);
        var saved = await db.Categories.AsNoTracking().SingleAsync();
        Assert.Equal(_userId, saved.UserId);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public async Task Create_EmptyName_Throws(string name)
    {
        var service = new CategoryService(TestHelpers.CreateDb());

        await Assert.ThrowsAsync<ArgumentException>(() => service.CreateAsync(_userId, name));
    }

    [Fact]
    public async Task Create_DuplicateOfOwnCategory_Throws()
    {
        var db = TestHelpers.CreateDb();
        TestHelpers.AddCategory(db, "Хобби", _userId);
        var service = new CategoryService(db);

        await Assert.ThrowsAsync<ArgumentException>(() => service.CreateAsync(_userId, "Хобби"));
    }

    [Fact]
    public async Task Create_DuplicateOfSharedCategory_Throws()
    {
        var db = TestHelpers.CreateDb();
        TestHelpers.AddCategory(db, "Продукты");
        var service = new CategoryService(db);

        await Assert.ThrowsAsync<ArgumentException>(() => service.CreateAsync(_userId, "Продукты"));
    }

    [Fact]
    public async Task Delete_OwnCategory_RemovesItAndItsRules()
    {
        var db = TestHelpers.CreateDb();
        var category = TestHelpers.AddCategory(db, "Хобби", _userId);
        TestHelpers.AddRule(db, category, "шахматы", _userId);
        var service = new CategoryService(db);

        await service.DeleteAsync(_userId, category.Id);

        Assert.Equal(0, await db.Categories.CountAsync());
        Assert.Equal(0, await db.Rules.CountAsync());
    }

    [Fact]
    public async Task Delete_SharedCategory_Throws()
    {
        var db = TestHelpers.CreateDb();
        var shared = TestHelpers.AddCategory(db, "Продукты");
        var service = new CategoryService(db);

        await Assert.ThrowsAsync<KeyNotFoundException>(() => service.DeleteAsync(_userId, shared.Id));
    }

    [Fact]
    public async Task Delete_OtherUsersCategory_Throws()
    {
        var db = TestHelpers.CreateDb();
        var foreign = TestHelpers.AddCategory(db, "Чужая", Guid.NewGuid());
        var service = new CategoryService(db);

        await Assert.ThrowsAsync<KeyNotFoundException>(() => service.DeleteAsync(_userId, foreign.Id));
    }
}