using Microsoft.EntityFrameworkCore;
namespace Finance.Identity;

public class AuthService(IdentityDbContext db, TokenService tokens)
{
    public async Task<TokenResponse> RegisterAsync(string email, string password)
    {
        email = email.Trim().ToLower();

        if (string.IsNullOrEmpty(email) || password.Length < 6)
        {
            throw new ArgumentException("Password is too short");
        }

        if (await db.Users.AnyAsync(u => u.Email == email))
        {
            throw new ArgumentException("Email already registered");
        }

        var user = new User
        {
            Id = Guid.NewGuid(),
            Role = "User",
            Email = email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(password)
        };
        db.Users.Add(user);

        return await IssueTokensAsync(user);
    }

    public async Task<TokenResponse> LoginAsync(string email, string password)
    {
        email = email.Trim().ToLower();
        var user = await db.Users.FirstOrDefaultAsync(u => u.Email == email);

        if (user == null || !BCrypt.Net.BCrypt.Verify(password, user.PasswordHash))
            throw new UnauthorizedAccessException("Invalid email or password");

        return await IssueTokensAsync(user);
    }

    private async Task<TokenResponse> IssueTokensAsync(User user)
    {
        var refresh = tokens.CreateRefreshToken();

        db.RefreshTokens.Add(new RefreshToken
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            TokenHash = TokenService.Hash(refresh),
            ExpiresAt = DateTime.UtcNow.AddDays(7)
        });

        await db.SaveChangesAsync();

        return new TokenResponse(tokens.CreateAccessToken(user), refresh);
    }

    public async Task<TokenResponse> RefreshAsync(string refreshToken)
    {
        var hash = TokenService.Hash(refreshToken);
        var saved = await db.RefreshTokens.FirstOrDefaultAsync(t => t.TokenHash == hash);

        if (saved == null || saved.Revoked || saved.ExpiresAt < DateTime.UtcNow)
            throw new UnauthorizedAccessException("Invalid refresh token");

        saved.Revoked = true;
        var user = await db.Users.FirstAsync(u => u.Id == saved.UserId);

        return await IssueTokensAsync(user);
    }

    public async Task LogoutAsync(string refreshToken)
    {
        var hash = TokenService.Hash(refreshToken);
        var saved = await db.RefreshTokens.FirstOrDefaultAsync(t => t.TokenHash == hash);

        if (saved != null)
        {
            saved.Revoked = true;
            await db.SaveChangesAsync();
        }
    }
}