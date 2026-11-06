using Finance.Common;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Finance.Categorization;

[ApiController]
[Route("api/rules")]
[Authorize]
public class RulesController(RuleService rules) : ControllerBase
{
    [HttpGet]
    public async Task<List<RuleDto>> GetAll()
    {
        return await rules.GetAllAsync(User.GetUserId());
    }

    [HttpPost]
    public async Task<RuleDto> Create(CreateRuleRequest request)
    {
        return await rules.CreateAsync(
            User.GetUserId(), request.CategoryId, request.Pattern, request.IsRegex, request.Priority);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        await rules.DeleteAsync(User.GetUserId(), id);
        return NoContent();
    }

    [HttpPost("preview")]
    public async Task<PreviewResult> Preview(PreviewRequest request)
    {
        return await rules.PreviewAsync(User.GetUserId(), request.Description);
    }
}