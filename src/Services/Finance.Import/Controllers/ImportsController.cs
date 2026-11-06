using System.Text;

using Finance.Common;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Finance.Import;

[ApiController]
[Route("api/imports")]
[Authorize]
public class ImportsController(ImportService imports) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> Start([FromForm] Guid accountId, [FromForm] string? format, IFormFile file)
    {
        if (file.Length > 5 * 1024 * 1024)
        {
            throw new ArgumentException("File is too large (max 5 MB)");
        }

        using var reader = new StreamReader(file.OpenReadStream(), Encoding.UTF8);
        string text = await reader.ReadToEndAsync();

        var batch = await imports.StartAsync(User.GetUserId(), accountId, format, text);
        return Accepted(batch);
    }

    [HttpGet]
    public async Task<List<ImportBatchDto>> GetAll()
    {
        return await imports.GetAllAsync(User.GetUserId());
    }

    [HttpGet("{id}")]
    public async Task<ImportBatchDto> Get(Guid id)
    {
        return await imports.GetAsync(User.GetUserId(), id);
    }

    [HttpGet("formats")]
    public List<string> GetFormats()
    {
        return imports.GetFormats();
    }
}