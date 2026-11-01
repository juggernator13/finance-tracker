namespace Finance.Import;

public record ImportBatchDto(Guid Id, Guid AccountId, string Format, string Status, int TotalRows, int Created, int Skipped, string? Error, DateTime CreatedAt);