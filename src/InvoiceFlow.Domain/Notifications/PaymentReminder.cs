using InvoiceFlow.Domain.Invoicing;
using InvoiceFlow.Domain.Shared;

namespace InvoiceFlow.Domain.Notifications;

public sealed class PaymentReminder : Entity
{
    public string TenantId { get; private set; } = string.Empty;
    public Guid InvoiceId { get; private set; }
    public SendChannel Channel { get; private set; }
    public int DaysAfterDue { get; private set; }
    public DateTime ScheduledAt { get; private set; }
    public DateTime? SentAt { get; private set; }
    public ReminderStatus Status { get; private set; }
    public string? ErrorMessage { get; private set; }

    private PaymentReminder() { }

    public static PaymentReminder Schedule(
        string tenantId,
        Guid invoiceId,
        SendChannel channel,
        int daysAfterDue,
        DateTime scheduledAt)
    {
        return new PaymentReminder
        {
            TenantId = tenantId,
            InvoiceId = invoiceId,
            Channel = channel,
            DaysAfterDue = daysAfterDue,
            ScheduledAt = scheduledAt,
            Status = ReminderStatus.Pending
        };
    }

    public void MarkSent()
    {
        Status = ReminderStatus.Sent;
        SentAt = DateTime.UtcNow;
        Touch();
    }

    public void MarkFailed(string errorMessage)
    {
        Status = ReminderStatus.Failed;
        ErrorMessage = errorMessage;
        Touch();
    }

    public void Cancel()
    {
        Status = ReminderStatus.Cancelled;
        Touch();
    }
}
