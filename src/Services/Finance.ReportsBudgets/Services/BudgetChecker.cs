namespace Finance.ReportsBudgets;

public static class BudgetChecker
{
    public static List<(Budget Budget, AggregateChange Change)> FindExceeded(
    IEnumerable<Budget> budgets, IEnumerable<AggregateChange> changes)
    {
        var result = new List<(Budget Budget, AggregateChange Change)>();

        foreach (var change in changes)
        {
            var budget = budgets.FirstOrDefault(b => b.CategoryId == change.CategoryId);

            if (budget == null)
            {
                continue;
            }

            if (change.Before <= budget.Limit && change.After > budget.Limit)
            {
                result.Add((budget, change));
            }
        }
        return result;
    }
}
