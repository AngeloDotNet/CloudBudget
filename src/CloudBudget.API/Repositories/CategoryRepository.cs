using CloudBudget.API.Data;
using CloudBudget.API.Entities;
using CloudBudget.API.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace CloudBudget.API.Repositories;

public class CategoryRepository(CloudBudgetDbContext db) : EfRepository<Category, Guid>(db), ICategoryRepository
{

    /// <summary>
    /// Soft-delete della category e propagazione IsDeleted= true alle Expenses correlate.
    /// </summary>
    public async Task SoftDeleteWithChildrenAsync(Guid id, CancellationToken ct = default)
    {
        var category = await _db.Categories
            .Include(c => c.Expenses)
            .FirstOrDefaultAsync(c => c.Id.Equals(id), ct);

        if (category == null)
        {
            return;
        }

        var now = DateTime.UtcNow;
        category.IsDeleted = true;
        if (category.DeletedAt == null)
        {
            category.DeletedAt = now;
        }

        foreach (var e in category.Expenses)
        {
            e.IsDeleted = true;
            if (e.DeletedAt == null)
            {
                e.DeletedAt = now;
            }

            _db.Entry(e).State = EntityState.Modified;
        }

        _db.Entry(category).State = EntityState.Modified;
        await _db.SaveChangesAsync(ct);
    }
}