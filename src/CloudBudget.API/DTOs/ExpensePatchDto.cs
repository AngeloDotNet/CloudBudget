namespace CloudBudget.API.DTOs;

public class ExpensePatchDto
{
    // Proprietà tutte nullable => se null = non modificare
    public string? Description { get; set; }
    public decimal? Amount { get; set; }
    public DateTime? Date { get; set; }
    public Guid? CategoryId { get; set; }
}