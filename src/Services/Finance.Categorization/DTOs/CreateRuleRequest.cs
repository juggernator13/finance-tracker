namespace Finance.Categorization;

public record CreateRuleRequest(Guid CategoryId, string Pattern, bool IsRegex, int Priority);