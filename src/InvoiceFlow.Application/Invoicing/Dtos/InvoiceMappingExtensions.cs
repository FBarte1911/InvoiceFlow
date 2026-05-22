using InvoiceFlow.Domain.Clients;
using InvoiceFlow.Domain.Invoicing;

namespace InvoiceFlow.Application.Invoicing.Dtos;

internal static class InvoiceMappingExtensions
{
    internal static InvoiceDto ToDto(this Invoice invoice, Client client) => new(
        invoice.Id,
        invoice.Number,
        client.Id,
        client.Name,
        client.Email,
        invoice.Status,
        invoice.IssueDate,
        invoice.DueDate,
        invoice.Currency,
        invoice.Subtotal.Amount,
        invoice.Total.Amount,
        invoice.DiscountType,
        invoice.DiscountValue,
        invoice.DiscountAmount.Amount,
        invoice.TaxRate,
        invoice.TaxAmount.Amount,
        invoice.AmountPaid.Amount,
        invoice.BalanceDue.Amount,
        invoice.Notes,
        invoice.SentAt,
        invoice.PaidAt,
        invoice.ReceiptSentAt,
        invoice.StripePaymentLink,
        invoice.MercadoPagoPaymentLink,
        invoice.Items.Select(i => new InvoiceItemDto(i.Id, i.Description, i.Quantity, i.UnitPrice.Amount, i.Total.Amount)).ToList(),
        invoice.CreatedAt);
}
