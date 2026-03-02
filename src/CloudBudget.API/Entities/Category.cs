namespace CloudBudget.API.Entities;

public class Category : BaseEntity<Guid>
{
    public Category()
    {
        Id = Guid.NewGuid();
    }

    public string Name { get; set; } = null!;

    public ICollection<Expense> Expenses { get; set; } = [];
}
