using System.Security.Claims;

namespace Finance.Common.Tests;

public class ClaimsPrincipalExtensionsTests
{
    [Fact]
    public void GetUserId_returns_guid_from_sub_claim()
    {
        var id = Guid.NewGuid();
        var identity = new ClaimsIdentity(new[] { new Claim("sub", id.ToString()) }, "test");
        var user = new ClaimsPrincipal(identity);

        var result = user.GetUserId();

        Assert.Equal(id, result);
    }

    [Fact]
    public void GetUserId_throws_when_claim_is_missing()
    {
        var user = new ClaimsPrincipal(new ClaimsIdentity("test"));

        Assert.Throws<UnauthorizedAccessException>(() => user.GetUserId());
    }
}