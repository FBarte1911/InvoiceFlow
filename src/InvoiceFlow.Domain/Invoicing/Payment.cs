using InvoiceFlow.Domain.Shared;

namespace InvoiceFlow.Domain.Invoicing;

public sealed class Payment : Entity
{
    public string TenantId { get; private set; } = string.Empty;
    public Guid InvoiceId { get; private set; }
    public Money Amount { get; private set; } = null!;
    public DateTime PaidAt { get; private set; }
    public PaymentMethod Method { get; private set; }
    public string? Notes { get; private set; }

    private Payment() { }

    public static Payment Create(
        string tenantId,
        Guid invoiceId,
        Money amount,
        DateTime paidAt,
        PaymentMethod method,
        string? notes = null)
    {
        if (amount.Amount <= 0) throw new ArgumentException("Payment amount must be positive.", nameof(amount));

        return new Payment
        {
            TenantId = tenantId,
            InvoiceId = invoiceId,
            Amount = amount,
            PaidAt = paidAt,
            Method = method,
            Notes = notes?.Trim()
        };
    }
}
