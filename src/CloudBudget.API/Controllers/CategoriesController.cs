using CloudBudget.API.Entities;
using CloudBudget.API.Repositories.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace CloudBudget.API.Controllers;

[ApiController]
[Route("api/categories")]
public class CategoriesController : ControllerBase
{
    private readonly IRepository<Category, Guid> repo;

    public CategoriesController(IRepository<Category, Guid> repo) => this.repo = repo;

    [HttpGet]
    public async Task<IActionResult> GetAll() => Ok(await repo.ListAsync());

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> Get(Guid id)
    {
        var c = await repo.GetByIdAsync(id);
        return c == null ? NotFound() : Ok(c);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] Category create, CancellationToken ct)
    {
        await repo.AddAsync(create, ct);
        return CreatedAtAction(nameof(Get), new { id = create.Id }, create);
    }

    // Soft-delete che potrebbe voler propagare il soft-delete alle spese correlate:
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        // Implementazione semplice: soft-delete solo la category.
        // Se desideri propagare a Expenses, implementa metodo specifico nel repository/service
        await repo.SoftDeleteAsync(id, ct);
        return NoContent();
    }
}