namespace CloudBudget.API.Entities;

public abstract class BaseEntity
{
    public Guid Id { get; set; } = Guid.NewGuid();

    // Timestamp di creazione / modifica
    public DateTime CreatedAt { get; set; }
    public DateTime? ModifiedAt { get; set; }

    // Soft-delete
    public bool IsDeleted { get; set; } = false;
    public DateTime? DeletedAt { get; set; }

    // Concurrency token (opzionale ma consigliato)
    public byte[]? RowVersion { get; set; }
}