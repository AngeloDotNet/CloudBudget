using CloudBudget.API.Entities.Interfaces;

namespace CloudBudget.API.Entities;

// Entità base generica: Id tipizzato + campi di audit/soft-delete
public abstract class BaseEntity<TId> : IBaseEntity
{
    public TId Id { get; set; } = default!;

    public DateTime CreatedAt { get; set; }
    public DateTime? ModifiedAt { get; set; }

    public bool IsDeleted { get; set; } = false;
    public DateTime? DeletedAt { get; set; }

    public byte[]? RowVersion { get; set; }
}