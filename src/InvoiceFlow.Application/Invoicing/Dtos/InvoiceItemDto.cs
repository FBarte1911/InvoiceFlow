namespace InvoiceFlow.Application.Invoicing.Dtos;

public sealed record InvoiceItemDto(
    Guid Id,
    string Description,
    decimal Quantity,
    decimal UnitPrice,
    decimal Total);
