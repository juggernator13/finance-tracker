using Microsoft.EntityFrameworkCore;

namespace Finance.AccountsTransactions;

public class AccountsDbContext : DbContext
{
    public AccountsDbContext(DbContextOptions<AccountsDbContext> options) : base(options) { }

    public DbSet<Account> Accounts => Set<Account>();

    public DbSet<Transaction> Transactions => Set<Transaction>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.Entity<Transaction>().Property(t => t.Amount).HasPrecision(18, 2);
        modelBuilder.Entity<Transaction>().HasIndex(t => new { t.AccountId, t.Hash }).IsUnique();
        modelBuilder.Entity<Account>().HasIndex(a => a.UserId);
    }
}
