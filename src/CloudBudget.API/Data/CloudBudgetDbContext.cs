using CloudBudget.API.Data.Configurations;
using CloudBudget.API.Entities;
using CloudBudget.API.Entities.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace CloudBudget.API.Data;

public class CloudBudgetDbContext(DbContextOptions<CloudBudgetDbContext> options) : IdentityDbContext<ApplicationUser, ApplicationRole, Guid,
    IdentityUserClaim<Guid>, ApplicationUserRole, IdentityUserLogin<Guid>, IdentityRoleClaim<Guid>, IdentityUserToken<Guid>>(options)
{
    public DbSet<Category> Categories { get; set; }
    public DbSet<Expense> Expenses { get; set; }
    public DbSet<RefreshToken> RefreshTokens { get; set; }
    public DbSet<RevokedJwt> RevokedJwts { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Applica configurazioni esplicite
        modelBuilder.ApplyConfiguration(new CategoryConfiguration());
        modelBuilder.ApplyConfiguration(new ExpenseConfiguration());
        modelBuilder.ApplyConfiguration(new ApplicationUserRoleConfiguration());
    }

    public override int SaveChanges()
            => SaveChangesAsync(default).GetAwaiter().GetResult();

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;

        foreach (var entry in ChangeTracker.Entries<IBaseEntity>())
        {
            var entity = entry.Entity;

            if (entry.State == EntityState.Added)
            {
                entity.CreatedAt = now;
                entity.ModifiedAt = null;
                if (entity.IsDeleted && entity.DeletedAt == null)
                    entity.DeletedAt = now;
            }
            else if (entry.State == EntityState.Modified)
            {
                entity.ModifiedAt = now;
                if (entity.IsDeleted && entity.DeletedAt == null)
                    entity.DeletedAt = now;
            }
        }

        return await base.SaveChangesAsync(cancellationToken);
    }
}