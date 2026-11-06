namespace Finance.Categorization;

public record RuleDto(Guid Id, Guid CategoryId, string Pattern, bool IsRegex, int Priority, bool IsShared);