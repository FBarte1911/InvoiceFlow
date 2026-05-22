using InvoiceFlow.Application.Clients.Dtos;
using InvoiceFlow.Application.Common.Exceptions;
using InvoiceFlow.Application.Common.Interfaces;
using InvoiceFlow.Domain.Clients;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace InvoiceFlow.Application.Clients.Queries.GetClientById;

public sealed class GetClientByIdQueryHandler(IApplicationDbContext dbContext)
    : IRequestHandler<GetClientByIdQuery, ClientDto>
{
    public async Task<ClientDto> Handle(GetClientByIdQuery request, CancellationToken cancellationToken)
    {
        var client = await dbContext.Clients
            .Where(c => c.Id == request.ClientId && c.IsActive)
            .Select(c => new ClientDto(c.Id, c.Name, c.Email, c.Phone, c.Company, c.TaxId, c.PreferredCurrency, c.IsActive, c.CreatedAt))
            .FirstOrDefaultAsync(cancellationToken)
            ?? throw new NotFoundException(nameof(Client), request.ClientId);

        return client;
    }
}
