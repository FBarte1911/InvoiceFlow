using InvoiceFlow.Domain.Invoicing;
using InvoiceFlow.Domain.Shared;

namespace InvoiceFlow.Application.Invoicing.Dtos;

public sealed record InvoiceDto(
    Guid Id,
    string Number,
    Guid ClientId,
    string ClientName,
    string ClientEmail,
    InvoiceStatus Status,
    DateOnly IssueDate,
    DateOnly DueDate,
    Currency Currency,
    decimal Subtotal,
    decimal Total,
    DiscountType DiscountType,
    decimal DiscountValue,
    decimal DiscountAmount,
    decimal TaxRate,
    decimal TaxAmount,
    decimal AmountPaid,
    decimal BalanceDue,
    string? Notes,
    DateTime? SentAt,
    DateTime? PaidAt,
    DateTime? ReceiptSentAt,
    string? StripePaymentLink,
    string? MercadoPagoPaymentLink,
    IReadOnlyList<InvoiceItemDto> Items,
    DateTime CreatedAt);
