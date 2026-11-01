namespace Finance.Import;

public record ImportItem(DateOnly Date, decimal Amount, string Description, int Ordinal);