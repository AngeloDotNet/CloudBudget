namespace CloudBudget.API.Entities;

public class Category : BaseEntity<Guid>
{
    // opzione: generare l'id lato client
    public Category() => Id = Guid.NewGuid();

    public string Name { get; set; } = null!;
    public ICollection<Expense> Expenses { get; set; } = [];
}