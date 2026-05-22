namespace InvoiceFlow.Domain.Notifications;

public interface IPaymentReminderRepository
{
    Task<IReadOnlyList<PaymentReminder>> GetPendingAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<PaymentReminder>> GetByInvoiceAsync(Guid invoiceId, CancellationToken cancellationToken = default);
    Task AddRangeAsync(IEnumerable<PaymentReminder> reminders, CancellationToken cancellationToken = default);
    Task UpdateAsync(PaymentReminder reminder, CancellationToken cancellationToken = default);
}
