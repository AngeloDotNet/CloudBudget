namespace CloudBudget.API.DTOs;

public class ExpensePatchDto
{
    public string? Description { get; set; }
    public decimal? Amount { get; set; }
    public DateTime? Date { get; set; }
    public Guid? CategoryId { get; set; }
}
