using CloudBudget.API.Data;
using CloudBudget.API.Entities;
using CloudBudget.API.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace CloudBudget.API.Repositories;

public class EfRepository<TEntity, TId> : IRepository<TEntity, TId>
        where TEntity : BaseEntity<TId>
{
    protected readonly CloudBudgetDbContext _db;
    protected readonly DbSet<TEntity> _set;

    public EfRepository(CloudBudgetDbContext db)
    {
        _db = db;
        _set = _db.Set<TEntity>();
    }

    public async Task<TEntity?> GetByIdAsync(TId id, CancellationToken ct = default)
    {
        return await _set.FindAsync(new object[] { id }, ct);
    }

    public async Task<IEnumerable<TEntity>> ListAsync(CancellationToken ct = default)
        => await _set.ToListAsync(ct);

    public async Task AddAsync(TEntity entity, CancellationToken ct = default)
    {
        await _set.AddAsync(entity, ct);
        await _db.SaveChangesAsync(ct);
    }

    public async Task UpdateAsync(TEntity entity, CancellationToken ct = default)
    {
        _set.Update(entity);
        // Non catturo DbUpdateConcurrencyException qui: lo gestisco a livello superiore (controller/service)
        await _db.SaveChangesAsync(ct);
    }

    public async Task SoftDeleteAsync(TId id, CancellationToken ct = default)
    {
        var entity = await GetByIdAsync(id, ct);
        if (entity == null)
        {
            return;
        }

        entity.IsDeleted = true;
        if (entity.DeletedAt == null)
        {
            entity.DeletedAt = DateTime.UtcNow;
        }

        _set.Update(entity);
        await _db.SaveChangesAsync(ct);
    }

    public async Task<bool> ExistsAsync(TId id, CancellationToken ct = default)
        => (await GetByIdAsync(id, ct)) != null;
}