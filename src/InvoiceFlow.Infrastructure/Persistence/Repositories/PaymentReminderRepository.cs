using InvoiceFlow.Domain.Notifications;
using Microsoft.EntityFrameworkCore;

namespace InvoiceFlow.Infrastructure.Persistence.Repositories;

public sealed class PaymentReminderRepository(InvoiceFlowDbContext dbContext) : IPaymentReminderRepository
{
    public async Task<IReadOnlyList<PaymentReminder>> GetPendingAsync(CancellationToken cancellationToken = default) =>
        await dbContext.PaymentReminders
            .Where(r => r.Status == ReminderStatus.Pending)
            .ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<PaymentReminder>> GetByInvoiceAsync(Guid invoiceId, CancellationToken cancellationToken = default) =>
        await dbContext.PaymentReminders
            .Where(r => r.InvoiceId == invoiceId)
            .ToListAsync(cancellationToken);

    public async Task AddRangeAsync(IEnumerable<PaymentReminder> reminders, CancellationToken cancellationToken = default) =>
        await dbContext.PaymentReminders.AddRangeAsync(reminders, cancellationToken);

    public Task UpdateAsync(PaymentReminder reminder, CancellationToken cancellationToken = default)
    {
        dbContext.PaymentReminders.Update(reminder);
        return Task.CompletedTask;
    }
}
