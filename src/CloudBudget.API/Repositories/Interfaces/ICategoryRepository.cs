using CloudBudget.API.Entities;

namespace CloudBudget.API.Repositories.Interfaces;

public interface ICategoryRepository : IRepository<Category, Guid>
{
    /// <summary>
    /// Soft-delete della category con propagazione del soft-delete alle expenses correlate.
    /// </summary>
    Task SoftDeleteWithChildrenAsync(Guid id, CancellationToken ct = default);
}