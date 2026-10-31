using Microsoft.EntityFrameworkCore;

namespace Finance.AccountsTransactions.Tests;

public static class TestHelpers
{
    public static AccountsDbContext CreateDb()
    {
        var options = new DbContextOptionsBuilder<AccountsDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new AccountsDbContext(options);
    }

    public static CreateTransactionRequest Request(string date = "2026-10-20", decimal amount = -150.50m, string description = "Coffee")
        => new(DateOnly.Parse(date), amount, description);
}