using Microsoft.AspNetCore.Identity;

namespace CloudBudget.API.Entities;

public class ApplicationRole : IdentityRole<Guid>
{
    public ApplicationRole()
    { }

    public ApplicationRole(string roleName) : base(roleName)
    { }

    public ICollection<ApplicationUserRole> UserRoles { get; set; }
}