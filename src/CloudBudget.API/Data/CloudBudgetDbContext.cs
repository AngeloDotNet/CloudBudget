using CloudBudget.API.Data.Configurations;
using CloudBudget.API.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace CloudBudget.API.Data;

//public class CloudBudgetDbContext(DbContextOptions<CloudBudgetDbContext> options) : DbContext(options)
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

    // Override SaveChanges per timestamp e gestione DeletedAt
    public override int SaveChanges() => SaveChangesAsync(default).GetAwaiter().GetResult();

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;

        foreach (var entry in ChangeTracker.Entries<BaseEntity>())
        {
            switch (entry.State)
            {
                case EntityState.Added:
                    entry.Entity.CreatedAt = now;
                    entry.Entity.ModifiedAt = null;

                    if (entry.Entity.IsDeleted && entry.Entity.DeletedAt == null)
                    {
                        entry.Entity.DeletedAt = now;
                    }

                    break;

                case EntityState.Modified:
                    entry.Entity.ModifiedAt = now;

                    // Se è stato marcato come soft-delete, imposta DeletedAt se non presente
                    var isDeleted = entry.Property(nameof(BaseEntity.IsDeleted)).CurrentValue as bool?;

                    if (isDeleted == true && entry.Entity.DeletedAt == null)
                    {
                        entry.Entity.DeletedAt = now;
                    }

                    break;
            }
        }

        return await base.SaveChangesAsync(cancellationToken);
    }
}