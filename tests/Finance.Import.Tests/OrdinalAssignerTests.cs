namespace Finance.Import.Tests;

public class OrdinalAssignerTests
{
    private static readonly DateOnly D = new(2026, 10, 21);

    [Fact]
    public void SameRows_GetIncreasingOrdinals()
    {
        var rows = new List<NormalizedRow>
        {
            new(D, -150m, "Кофе"),
            new(D, -150m, "Кофе"),
            new(D, -150m, "Кофе")
        };

        var result = OrdinalAssigner.Assign(rows);

        Assert.Equal(new[] { 0, 1, 2 }, result.Select(r => r.Ordinal));
    }

    [Fact]
    public void DifferentRows_AllGetZero()
    {
        var rows = new List<NormalizedRow>
        {
            new(D, -150m, "Кофе"),
            new(D, -200m, "Кофе"),
            new(D, -150m, "Чай")
        };

        var result = OrdinalAssigner.Assign(rows);

        Assert.All(result, r => Assert.Equal(0, r.Ordinal));
    }

    [Fact]
    public void SameDescriptionOnDifferentDays_AreDifferentKeys()
    {
        var rows = new List<NormalizedRow>
        {
            new(D, -150m, "Кофе"),
            new(D.AddDays(1), -150m, "Кофе")
        };

        var result = OrdinalAssigner.Assign(rows);

        Assert.All(result, r => Assert.Equal(0, r.Ordinal));
    }
}