using System.Text;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;

namespace Finance.Common;

public static class JwtExtensions
{
    public static void AddFinanceJwt(this IServiceCollection services, IConfiguration config)
    {
        var key = config["Jwt:SigningKey"]!;

        services.AddAuthentication("Bearer").AddJwtBearer(options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidIssuer = config["Jwt:Issuer"],
                ValidAudience = config["Jwt:Audience"],
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key)),
                RoleClaimType = "role"
            };
        });

        services.AddAuthorization();
    }
}