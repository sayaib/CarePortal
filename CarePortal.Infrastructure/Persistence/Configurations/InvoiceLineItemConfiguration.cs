using CarePortal.Domain.Billing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CarePortal.Infrastructure.Persistence.Configurations;

public sealed class InvoiceLineItemConfiguration : IEntityTypeConfiguration<InvoiceLineItem>
{
    public void Configure(EntityTypeBuilder<InvoiceLineItem> builder)
    {
        builder.ToTable("invoice_line_items");

        builder.HasKey(lineItem => lineItem.Id);

        builder.Property(lineItem => lineItem.Id)
            .HasColumnName("id")
            .ValueGeneratedNever();

        builder.Property(lineItem => lineItem.InvoiceId)
            .HasColumnName("invoice_id")
            .IsRequired();

        builder.Property(lineItem => lineItem.Description)
            .HasColumnName("description")
            .HasMaxLength(500)
            .IsRequired();

        builder.Property(lineItem => lineItem.DueDate)
            .HasColumnName("due_date")
            .IsRequired();

        builder.Property(lineItem => lineItem.Amount)
            .HasColumnName("amount")
            .HasPrecision(18, 2)
            .IsRequired();

        builder.Property(lineItem => lineItem.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.HasIndex(lineItem => lineItem.InvoiceId);

        builder.HasMany(lineItem => lineItem.LedgerEntries)
            .WithOne(entry => entry.LineItem)
            .HasForeignKey(entry => entry.LineItemId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Navigation(lineItem => lineItem.LedgerEntries)
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.Ignore(lineItem => lineItem.DomainEvents);
    }
}
