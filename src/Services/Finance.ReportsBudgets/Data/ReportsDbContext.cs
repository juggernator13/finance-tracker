using Microsoft.EntityFrameworkCore;

namespace Finance.ReportsBudgets;

public class ReportsDbContext : DbContext
{
    public ReportsDbContext(DbContextOptions<ReportsDbContext> option) : base(option) { }

    public DbSet<Budget> Budgets => Set<Budget>();
    public DbSet<MonthlyTotal> MonthlyTotals => Set<MonthlyTotal>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.Entity<MonthlyTotal>().HasIndex(t => new { t.UserId, t.CategoryId, t.Year, t.Month }).IsUnique();
        modelBuilder.Entity<MonthlyTotal>().Property(t => t.Expense).HasPrecision(18, 2);
        modelBuilder.Entity<MonthlyTotal>().Property(t => t.Income).HasPrecision(18, 2);
        modelBuilder.Entity<Budget>().Property(b => b.Limit).HasPrecision(18, 2);
        modelBuilder.Entity<Budget>().HasIndex(b => new { b.UserId, b.CategoryId }).IsUnique();
    }
}