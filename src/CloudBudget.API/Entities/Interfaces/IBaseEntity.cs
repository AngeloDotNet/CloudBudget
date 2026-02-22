namespace CloudBudget.API.Entities.Interfaces;

// Interfaccia non-generic per gestire audit/soft-delete indipendentemente dal tipo della PK
public interface IBaseEntity
{
    DateTime CreatedAt { get; set; }
    DateTime? ModifiedAt { get; set; }
    bool IsDeleted { get; set; }
    DateTime? DeletedAt { get; set; }
    byte[]? RowVersion { get; set; }
}