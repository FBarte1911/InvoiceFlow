using InvoiceFlow.Domain.Invoicing;
using InvoiceFlow.Domain.Shared;
using MediatR;

namespace InvoiceFlow.Application.Invoicing.Commands.CreateInvoice;

public sealed record CreateInvoiceItemRequest(string Description, decimal Quantity, decimal UnitPrice);

public sealed record CreateInvoiceCommand(
    Guid ClientId,
    DateOnly IssueDate,
    DateOnly DueDate,
    Currency Currency,
    string? Notes,
    IReadOnlyList<CreateInvoiceItemRequest> Items,
    DiscountType DiscountType = DiscountType.None,
    decimal DiscountValue = 0,
    decimal? TaxRate = null) : IRequest<Guid>;
