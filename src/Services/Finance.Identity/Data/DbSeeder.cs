namespace Finance.Identity;

public static class DbSeeder
{
    public static void Seed(IdentityDbContext db)
    {
        if (db.Users.Any())
        {
            return;
        }

        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = "user@example.com",
            Role = "User",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("User123!")
        };

        var admin = new User
        {
            Id = Guid.NewGuid(),
            Email = "admin@example.com",
            Role = "Admin",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin123!")
        };

        db.Users.AddRange(user, admin);
        db.SaveChanges();
    }
}
