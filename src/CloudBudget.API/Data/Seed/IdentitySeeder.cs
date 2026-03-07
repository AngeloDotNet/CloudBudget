using System.Security.Claims;
using CloudBudget.API.Entities;
using Microsoft.AspNetCore.Identity;

namespace CloudBudget.API.Data.Seed;

/// <summary>
/// Seeder per ruoli, utente admin e claim.
/// </summary>
public class IdentitySeeder(RoleManager<IdentityRole<Guid>> roleManager, UserManager<ApplicationUser> userManager, IConfiguration configuration, ILogger<IdentitySeeder> logger)
{
    public async Task SeedAsync()
    {
        try
        {
            // ruoli desiderati
            var roles = new[] { "Admin", "User" };

            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    var identityRole = new IdentityRole<Guid> { Id = Guid.NewGuid(), Name = role, NormalizedName = role.ToUpperInvariant() };
                    var result = await roleManager.CreateAsync(identityRole);

                    if (!result.Succeeded)
                    {
                        logger.LogWarning("Creazione ruolo {Role} fallita: {Errors}", role, string.Join(", ", result.Errors));
                    }
                }
            }

            // Admin user config (se presente in appsettings)
            var adminEmail = configuration["AdminUser:Email"] ?? "admin@local.test";
            var adminPassword = configuration["AdminUser:Password"] ?? "Admin123!";

            var adminRole = configuration["AdminUser:Role"] ?? "Admin";
            var existing = await userManager.FindByEmailAsync(adminEmail);

            if (existing == null)
            {
                var admin = new ApplicationUser
                {
                    Id = Guid.NewGuid(),
                    UserName = adminEmail,
                    Email = adminEmail,
                    EmailConfirmed = true,
                };

                var createRes = await userManager.CreateAsync(admin, adminPassword);

                if (!createRes.Succeeded)
                {
                    logger.LogWarning("Creazione admin fallita: {Errors}", string.Join(", ", createRes.Errors));
                }
                else
                {
                    await userManager.AddToRoleAsync(admin, adminRole);

                    // aggiungi claim utile per invio report
                    await userManager.AddClaimAsync(admin, new Claim("reports:send", "true"));
                    logger.LogInformation("Utente admin creato: {Email}", adminEmail);
                }
            }
            else
            {
                // assicurati che abbia il ruolo e il claim
                if (!await userManager.IsInRoleAsync(existing, adminRole))
                {
                    await userManager.AddToRoleAsync(existing, adminRole);
                }

                var claims = await userManager.GetClaimsAsync(existing);

                if (!claims.Any(c => c.Type == "reports:send"))
                {
                    await userManager.AddClaimAsync(existing, new Claim("reports:send", "true"));
                }
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Errore durante Identity seeding");
            throw;
        }
    }
}