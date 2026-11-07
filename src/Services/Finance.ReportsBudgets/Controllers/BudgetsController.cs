using Finance.Common;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Finance.ReportsBudgets;

[ApiController]
[Route("api/budgets")]
[Authorize]
public class BudgetsController(BudgetService budgets) : ControllerBase
{
    [HttpGet]
    public async Task<List<BudgetDto>> GetAll()
    {
        return await budgets.GetAllAsync(User.GetUserId());
    }

    [HttpPost]
    public async Task<BudgetDto> Create(CreateBudgetRequest request)
    {
        return await budgets.CreateAsync(User.GetUserId(), request.CategoryId, request.CategoryName, request.Limit);
    }

    [HttpPut("{id:guid}")]
    public async Task<BudgetDto> Update(Guid id, UpdateBudgetRequest request)
    {
        return await budgets.UpdateAsync(User.GetUserId(), id, request.Limit);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        await budgets.DeleteAsync(User.GetUserId(), id);
        return NoContent();
    }
}