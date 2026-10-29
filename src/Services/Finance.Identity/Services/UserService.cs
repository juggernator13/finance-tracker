using Microsoft.EntityFrameworkCore;

namespace Finance.Identity;

public class UserService(IdentityDbContext db)
{
    public async Task<UserDto> GetAsync(Guid id)
    {
        var user = await db.Users.FirstOrDefaultAsync(u => u.Id == id);

        if (user == null)
        {
            throw new KeyNotFoundException("User not found");
        }

        return new UserDto(user.Id, user.Email, user.Role);
    }

    public async Task<List<UserDto>> GetAllAsync()
    {
        return await db.Users
            .Select(u => new UserDto(u.Id, u.Email, u.Role))
            .ToListAsync();
    }

    public async Task SetRoleAsync(Guid id, string role)
    {
        if (role != "User" && role != "Admin")
            throw new ArgumentException("Role must be User or Admin");

        var user = await db.Users.FirstOrDefaultAsync(u => u.Id == id);

        if (user == null)
            throw new KeyNotFoundException("User not found");

        user.Role = role;
        await db.SaveChangesAsync();
    }
}
