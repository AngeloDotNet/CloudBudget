using System.ComponentModel.DataAnnotations;

namespace CloudBudget.API.DTOs;

public class CreateExpenseDto
{
    [Required]
    [StringLength(500)]
    public string Description { get; set; } = null!;

    [Required]
    [Range(0.01, 1000000)]
    public decimal Amount { get; set; }

    [Required]
    public DateTime Date { get; set; }

    [Required]
    public Guid CategoryId { get; set; }
}