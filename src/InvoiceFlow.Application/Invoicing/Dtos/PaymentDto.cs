using InvoiceFlow.Domain.Invoicing;

namespace InvoiceFlow.Application.Invoicing.Dtos;

public sealed record PaymentDto(
    Guid Id,
    decimal Amount,
    string Currency,
    DateTime PaidAt,
    PaymentMethod Method,
    string? Notes);
