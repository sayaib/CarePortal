using CarePortal.Domain.Billing;
using Microsoft.EntityFrameworkCore;

namespace CarePortal.Infrastructure.Persistence;

public sealed class CarePortalDbContext : DbContext
{
    public CarePortalDbContext(DbContextOptions<CarePortalDbContext> options)
        : base(options)
    {
    }

    public DbSet<Invoice> Invoices => Set<Invoice>();
    public DbSet<InvoiceLineItem> InvoiceLineItems => Set<InvoiceLineItem>();
    public DbSet<LedgerEntry> LedgerEntries => Set<LedgerEntry>();

    public override int SaveChanges()
    {
        EnsureLedgerEntriesAreAppendOnly();
        return base.SaveChanges();
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        EnsureLedgerEntriesAreAppendOnly();
        return base.SaveChangesAsync(cancellationToken);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(CarePortalDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }

    private void EnsureLedgerEntriesAreAppendOnly()
    {
        var hasLedgerMutation = ChangeTracker.Entries<LedgerEntry>()
            .Any(entry => entry.State is EntityState.Modified or EntityState.Deleted);

        if (hasLedgerMutation)
        {
            throw new InvalidOperationException("Ledger entries are append-only and cannot be updated or deleted.");
        }
    }
}
