using InvoiceFlow.Application.Common.Interfaces;
using InvoiceFlow.Domain.Clients;
using InvoiceFlow.Domain.Invoicing;
using InvoiceFlow.Domain.Notifications;
using InvoiceFlow.Domain.Shared;
using InvoiceFlow.Domain.Subscriptions;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace InvoiceFlow.Infrastructure.Persistence;

public sealed class InvoiceFlowDbContext(
    DbContextOptions<InvoiceFlowDbContext> options,
    IHttpContextAccessor httpContextAccessor,
    IConfiguration configuration)
    : DbContext(options), IApplicationDbContext
{
    private string CurrentTenantId =>
        httpContextAccessor.HttpContext?.User.FindFirst("tenant_id")?.Value
        ?? configuration["DevTenant:Id"]
        ?? "system";

    public DbSet<Invoice> Invoices => Set<Invoice>();
    public DbSet<InvoiceItem> InvoiceItems => Set<InvoiceItem>();
    public DbSet<Client> Clients => Set<Client>();
    public DbSet<Subscription> Subscriptions => Set<Subscription>();
    public DbSet<PaymentReminder> PaymentReminders => Set<PaymentReminder>();
    public DbSet<Payment> Payments => Set<Payment>();
    public DbSet<CreditNote> CreditNotes => Set<CreditNote>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(InvoiceFlowDbContext).Assembly);

        modelBuilder.Entity<Invoice>().HasQueryFilter(i => i.TenantId == CurrentTenantId);
        modelBuilder.Entity<Client>().HasQueryFilter(c => c.TenantId == CurrentTenantId);
        modelBuilder.Entity<Subscription>().HasQueryFilter(s => s.TenantId == CurrentTenantId);
        modelBuilder.Entity<PaymentReminder>().HasQueryFilter(r => r.TenantId == CurrentTenantId);
        modelBuilder.Entity<Payment>().HasQueryFilter(p => p.TenantId == CurrentTenantId);
        modelBuilder.Entity<CreditNote>().HasQueryFilter(cn => cn.TenantId == CurrentTenantId);
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        foreach (var entry in ChangeTracker.Entries<Entity>().Where(e => e.State == EntityState.Modified))
            entry.Property(nameof(Entity.UpdatedAt)).CurrentValue = DateTime.UtcNow;

        return base.SaveChangesAsync(cancellationToken);
    }
}
