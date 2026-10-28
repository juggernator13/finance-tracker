using Finance.Common;
using Finance.Identity.DTOs;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Finance.Identity;

[ApiController]
[Route("api/users")]
[Authorize]
public class UsersController(UserService users) : ControllerBase
{
    [HttpGet("me")]
    public Task<UserDto> Me()
        => users.GetAsync(User.GetUserId());

    [HttpGet]
    [Authorize(Roles = "Admin")]
    public Task<List<UserDto>> GetAll()
        => users.GetAllAsync();

    [HttpPut("{id}/role")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> SetRole(Guid id, SetRoleRequest request)
    {
        await users.SetRoleAsync(id, request.Role);
        return NoContent();
    }
}