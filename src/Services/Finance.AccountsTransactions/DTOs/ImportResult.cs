using Finance.Contracts;
namespace Finance.AccountsTransactions;

public record ImportResult(int Created, int Skipped, List<TransactionItem> Items);