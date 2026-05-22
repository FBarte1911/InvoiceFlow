using InvoiceFlow.Application.Common.Exceptions;
using InvoiceFlow.Application.Common.Interfaces;
using InvoiceFlow.Application.Invoicing.Dtos;
using InvoiceFlow.Domain.Clients;
using InvoiceFlow.Domain.Invoicing;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace InvoiceFlow.Application.Invoicing.Queries.GetInvoiceById;

public sealed class GetInvoiceByIdQueryHandler(IApplicationDbContext dbContext)
    : IRequestHandler<GetInvoiceByIdQuery, InvoiceDto>
{
    public async Task<InvoiceDto> Handle(GetInvoiceByIdQuery request, CancellationToken cancellationToken)
    {
        var invoice = await dbContext.Invoices
            .Include(i => i.Items)
            .FirstOrDefaultAsync(i => i.Id == request.InvoiceId, cancellationToken)
            ?? throw new NotFoundException(nameof(Invoice), request.InvoiceId);

        var client = await dbContext.Clients
            .FirstOrDefaultAsync(c => c.Id == invoice.ClientId, cancellationToken)
            ?? throw new NotFoundException(nameof(Client), invoice.ClientId);

        return invoice.ToDto(client);
    }
}
