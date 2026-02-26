namespace CloudBudget.API.Entities;

public class Category : BaseEntity<Guid>
{
    public Category()
    {
        // genera Id lato application; se preferisci DB-generated, rimuovi questa riga
        Id = Guid.NewGuid();
    }

    public string Name { get; set; } = null!;

    // Navigation: one -> many
    public ICollection<Expense> Expenses { get; set; } = new List<Expense>();
}