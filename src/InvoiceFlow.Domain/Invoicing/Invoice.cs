using InvoiceFlow.Domain.Invoicing.Events;
using InvoiceFlow.Domain.Shared;

namespace InvoiceFlow.Domain.Invoicing;

public sealed class Invoice : AggregateRoot
{
    private readonly List<InvoiceItem> _items = [];

    public string TenantId { get; private set; } = string.Empty;
    public string Number { get; private set; } = string.Empty;
    public Guid ClientId { get; private set; }
    public InvoiceStatus Status { get; private set; }
    public DateOnly IssueDate { get; private set; }
    public DateOnly DueDate { get; private set; }
    public Currency Currency { get; private set; }
    public Money Subtotal { get; private set; } = null!;
    public Money Total { get; private set; } = null!;
    public string? Notes { get; private set; }
    public DateTime? SentAt { get; private set; }
    public DateTime? PaidAt { get; private set; }
    public DateTime? ReceiptSentAt { get; private set; }
    public SendChannel? LastSentChannel { get; private set; }
    public string? StripePaymentLink { get; private set; }
    public DiscountType DiscountType { get; private set; }
    public decimal DiscountValue { get; private set; }
    public Money DiscountAmount { get; private set; } = null!;
    public decimal TaxRate { get; private set; } = 0;
    public Money TaxAmount { get; private set; } = null!;
    public Money AmountPaid { get; private set; } = null!;
    public string? MercadoPagoPaymentLink { get; private set; }

    public IReadOnlyList<InvoiceItem> Items => _items.AsReadOnly();

    private Invoice() { }

    public Money BalanceDue => Money.Of(Currency, Math.Max(0, Total.Amount - AmountPaid.Amount));

    public static Invoice Create(
        string tenantId,
        string number,
        Guid clientId,
        DateOnly issueDate,
        DateOnly dueDate,
        Currency currency,
        string? notes = null,
        decimal taxRate = 0)
    {
        if (string.IsNullOrWhiteSpace(tenantId)) throw new ArgumentException("TenantId is required.", nameof(tenantId));
        if (string.IsNullOrWhiteSpace(number)) throw new ArgumentException("Invoice number is required.", nameof(number));
        if (dueDate < issueDate) throw new ArgumentException("Due date cannot be before issue date.", nameof(dueDate));
        if (taxRate < 0 || taxRate > 100) throw new ArgumentException("Tax rate must be between 0 and 100.", nameof(taxRate));

        var invoice = new Invoice
        {
            TenantId = tenantId,
            Number = number.Trim(),
            ClientId = clientId,
            IssueDate = issueDate,
            DueDate = dueDate,
            Currency = currency,
            Status = InvoiceStatus.Draft,
            Notes = notes?.Trim(),
            TaxRate = taxRate,
            Subtotal = Money.Zero(currency),
            DiscountAmount = Money.Zero(currency),
            TaxAmount = Money.Zero(currency),
            AmountPaid = Money.Zero(currency),
            Total = Money.Zero(currency)
        };

        invoice.Raise(new InvoiceCreatedEvent(invoice.Id, tenantId));
        return invoice;
    }

    public void AddItem(string description, decimal quantity, decimal unitPrice)
    {
        EnsureStatus(InvoiceStatus.Draft);
        var item = InvoiceItem.Create(Id, description, quantity, Money.Of(Currency, unitPrice));
        _items.Add(item);
        RecalculateTotals();
        Touch();
    }

    public void RemoveItem(Guid itemId)
    {
        EnsureStatus(InvoiceStatus.Draft);
        var item = _items.FirstOrDefault(i => i.Id == itemId)
            ?? throw new InvalidOperationException("Item not found.");
        _items.Remove(item);
        RecalculateTotals();
        Touch();
    }

    public void Send(SendChannel channel)
    {
        if (Status != InvoiceStatus.Draft && Status != InvoiceStatus.Sent)
            throw new InvalidOperationException($"Cannot send invoice with status {Status}.");
        if (!_items.Any())
            throw new InvalidOperationException("Cannot send an invoice with no items.");

        Status = InvoiceStatus.Sent;
        SentAt = DateTime.UtcNow;
        LastSentChannel = channel;
        Touch();

        Raise(new InvoiceSentEvent(Id, TenantId, channel));
    }

