using CloudBudget.API.Entities.Interfaces;

namespace BudgetApp.Models
{
    // Entità base generica: contiene l'Id di tipo TId + campi di audit ereditati da IBaseEntity
    public abstract class BaseEntity<TId> : IBaseEntity
    {
        public TId Id { get; set; } = default!;

        public DateTime CreatedAt { get; set; }
        public DateTime? ModifiedAt { get; set; }

        // Soft-delete
        public bool IsDeleted { get; set; } = false;
        public DateTime? DeletedAt { get; set; }

        // Concurrency token
        public byte[]? RowVersion { get; set; }
    }
}