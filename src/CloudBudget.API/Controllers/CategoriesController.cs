using CloudBudget.API.Entities;
using CloudBudget.API.Repositories.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CloudBudget.API.Controllers;

[ApiController]
[Route("api/categories")]
public class CategoriesController(ICategoryRepository repo) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAllAsync() => Ok(await repo.ListAsync());

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetAsync(Guid id)
    {
        var c = await repo.GetByIdAsync(id);
        return c == null ? NotFound() : Ok(c);
    }

    [HttpPost]
    public async Task<IActionResult> CreateAsync([FromBody] Category create, CancellationToken ct)
    {
        if (create == null)
        {
            return BadRequest();
        }

        await repo.AddAsync(create, ct);
        return CreatedAtAction(nameof(GetAsync), new { id = create.Id }, create);
    }

    // Soft-delete che propaga alle spese correlate
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteAsync(Guid id, CancellationToken ct)
    {
        try
        {
            await repo.SoftDeleteWithChildrenAsync(id, ct);
            return NoContent();
        }
        catch (DbUpdateConcurrencyException)
        {
            return Conflict(new { message = "Concurrency conflict: the resource was modified by another process. Reload and retry." });
        }
    }
}
