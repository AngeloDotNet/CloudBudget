using AutoMapper;
using CloudBudget.API.DTOs;
using CloudBudget.API.Entities;
using CloudBudget.API.Repositories.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CloudBudget.API.Controllers;

[ApiController]
[Route("api/categories")]
public class CategoriesController(ICategoryRepository repo, IMapper mapper) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll() => Ok(await repo.ListAsync());

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> Get(Guid id)
    {
        var c = await repo.GetByIdAsync(id);
        return c == null ? NotFound() : Ok(c);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateCategoryDto dto, CancellationToken ct)
    {
        if (!ModelState.IsValid)
        {
            return ValidationProblem(ModelState);
        }

        var entity = mapper.Map<Category>(dto);
        entity.Id = Guid.NewGuid();

        await repo.AddAsync(entity, ct);
        return CreatedAtAction(nameof(Get), new { id = entity.Id }, entity);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateCategoryDto dto, CancellationToken ct)
    {
        if (!ModelState.IsValid)
        {
            return ValidationProblem(ModelState);
        }

        var existing = await repo.GetByIdAsync(id, ct);
        if (existing == null)
        {
            return NotFound();
        }

        mapper.Map(dto, existing);

        try
        {
            await repo.UpdateAsync(existing, ct);
            return NoContent();
        }
        catch (DbUpdateConcurrencyException)
        {
            return Conflict(new { message = "Concurrency conflict: the resource was modified by another process. Reload and retry." });
        }
    }

    // Soft-delete che propaga alle spese correlate
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
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