using InvoiceFlow.Domain.Invoicing.Events;
using InvoiceFlow.Domain.Shared;

namespace InvoiceFlow.Domain.Invoicing;

public sealed class CreditNote : AggregateRoot
{
    public string TenantId { get; private set; } = string.Empty;
    public string Number { get; private set; } = string.Empty;
    public Guid OriginalInvoiceId { get; private set; }
    public Guid ClientId { get; private set; }
    public Currency Currency { get; private set; }
    public Money Amount { get; private set; } = null!;
    public string Reason { get; private set; } = string.Empty;
    public DateOnly IssuedAt { get; private set; }
    public CreditNoteStatus Status { get; private set; }
    public DateTime? SentAt { get; private set; }

    private CreditNote() { }

    public static CreditNote Create(
        string tenantId,
        string number,
        Guid originalInvoiceId,
        Guid clientId,
        Currency currency,
        decimal amount,
        string reason)
    {
        if (string.IsNullOrWhiteSpace(tenantId)) throw new ArgumentException("TenantId is required.", nameof(tenantId));
        if (string.IsNullOrWhiteSpace(number)) throw new ArgumentException("Number is required.", nameof(number));
        if (amount <= 0) throw new ArgumentException("Amount must be positive.", nameof(amount));
        if (string.IsNullOrWhiteSpace(reason)) throw new ArgumentException("Reason is required.", nameof(reason));

        return new CreditNote
        {
            TenantId = tenantId,
            Number = number.Trim(),
            OriginalInvoiceId = originalInvoiceId,
            ClientId = clientId,
            Currency = currency,
            Amount = Money.Of(currency, amount),
            Reason = reason.Trim(),
            IssuedAt = DateOnly.FromDateTime(DateTime.UtcNow),
            Status = CreditNoteStatus.Draft
        };
    }

    public void Issue()
    {
        if (Status != CreditNoteStatus.Draft)
            throw new InvalidOperationException("Only draft credit notes can be issued.");

        Status = CreditNoteStatus.Issued;
        Touch();
        Raise(new CreditNoteIssuedEvent(Id, TenantId, OriginalInvoiceId));
    }

    public void MarkSent()
    {
        SentAt = DateTime.UtcNow;
        Touch();
    }
}
