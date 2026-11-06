using System.Text.RegularExpressions;

namespace Finance.Categorization;

public static class RuleMatcher
{
    public static Rule? Match(string description, IEnumerable<Rule> rules)
    {
        var sortedRules = rules
            .OrderByDescending(r => r.Priority)
            .ThenByDescending(r => r.UserId.HasValue);

        foreach (var rule in sortedRules)
        {
            if (!rule.IsRegex)
            {
                if (description.Contains(rule.Pattern, StringComparison.OrdinalIgnoreCase))
                {
                    return rule;
                }
            }

            try
            {
                if (Regex.IsMatch(description, rule.Pattern, RegexOptions.IgnoreCase, TimeSpan.FromMilliseconds(100)))
                {
                    return rule;
                }
            }
            catch (ArgumentException) { }
            catch (RegexMatchTimeoutException) { }
        }

        return null;
    }
}