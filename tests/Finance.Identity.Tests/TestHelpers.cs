using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace Finance.Identity.Tests;

public static class TestHelpers
{
    // новая пустая база в памяти для каждого теста
    public static IdentityDbContext CreateDb()
    {
        var options = new DbContextOptionsBuilder<IdentityDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new IdentityDbContext(options);
    }

    // TokenService с тестовыми настройками JWT
    public static TokenService CreateTokens()
    {
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Jwt:Issuer"] = "test",
                ["Jwt:Audience"] = "test",
                ["Jwt:SigningKey"] = "test_key_test_key_test_key_test_key_123"
            })
            .Build();
        return new TokenService(config);
    }
}