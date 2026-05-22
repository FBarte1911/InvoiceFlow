using InvoiceFlow.Application.Common.Interfaces;
using InvoiceFlow.Application.Invoicing.Dtos;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace InvoiceFlow.Application.Invoicing.Queries.GetCreditNotesByInvoice;

public sealed class GetCreditNotesByInvoiceQueryHandler(IApplicationDbContext dbContext)
    : IRequestHandler<GetCreditNotesByInvoiceQuery, IReadOnlyList<CreditNoteDto>>
{
    public async Task<IReadOnlyList<CreditNoteDto>> Handle(GetCreditNotesByInvoiceQuery request, CancellationToken cancellationToken)
    {
        var invoice = await dbContext.Invoices
            .Where(i => i.Id == request.InvoiceId)
            .Select(i => new { i.Id, i.Number })
            .FirstOrDefaultAsync(cancellationToken);

        var invoiceNumber = invoice?.Number ?? string.Empty;

        var clientIds = await dbContext.CreditNotes
            .Where(cn => cn.OriginalInvoiceId == request.InvoiceId)
            .Select(cn => cn.ClientId)
            .Distinct()
            .ToListAsync(cancellationToken);

        var clients = await dbContext.Clients
            .Where(c => clientIds.Contains(c.Id))
            .ToDictionaryAsync(c => c.Id, cancellationToken);

        return await dbContext.CreditNotes
            .Where(cn => cn.OriginalInvoiceId == request.InvoiceId)
            .OrderByDescending(cn => cn.CreatedAt)
            .Select(cn => new CreditNoteDto(
                cn.Id,
                cn.Number,
                cn.OriginalInvoiceId,
                invoiceNumber,
                clients.ContainsKey(cn.ClientId) ? clients[cn.ClientId].Name : string.Empty,
                cn.Currency.ToString(),
                cn.Amount.Amount,
                cn.Reason,
                cn.IssuedAt,
                cn.Status,
                cn.SentAt,
                cn.CreatedAt))
            .ToListAsync(cancellationToken);
    }
}
