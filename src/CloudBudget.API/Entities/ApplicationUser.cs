using Microsoft.AspNetCore.Identity;

namespace CloudBudget.API.Entities;

// User identity basato su Guid
public class ApplicationUser : IdentityUser<Guid>
{
    // estendi con proprietà personalizzate se serve (es. FullName)
    public string? FullName { get; set; }
    public ICollection<ApplicationUserRole> UserRoles { get; set; } = [];
}