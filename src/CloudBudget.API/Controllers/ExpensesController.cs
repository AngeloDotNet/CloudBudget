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

    // PATCH via DTO (AutoMapper applica solo campi non-null)
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

        // Se CategoryId presente, verifica esistenza (esempio di validazione)
        if (dto.CategoryId.HasValue)
        {
            var exists = await categoryRepo.ExistsAsync(dto.CategoryId.Value, ct);
            if (!exists)
            {
                ModelState.AddModelError(nameof(dto.CategoryId), "Categoria non trovata");
                return ValidationProblem(ModelState);
            }
        }

        // Mappa (solo campi non-null, grazie al MappingProfile)
        mapper.Map(dto, entity);

        // eventuale logica aggiuntiva (es. aggiornare ModifiedAt)
        entity.ModifiedAt = DateTime.UtcNow;

        await expenseRepo.UpdateAsync(entity, ct);
        return NoContent();
    }
}