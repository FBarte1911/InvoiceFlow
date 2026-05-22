using InvoiceFlow.Application.Invoicing.Dtos;
using InvoiceFlow.Domain.Invoicing;
using MediatR;

namespace InvoiceFlow.Application.Invoicing.Queries.ListInvoices;

public sealed record ListInvoicesQuery(int Page = 1, int PageSize = 20, InvoiceStatus? Status = null) : IRequest<ListInvoicesResult>;

public sealed record ListInvoicesResult(IReadOnlyList<InvoiceDto> Items, int TotalCount, int Page, int PageSize)
{
    public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
}
