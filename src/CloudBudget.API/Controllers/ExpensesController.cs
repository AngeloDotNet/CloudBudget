using AutoMapper;
using CloudBudget.API.DTOs;
using CloudBudget.API.Entities;
using CloudBudget.API.Repositories.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CloudBudget.API.Controllers;

[ApiController]
[Route("api/expenses")]
public class ExpensesController(IRepository<Expense, Guid> expenseRepo, IRepository<Category, Guid> categoryRepo, IMapper mapper) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll()
            => Ok(await expenseRepo.ListAsync());

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> Get(Guid id)
    {
        var e = await expenseRepo.GetByIdAsync(id);
        return e == null ? NotFound() : Ok(e);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] Expense create, CancellationToken ct)
    {
        if (create == null)
        {
            return BadRequest();
        }

        await expenseRepo.AddAsync(create, ct);
        return CreatedAtAction(nameof(Get), new { id = create.Id }, create);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] Expense update, CancellationToken ct)
    {
        if (update == null)
        {
            return BadRequest();
        }

        var exists = await expenseRepo.ExistsAsync(id, ct);
        if (!exists)
        {
            return NotFound();
        }

        update.Id = id;

        try
        {
            await expenseRepo.UpdateAsync(update, ct);
            return NoContent();
        }
        catch (DbUpdateConcurrencyException)
        {
            // Conflitto di concorrenza: restituire 409 con info minima
            return Conflict(new { message = "Concurrency conflict: the resource was modified by another process. Reload and retry." });
        }
    }

    // PATCH tramite DTO + AutoMapper (map only non-null props)
    [HttpPatch("{id:guid}")]
    public async Task<IActionResult> Patch(Guid id, [FromBody] ExpensePatchDto dto, CancellationToken ct)
    {
        if (dto == null)
        {
            return BadRequest();
        }

        var entity = await expenseRepo.GetByIdAsync(id, ct);
        if (entity == null)
        {
            return NotFound();
        }

        if (dto.CategoryId.HasValue)
        {
            var exists = await categoryRepo.ExistsAsync(dto.CategoryId.Value, ct);
            if (!exists)
            {
                ModelState.AddModelError(nameof(dto.CategoryId), "Categoria non trovata");
                return ValidationProblem(ModelState);
            }
        }

        mapper.Map(dto, entity);

        try
        {
            await expenseRepo.UpdateAsync(entity, ct);
            return NoContent();
        }
        catch (DbUpdateConcurrencyException)
        {
            return Conflict(new { message = "Concurrency conflict: the resource was modified by another process. Reload and retry." });
        }
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        await expenseRepo.SoftDeleteAsync(id, ct);
        return NoContent();
    }
}