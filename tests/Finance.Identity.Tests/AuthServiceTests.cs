using Microsoft.EntityFrameworkCore;

namespace Finance.Identity.Tests;

public class AuthServiceTests
{
    private static (AuthService Auth, IdentityDbContext Db) Create()
    {
        var db = TestHelpers.CreateDb();
        return (new AuthService(db, TestHelpers.CreateTokens()), db);
    }

    [Fact]
    public async Task Register_saves_user_with_hashed_password()
    {
        var (auth, db) = Create();

        await auth.RegisterAsync("A@B.com", "secret123");

        var user = await db.Users.SingleAsync();
        Assert.Equal("a@b.com", user.Email);
        Assert.Equal("User", user.Role);
        Assert.NotEqual("secret123", user.PasswordHash);
        Assert.True(BCrypt.Net.BCrypt.Verify("secret123", user.PasswordHash));
    }

    [Fact]
    public async Task Register_with_existing_email_throws()
    {
        var (auth, _) = Create();
        await auth.RegisterAsync("a@b.com", "secret123");

        await Assert.ThrowsAsync<ArgumentException>(() => auth.RegisterAsync("a@b.com", "other456"));
    }

    [Fact]
    public async Task Register_with_short_password_throws()
    {
        var (auth, _) = Create();

        await Assert.ThrowsAsync<ArgumentException>(() => auth.RegisterAsync("a@b.com", "123"));
    }

    [Fact]
    public async Task Login_with_correct_password_returns_tokens()
    {
        var (auth, _) = Create();
        await auth.RegisterAsync("a@b.com", "secret123");

        var result = await auth.LoginAsync("a@b.com", "secret123");

        Assert.False(string.IsNullOrEmpty(result.AccessToken));
        Assert.False(string.IsNullOrEmpty(result.RefreshToken));
    }

    [Fact]
    public async Task Login_with_wrong_password_throws()
    {
        var (auth, _) = Create();
        await auth.RegisterAsync("a@b.com", "secret123");

        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => auth.LoginAsync("a@b.com", "wrong"));
    }

    [Fact]
    public async Task Login_with_unknown_email_throws()
    {
        var (auth, _) = Create();

        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => auth.LoginAsync("nobody@b.com", "secret123"));
    }

    [Fact]
    public async Task Refresh_returns_new_refresh_token_and_revokes_old_one()
    {
        var (auth, db) = Create();
        var first = await auth.RegisterAsync("a@b.com", "secret123");

        var second = await auth.RefreshAsync(first.RefreshToken);

        Assert.NotEqual(first.RefreshToken, second.RefreshToken);
        var oldHash = TokenService.Hash(first.RefreshToken);
        var old = await db.RefreshTokens.SingleAsync(t => t.TokenHash == oldHash);
        Assert.True(old.Revoked);
    }

    [Fact]
    public async Task Refresh_with_already_used_token_throws()
    {
        var (auth, _) = Create();
        var first = await auth.RegisterAsync("a@b.com", "secret123");
        await auth.RefreshAsync(first.RefreshToken);

        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => auth.RefreshAsync(first.RefreshToken));
    }

    [Fact]
    public async Task Refresh_with_unknown_token_throws()
    {
        var (auth, _) = Create();

        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => auth.RefreshAsync("no-such-token"));
    }

    [Fact]
    public async Task Refresh_with_expired_token_throws()
    {
        var (auth, db) = Create();
        var first = await auth.RegisterAsync("a@b.com", "secret123");
        var saved = await db.RefreshTokens.SingleAsync();
        saved.ExpiresAt = DateTime.UtcNow.AddDays(-1);
        await db.SaveChangesAsync();

        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => auth.RefreshAsync(first.RefreshToken));
    }

    [Fact]
    public async Task Logout_makes_refresh_token_unusable()
    {
        var (auth, _) = Create();
        var first = await auth.RegisterAsync("a@b.com", "secret123");

        await auth.LogoutAsync(first.RefreshToken);

        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => auth.RefreshAsync(first.RefreshToken));
    }
}