using System.Security.Claims;

namespace Finance.Common;

public static class ClaimsPrincipalExtensions
{
    public static Guid GetUserId(this ClaimsPrincipal user)
    {
        var _user = user.FindFirst("sub")?.Value
         ?? user.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (Guid.TryParse(_user, out Guid result))
        {
            return result;
        }
        throw new UnauthorizedAccessException("Invalid user ID");
    }
}