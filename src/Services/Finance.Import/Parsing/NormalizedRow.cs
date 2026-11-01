namespace Finance.Import;

public record NormalizedRow(DateOnly Date, decimal Amount, string Description, int Ordinal = 0);