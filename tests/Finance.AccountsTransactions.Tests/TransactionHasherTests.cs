namespace Finance.AccountsTransactions.Tests;

public class TransactionHasherTests
{
    private static readonly Guid AccountId = Guid.NewGuid();
    private static readonly DateOnly Date = new(2026, 10, 20);

    [Fact]
    public void Same_data_gives_same_hash()
    {
        var first = TransactionHasher.Compute(AccountId, Date, -150.50m, "Coffee", 0);
        var second = TransactionHasher.Compute(AccountId, Date, -150.50m, "Coffee", 0);

        Assert.Equal(first, second);
    }

    [Fact]
    public void Different_ordinal_gives_different_hash()
    {
        var first = TransactionHasher.Compute(AccountId, Date, -150.50m, "Coffee", 0);
        var second = TransactionHasher.Compute(AccountId, Date, -150.50m, "Coffee", 1);

        Assert.NotEqual(first, second);
    }

    [Fact]
    public void Different_amount_description_date_or_account_gives_different_hash()
    {
        var original = TransactionHasher.Compute(AccountId, Date, -150.50m, "Coffee", 0);

        Assert.NotEqual(original, TransactionHasher.Compute(AccountId, Date, -151m, "Coffee", 0));
        Assert.NotEqual(original, TransactionHasher.Compute(AccountId, Date, -150.50m, "Tea", 0));
        Assert.NotEqual(original, TransactionHasher.Compute(AccountId, Date.AddDays(1), -150.50m, "Coffee", 0));
        Assert.NotEqual(original, TransactionHasher.Compute(Guid.NewGuid(), Date, -150.50m, "Coffee", 0));
    }
}