using InvoiceFlow.Application.Common.Interfaces;
using InvoiceFlow.Application.Invoicing.Dtos;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace InvoiceFlow.Application.Invoicing.Queries.ListInvoices;

public sealed class ListInvoicesQueryHandler(IApplicationDbContext dbContext)
    : IRequestHandler<ListInvoicesQuery, ListInvoicesResult>
{
    public async Task<ListInvoicesResult> Handle(ListInvoicesQuery request, CancellationToken cancellationToken)
    {
        var query = dbContext.Invoices.Include(i => i.Items).AsQueryable();

        if (request.Status.HasValue)
            query = query.Where(i => i.Status == request.Status.Value);

        var totalCount = await query.CountAsync(cancellationToken);

        var invoices = await query
            .OrderByDescending(i => i.CreatedAt)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync(cancellationToken);

        var clientIds = invoices.Select(i => i.ClientId).Distinct().ToList();
        var clients = await dbContext.Clients
            .Where(c => clientIds.Contains(c.Id))
            .ToDictionaryAsync(c => c.Id, cancellationToken);

        var dtos = invoices
            .Where(i => clients.ContainsKey(i.ClientId))
            .Select(i => i.ToDto(clients[i.ClientId]))
            .ToList();

        return new ListInvoicesResult(dtos, totalCount, request.Page, request.PageSize);
    }
}
