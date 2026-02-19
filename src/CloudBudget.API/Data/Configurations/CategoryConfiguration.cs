using CloudBudget.API.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CloudBudget.API.Data.Configurations;

public class CategoryConfiguration : IEntityTypeConfiguration<Category>
{
    public void Configure(EntityTypeBuilder<Category> builder)
    {
        builder.ToTable("Categories");

        builder.HasKey(c => c.Id);

        builder.Property(c => c.Name)
            .IsRequired()
            .HasMaxLength(200);

        // RowVersion / concurrency
        builder.Property(c => c.RowVersion)
            .IsRowVersion()
            .IsConcurrencyToken();

        // Relation 1:N explicit: Category.HasMany -> Expense.WithOne
        builder.HasMany(c => c.Expenses)
            .WithOne(e => e.Category)
            .HasForeignKey(e => e.CategoryId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Restrict); // evita cascade delete fisico

        // Global query filter per soft-delete
        builder.HasQueryFilter(c => !c.IsDeleted);

        // Index utile
        builder.HasIndex(c => c.Name);
    }
}