    public void MarkAsPaid()
    {
        if (Status != InvoiceStatus.Sent && Status != InvoiceStatus.Overdue)
            throw new InvalidOperationException($"Cannot mark as paid invoice with status {Status}.");

        Status = InvoiceStatus.Paid;
        PaidAt = DateTime.UtcNow;
        Touch();

        Raise(new InvoicePaidEvent(Id, TenantId, PaidAt.Value));
    }

    public void MarkAsOverdue()
    {
        if (Status != InvoiceStatus.Sent)
            throw new InvalidOperationException("Only sent invoices can be marked as overdue.");

        Status = InvoiceStatus.Overdue;
        Touch();
    }

    public void Cancel()
    {
        if (Status == InvoiceStatus.Paid)
            throw new InvalidOperationException("Cannot cancel a paid invoice.");
        if (Status == InvoiceStatus.Cancelled)
            throw new InvalidOperationException("Invoice is already cancelled.");

        Status = InvoiceStatus.Cancelled;
        Touch();
    }

    public void SetStripePaymentLink(string link)
    {
        StripePaymentLink = link;
        Touch();
    }

    public void SetMercadoPagoPaymentLink(string link)
    {
        MercadoPagoPaymentLink = link;
        Touch();
    }

    public void SetTaxRate(decimal rate)
    {
        EnsureStatus(InvoiceStatus.Draft);
        if (rate < 0 || rate > 100) throw new ArgumentException("Tax rate must be between 0 and 100.", nameof(rate));
        TaxRate = rate;
        RecalculateTotals();
        Touch();
    }

    public void ApplyPayment(Money payment)
    {
        if (Status == InvoiceStatus.Paid)
            throw new InvalidOperationException("Invoice is already fully paid.");
        if (Status == InvoiceStatus.Cancelled)
            throw new InvalidOperationException("Cannot apply payment to a cancelled invoice.");
        if (payment.Currency != Currency)
            throw new InvalidOperationException("Payment currency must match invoice currency.");
        if (payment.Amount <= 0)
            throw new ArgumentException("Payment amount must be positive.", nameof(payment));

        AmountPaid = AmountPaid.Add(payment);
        Touch();

        if (AmountPaid.Amount >= Total.Amount)
        {
            Status = InvoiceStatus.Paid;
            PaidAt = DateTime.UtcNow;
            Raise(new InvoicePaidEvent(Id, TenantId, PaidAt.Value));
        }
    }

    public void SetDiscount(DiscountType type, decimal value)
    {
        EnsureStatus(InvoiceStatus.Draft);
        if (type == DiscountType.Percentage && (value < 0 || value > 100))
            throw new ArgumentException("El descuento porcentual debe estar entre 0 y 100.", nameof(value));
        if (type == DiscountType.FixedAmount && value < 0)
            throw new ArgumentException("El descuento no puede ser negativo.", nameof(value));
        if (type == DiscountType.FixedAmount && value > Subtotal.Amount)
            throw new ArgumentException("El descuento no puede ser mayor al subtotal.", nameof(value));

        DiscountType = type;
        DiscountValue = value;
        RecalculateTotals();
        Touch();
    }

    public void MarkReceiptSent()
    {
        ReceiptSentAt = DateTime.UtcNow;
        Touch();
    }

    private void RecalculateTotals()
    {
        Subtotal = _items.Aggregate(Money.Zero(Currency), (acc, item) => acc.Add(item.Total));
        DiscountAmount = DiscountType switch
        {
            DiscountType.Percentage => Money.Of(Currency, Math.Round(Subtotal.Amount * DiscountValue / 100, 2)),
            DiscountType.FixedAmount => Money.Of(Currency, DiscountValue),
            _ => Money.Zero(Currency)
        };
        var netBase = Subtotal.Amount - DiscountAmount.Amount;
        TaxAmount = Money.Of(Currency, Math.Round(netBase * TaxRate / 100, 2));
        Total = Money.Of(Currency, netBase + TaxAmount.Amount);
    }

    private void EnsureStatus(InvoiceStatus expected)
    {
        if (Status != expected)
            throw new InvalidOperationException($"Operation requires invoice status {expected}, but current status is {Status}.");
    }
}
