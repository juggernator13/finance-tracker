namespace Finance.Categorization.Tests;

public class RuleMatcherTests
{
    private static Rule R(string pattern, int priority = 10, bool isRegex = false, Guid? userId = null)
    {
        return new Rule
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            CategoryId = Guid.NewGuid(),
            Pattern = pattern,
            IsRegex = isRegex,
            Priority = priority
        };
    }

    [Fact]
    public void Match_FindsRuleBySubstring()
    {
        var coffee = R("кофе");
        var taxi = R("такси");

        var result = RuleMatcher.Match("Кофе в центре", new[] { taxi, coffee });

        Assert.Same(coffee, result);
    }

    [Fact]
    public void Match_IgnoresCase()
    {
        var coffee = R("кофе");

        var result = RuleMatcher.Match("КОФЕ", new[] { coffee });

        Assert.Same(coffee, result);
    }

    [Fact]
    public void Match_HigherPriorityWins()
    {
        var low = R("кофе", priority: 10);
        var high = R("кофе", priority: 100);

        var result = RuleMatcher.Match("Кофе", new[] { low, high });

        Assert.Same(high, result);
    }

    [Fact]
    public void Match_SamePriority_PersonalRuleBeatsShared()
    {
        var shared = R("кофе", priority: 10, userId: null);
        var personal = R("кофе", priority: 10, userId: Guid.NewGuid());

        var result = RuleMatcher.Match("Кофе", new[] { shared, personal });

        Assert.Same(personal, result);
    }

    [Fact]
    public void Match_RegexRuleWorks()
    {
        var uber = R("^uber.*trip$", isRegex: true);

        var result = RuleMatcher.Match("UBER city trip", new[] { uber });

        Assert.Same(uber, result);
    }

    [Fact]
    public void Match_InvalidRegex_IsSkipped_NextRuleStillWorks()
    {
        var broken = R("[abc", priority: 100, isRegex: true);
        var coffee = R("кофе", priority: 10);

        var result = RuleMatcher.Match("Кофе", new[] { broken, coffee });

        Assert.Same(coffee, result);
    }

    [Fact]
    public void Match_PlainPatternWithRegexSymbols_IsNotTreatedAsRegex()
    {
        var plain = R("c++");

        var result = RuleMatcher.Match("курс c++ для начинающих", new[] { plain });

        Assert.Same(plain, result);
    }

    [Fact]
    public void Match_NothingMatches_ReturnsNull()
    {
        var result = RuleMatcher.Match("абракадабра", new[] { R("кофе"), R("такси") });

        Assert.Null(result);
    }

    [Fact]
    public void Match_EmptyRules_ReturnsNull()
    {
        var result = RuleMatcher.Match("Кофе", new List<Rule>());

        Assert.Null(result);
    }
}