using Finance.Common;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Finance.ReportsBudgets;

[ApiController]
[Route("api/reports")]
[Authorize]
public class ReportsController(ReportService reports) : ControllerBase
{
    [HttpGet("by-category")]
    public async Task<List<CategoryReportItem>> ByCategory(int? year, int? month)
    {
        var now = DateTime.UtcNow;
        return await reports.GetByCategoryAsync(User.GetUserId(), year ?? now.Year, month ?? now.Month);
    }

    [HttpGet("monthly")]
    public async Task<List<MonthlyReportItem>> Monthly(int? year)
    {
        var now = DateTime.UtcNow;
        return await reports.GetMonthlyAsync(User.GetUserId(), year ?? now.Year);
    }
}