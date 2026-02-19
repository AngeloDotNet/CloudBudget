using System.ComponentModel.DataAnnotations;

namespace CloudBudget.API.DTOs;

public class UpdateCategoryDto
{
    [Required]
    [StringLength(200, MinimumLength = 1)]
    public string Name { get; set; } = null!;
}