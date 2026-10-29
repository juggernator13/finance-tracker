using System.IdentityModel.Tokens.Jwt;

namespace Finance.Identity.Tests;

public class TokenServiceTests
{
    private static User CreateUser() => new()
    {
        Id = Guid.NewGuid(),
        Email = "a@b.com",
        Role = "Admin"
    };

    [Fact]
    public void Access_token_contains_sub_email_and_role()
    {
        var user = CreateUser();
        var token = TestHelpers.CreateTokens().CreateAccessToken(user);

        var jwt = new JwtSecurityTokenHandler().ReadJwtToken(token);

        Assert.Equal(user.Id.ToString(), jwt.Claims.First(c => c.Type == "sub").Value);
        Assert.Equal("a@b.com", jwt.Claims.First(c => c.Type == "email").Value);
        Assert.Equal("Admin", jwt.Claims.First(c => c.Type == "role").Value);
    }

    [Fact]
    public void Access_token_lives_about_15_minutes()
    {
        var token = TestHelpers.CreateTokens().CreateAccessToken(CreateUser());

        var jwt = new JwtSecurityTokenHandler().ReadJwtToken(token);

        Assert.InRange(jwt.ValidTo, DateTime.UtcNow.AddMinutes(14), DateTime.UtcNow.AddMinutes(16));
    }

    [Fact]
    public void Refresh_tokens_are_different_each_time()
    {
        var tokens = TestHelpers.CreateTokens();

        Assert.NotEqual(tokens.CreateRefreshToken(), tokens.CreateRefreshToken());
    }

    [Fact]
    public void Hash_is_stable_and_depends_on_input()
    {
        Assert.Equal(TokenService.Hash("abc"), TokenService.Hash("abc"));
        Assert.NotEqual(TokenService.Hash("abc"), TokenService.Hash("abd"));
    }
}