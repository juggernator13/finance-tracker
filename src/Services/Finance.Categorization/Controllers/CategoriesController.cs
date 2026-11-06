using Finance.Common;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Finance.Categorization;

[ApiController]
[Route("api/categories")]
[Authorize]
public class CategoriesController(CategoryService categories) : ControllerBase
{
    [HttpGet]
    public async Task<List<CategoryDto>> GetAll()
    {
        return await categories.GetAllAsync(User.GetUserId());
    }

    [HttpPost]
    public async Task<CategoryDto> Create(CreateCategoryRequest request)
    {
        return await categories.CreateAsync(User.GetUserId(), request.Name);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        await categories.DeleteAsync(User.GetUserId(), id);
        return NoContent();
    }
}