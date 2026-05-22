using InvoiceFlow.Domain.Shared;

namespace InvoiceFlow.Domain.Invoicing;

public sealed class InvoiceItem : Entity
{
    public Guid InvoiceId { get; private set; }
    public string Description { get; private set; } = string.Empty;
    public decimal Quantity { get; private set; }
    public Money UnitPrice { get; private set; } = null!;
    public Money Total => UnitPrice.Multiply(Quantity);

    private InvoiceItem() { }

    internal static InvoiceItem Create(Guid invoiceId, string description, decimal quantity, Money unitPrice)
    {
        if (quantity <= 0) throw new ArgumentException("Quantity must be positive.", nameof(quantity));
        if (string.IsNullOrWhiteSpace(description)) throw new ArgumentException("Description is required.", nameof(description));

        return new InvoiceItem
        {
            InvoiceId = invoiceId,
            Description = description.Trim(),
            Quantity = quantity,
            UnitPrice = unitPrice
        };
    }
}
