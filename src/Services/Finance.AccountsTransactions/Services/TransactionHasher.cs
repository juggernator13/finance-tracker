using System.Globalization;
using System.Security.Cryptography;
using System.Text;

namespace Finance.AccountsTransactions;

public static class TransactionHasher
{
    public static string Compute(Guid accountId, DateOnly date, decimal amount, string description, int ordinal)
    {
        string rawString = string.Create(CultureInfo.InvariantCulture, $"{accountId}|{date}|{amount}|{description}|{ordinal}");

        return Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(rawString)));
    }
}