namespace CloudBudget.API.Entities;

public class Expense : BaseEntity<Guid>
{
    public Expense()
    {
        Id = Guid.NewGuid();
    }

    public string Description { get; set; } = null!;
    public decimal Amount { get; set; }
    public DateTime Date { get; set; }

    public Guid CategoryId { get; set; }
    public Category Category { get; set; } = null!;
}
