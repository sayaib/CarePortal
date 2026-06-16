using System;
using CarePortal.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

#nullable disable

namespace CarePortal.Infrastructure.Persistence.Migrations;

[DbContext(typeof(CarePortalDbContext))]
public partial class CarePortalDbContextModelSnapshot : ModelSnapshot
{
    protected override void BuildModel(ModelBuilder modelBuilder)
    {
        modelBuilder.HasAnnotation("ProductVersion", "8.0.11");

        modelBuilder.Entity("CarePortal.Domain.Billing.Invoice", b =>
        {
            b.Property<Guid>("Id").ValueGeneratedNever().HasColumnName("id");
            b.Property<DateTime>("CreatedAt").HasColumnName("created_at");
            b.Property<string>("Reference").IsRequired().HasMaxLength(50).HasColumnName("reference");
            b.HasKey("Id");
            b.HasIndex("Reference").IsUnique();
            b.ToTable("invoices");
        });

        modelBuilder.Entity("CarePortal.Domain.Billing.InvoiceLineItem", b =>
        {
            b.Property<Guid>("Id").ValueGeneratedNever().HasColumnName("id");
            b.Property<decimal>("Amount").HasPrecision(18, 2).HasColumnName("amount");
            b.Property<DateTime>("CreatedAt").HasColumnName("created_at");
            b.Property<string>("Description").IsRequired().HasMaxLength(500).HasColumnName("description");
            b.Property<DateOnly>("DueDate").HasColumnName("due_date");
            b.Property<Guid>("InvoiceId").HasColumnName("invoice_id");
            b.HasKey("Id");
            b.HasIndex("InvoiceId");
            b.ToTable("invoice_line_items");
        });

        modelBuilder.Entity("CarePortal.Domain.Billing.LedgerEntry", b =>
        {
            b.Property<Guid>("Id").ValueGeneratedNever().HasColumnName("id");
            b.Property<decimal>("Amount").HasPrecision(18, 2).HasColumnName("amount");
            b.Property<DateTime>("CreatedAt").HasColumnName("created_at");
            b.Property<Guid>("InvoiceId").HasColumnName("invoice_id");
            b.Property<Guid?>("LineItemId").HasColumnName("line_item_id");
            b.Property<string>("Type").IsRequired().HasMaxLength(50).HasColumnName("type");
            b.HasKey("Id");
            b.HasIndex("InvoiceId");
            b.HasIndex("LineItemId");
            b.ToTable("ledger_entries");
        });

        modelBuilder.Entity("CarePortal.Domain.Patients.Patient", b =>
        {
            b.Property<Guid>("Id").ValueGeneratedNever().HasColumnName("id");
            b.Property<DateTimeOffset>("CreatedAtUtc").HasColumnName("created_at_utc");
            b.Property<DateOnly>("DateOfBirth").HasColumnName("date_of_birth");
            b.Property<string>("Email").IsRequired().HasMaxLength(256).HasColumnName("email");
            b.Property<string>("FirstName").IsRequired().HasMaxLength(100).HasColumnName("first_name");
            b.Property<string>("LastName").IsRequired().HasMaxLength(100).HasColumnName("last_name");
            b.HasKey("Id");
            b.HasIndex("Email").IsUnique();
            b.ToTable("patients");
        });

        modelBuilder.Entity("CarePortal.Domain.Billing.InvoiceLineItem", b =>
        {
            b.HasOne("CarePortal.Domain.Billing.Invoice", "Invoice")
                .WithMany("LineItems")
                .HasForeignKey("InvoiceId")
                .OnDelete(DeleteBehavior.Cascade)
                .IsRequired();
            b.Navigation("Invoice");
        });

        modelBuilder.Entity("CarePortal.Domain.Billing.LedgerEntry", b =>
        {
            b.HasOne("CarePortal.Domain.Billing.Invoice", "Invoice")
                .WithMany("LedgerEntries")
                .HasForeignKey("InvoiceId")
                .OnDelete(DeleteBehavior.Restrict)
                .IsRequired();
            b.HasOne("CarePortal.Domain.Billing.InvoiceLineItem", "LineItem")
                .WithMany("LedgerEntries")
                .HasForeignKey("LineItemId")
                .OnDelete(DeleteBehavior.Restrict);
            b.Navigation("Invoice");
            b.Navigation("LineItem");
        });

        modelBuilder.Entity("CarePortal.Domain.Billing.Invoice", b =>
        {
            b.Navigation("LedgerEntries");
            b.Navigation("LineItems");
        });

        modelBuilder.Entity("CarePortal.Domain.Billing.InvoiceLineItem", b =>
        {
            b.Navigation("LedgerEntries");
        });
    }
}
