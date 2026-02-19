using CloudBudget.API.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CloudBudget.API.Data.Configurations;

public class ExpenseConfiguration : IEntityTypeConfiguration<Expense>
{
    public void Configure(EntityTypeBuilder<Expense> builder)
    {
        builder.ToTable("Expenses");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.Description)
            .IsRequired()
            .HasMaxLength(500);

        // Precisione per l'importo
        builder.Property(e => e.Amount)
            .HasPrecision(12, 2)
            .IsRequired();

        builder.Property(e => e.Date)
            .IsRequired();

        builder.Property(e => e.RowVersion)
            .IsRowVersion()
            .IsConcurrencyToken();

        // Query filter per soft-delete
        builder.HasQueryFilter(e => !e.IsDeleted);

        // Indice su date e categoria (esempio per report/filtri)
        builder.HasIndex(e => new { e.Date, e.CategoryId });
    }
}