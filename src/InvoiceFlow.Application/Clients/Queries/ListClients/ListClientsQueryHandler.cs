using InvoiceFlow.Application.Clients.Dtos;
using InvoiceFlow.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace InvoiceFlow.Application.Clients.Queries.ListClients;

public sealed class ListClientsQueryHandler(IApplicationDbContext dbContext)
    : IRequestHandler<ListClientsQuery, ListClientsResult>
{
    public async Task<ListClientsResult> Handle(ListClientsQuery request, CancellationToken cancellationToken)
    {
        var query = dbContext.Clients.Where(c => c.IsActive);
        var totalCount = await query.CountAsync(cancellationToken);

        var clients = await query
            .OrderBy(c => c.Name)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(c => new ClientDto(c.Id, c.Name, c.Email, c.Phone, c.Company, c.TaxId, c.PreferredCurrency, c.IsActive, c.CreatedAt))
            .ToListAsync(cancellationToken);

        return new ListClientsResult(clients, totalCount);
    }
}
