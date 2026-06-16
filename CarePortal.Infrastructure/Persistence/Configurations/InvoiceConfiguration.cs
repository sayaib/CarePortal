using CarePortal.Domain.Billing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CarePortal.Infrastructure.Persistence.Configurations;

public sealed class InvoiceConfiguration : IEntityTypeConfiguration<Invoice>
{
    public void Configure(EntityTypeBuilder<Invoice> builder)
    {
        builder.ToTable("invoices");

        builder.HasKey(invoice => invoice.Id);

        builder.Property(invoice => invoice.Id)
            .HasColumnName("id")
            .ValueGeneratedNever();

        builder.Property(invoice => invoice.Reference)
            .HasColumnName("reference")
            .HasMaxLength(50)
            .IsRequired();

        builder.HasIndex(invoice => invoice.Reference)
            .IsUnique();

        builder.Property(invoice => invoice.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.HasMany(invoice => invoice.LineItems)
            .WithOne(lineItem => lineItem.Invoice)
            .HasForeignKey(lineItem => lineItem.InvoiceId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(invoice => invoice.LedgerEntries)
            .WithOne(entry => entry.Invoice)
            .HasForeignKey(entry => entry.InvoiceId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Navigation(invoice => invoice.LineItems)
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.Navigation(invoice => invoice.LedgerEntries)
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.Ignore(invoice => invoice.DomainEvents);
    }
}
