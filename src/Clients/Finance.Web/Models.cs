namespace Finance.Web;

public record LoginResult(string AccessToken);

public record AccountDto(Guid Id, string Name, string Currency);

public record TransactionDto(Guid Id, Guid AccountId, DateOnly Date, decimal Amount, string Description);

public record ImportBatchDto(Guid Id, Guid AccountId, string Format, string Status,
    int TotalRows, int Created, int Skipped, string? Error, DateTime CreatedAt);