using BudgetApp.Models;

namespace CloudBudget.API.Entities;

public class Category : BaseEntity<Guid>
{
    public string Name { get; set; } = null!;
    public ICollection<Expense> Expenses { get; set; } = [];
}