namespace CloudBudget.API.Entities.Interfaces;

// Interfaccia non-generic per poter iterare ChangeTracker.Entries<IBaseEntity>()
public interface IBaseEntity
{
    DateTime CreatedAt { get; set; }
    DateTime? ModifiedAt { get; set; }
    bool IsDeleted { get; set; }
    DateTime? DeletedAt { get; set; }
    byte[]? RowVersion { get; set; }
}