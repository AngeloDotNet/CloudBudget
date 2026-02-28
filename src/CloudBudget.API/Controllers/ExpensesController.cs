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
    public async Task<IActionResult> Create([FromBody] CreateExpenseDto dto, CancellationToken ct)
    {
        if (!ModelState.IsValid)
        {
            return ValidationProblem(ModelState);
        }

        // Validate category existence
        var catExists = await categoryRepo.ExistsAsync(dto.CategoryId, ct);

        if (!catExists)
        {
            ModelState.AddModelError(nameof(dto.CategoryId), "Categoria non trovata");
            return ValidationProblem(ModelState);
        }

        var entity = mapper.Map<Expense>(dto);
        entity.Id = Guid.NewGuid();

        await expenseRepo.AddAsync(entity, ct);
        return CreatedAtAction(nameof(Get), new { id = entity.Id }, entity);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateExpenseDto dto, CancellationToken ct)
    {
        if (!ModelState.IsValid)
        {
            return ValidationProblem(ModelState);
        }

        var existing = await expenseRepo.GetByIdAsync(id, ct);
        if (existing == null)
        {
            return NotFound();
        }

        // Validate category
        var catExists = await categoryRepo.ExistsAsync(dto.CategoryId, ct);
        if (!catExists)
        {
            ModelState.AddModelError(nameof(dto.CategoryId), "Categoria non trovata");
            return ValidationProblem(ModelState);
        }

        mapper.Map(dto, existing);

        try
        {
            await expenseRepo.UpdateAsync(existing, ct);
            return NoContent();
        }
        catch (DbUpdateConcurrencyException)
        {
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