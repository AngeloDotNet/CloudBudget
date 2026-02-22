using AutoMapper;
using CloudBudget.API.DTOs;
using CloudBudget.API.Entities;
using CloudBudget.API.Repositories.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace CloudBudget.API.Controllers;

[ApiController]
[Route("api/expenses")]
public class ExpensesController(IRepository<Expense, Guid> expenseRepo, IRepository<Category, Guid> categoryRepo, IMapper mapper) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAllAsync()
        => Ok(await expenseRepo.ListAsync());

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetAsync(Guid id)
    {
        var e = await expenseRepo.GetByIdAsync(id);
        return e == null ? NotFound() : Ok(e);
    }

    [HttpPost]
    public async Task<IActionResult> CreateAsync([FromBody] Expense create, CancellationToken ct)
    {
        // semplice create - in produzione separare DTO di input
        await expenseRepo.AddAsync(create, ct);
        return CreatedAtAction(nameof(GetAsync), new { id = create.Id }, create);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> UpdateAsync(Guid id, [FromBody] Expense update, CancellationToken ct)
    {
        var exists = await expenseRepo.ExistsAsync(id, ct);
        if (!exists)
        {
            return NotFound();
        }

        update.Id = id;
        await expenseRepo.UpdateAsync(update, ct);
        return NoContent();
    }

    // PATCH tramite DTO + AutoMapper (map only non-null props)
    [HttpPatch("{id:guid}")]
    public async Task<IActionResult> PatchAsync(Guid id, [FromBody] ExpensePatchDto dto, CancellationToken ct)
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

        // eventuale gestione ModifiedAt avviene in SaveChangesAsync del DbContext
        await expenseRepo.UpdateAsync(entity, ct);
        return NoContent();
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteAsync(Guid id, CancellationToken ct)
    {
        await expenseRepo.SoftDeleteAsync(id, ct);
        return NoContent();
    }
}