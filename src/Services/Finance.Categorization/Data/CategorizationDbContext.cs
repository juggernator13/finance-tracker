using Microsoft.EntityFrameworkCore;

namespace Finance.Categorization;

public class CategorizationDbContext : DbContext
{
    public CategorizationDbContext(DbContextOptions<CategorizationDbContext> options) : base(options) { }

    public DbSet<Category> Categories => Set<Category>();

    public DbSet<Rule> Rules => Set<Rule>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.Entity<Category>().HasIndex(a => a.UserId);
        modelBuilder.Entity<Rule>().HasIndex(a => a.UserId);
    }
}