using InvoiceFlow.Domain.Clients;
using InvoiceFlow.Domain.Invoicing;
using InvoiceFlow.Domain.Notifications;
using InvoiceFlow.Domain.Subscriptions;
using Microsoft.EntityFrameworkCore;

namespace InvoiceFlow.Application.Common.Interfaces;

public interface IApplicationDbContext
{
    DbSet<Invoice> Invoices { get; }
    DbSet<InvoiceItem> InvoiceItems { get; }
    DbSet<Client> Clients { get; }
    DbSet<Subscription> Subscriptions { get; }
    DbSet<PaymentReminder> PaymentReminders { get; }
    DbSet<Payment> Payments { get; }
    DbSet<CreditNote> CreditNotes { get; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
