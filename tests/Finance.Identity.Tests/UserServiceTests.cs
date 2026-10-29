namespace Finance.Identity.Tests;

public class UserServiceTests
{
    // создаёт сервис и пользователя в БД
    private static async Task<(UserService Service, IdentityDbContext Db, User User)> Create()
    {
        var db = TestHelpers.CreateDb();
        var user = new User { Id = Guid.NewGuid(), Email = "a@b.com", PasswordHash = "hash", Role = "User" };
        db.Users.Add(user);
        await db.SaveChangesAsync();
        return (new UserService(db), db, user);
    }

    [Fact]
    public async Task Get_returns_user_data()
    {
        var (service, _, user) = await Create();

        var dto = await service.GetAsync(user.Id);

        Assert.Equal("a@b.com", dto.Email);
        Assert.Equal("User", dto.Role);
    }

    [Fact]
    public async Task Get_missing_user_throws()
    {
        var (service, _, _) = await Create();

        await Assert.ThrowsAsync<KeyNotFoundException>(() => service.GetAsync(Guid.NewGuid()));
    }

    [Fact]
    public async Task GetAll_returns_all_users()
    {
        var (service, db, _) = await Create();
        db.Users.Add(new User { Id = Guid.NewGuid(), Email = "c@d.com", PasswordHash = "hash", Role = "Admin" });
        await db.SaveChangesAsync();

        var list = await service.GetAllAsync();

        Assert.Equal(2, list.Count);
    }

    [Fact]
    public async Task SetRole_changes_role()
    {
        var (service, db, user) = await Create();

        await service.SetRoleAsync(user.Id, "Admin");

        Assert.Equal("Admin", (await db.Users.FindAsync(user.Id))!.Role);
    }

    [Fact]
    public async Task SetRole_with_unknown_role_throws()
    {
        var (service, _, user) = await Create();

        await Assert.ThrowsAsync<ArgumentException>(() => service.SetRoleAsync(user.Id, "Boss"));
    }

    [Fact]
    public async Task SetRole_for_missing_user_throws()
    {
        var (service, _, _) = await Create();

        await Assert.ThrowsAsync<KeyNotFoundException>(() => service.SetRoleAsync(Guid.NewGuid(), "Admin"));
    }
}