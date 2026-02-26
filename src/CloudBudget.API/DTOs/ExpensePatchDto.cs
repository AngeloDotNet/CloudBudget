namespace CloudBudget.API.DTOs;

// DTO per PATCH: tutte le proprietà nullable -> null significa "non aggiornare"
public class ExpensePatchDto
{
    public string? Description { get; set; }
    public decimal? Amount { get; set; }
    public DateTime? Date { get; set; }
    public Guid? CategoryId { get; set; }
}