using InvoiceFlow.Application.Common.Interfaces;
using InvoiceFlow.Application.Invoicing.Dtos;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace InvoiceFlow.Application.Invoicing.Queries.ListCreditNotes;

public sealed class ListCreditNotesQueryHandler(IApplicationDbContext db)
    : IRequestHandler<ListCreditNotesQuery, ListCreditNotesResult>
{
    public async Task<ListCreditNotesResult> Handle(ListCreditNotesQuery request, CancellationToken ct)
    {
        var query = db.CreditNotes.AsQueryable();

        if (request.OriginalInvoiceId.HasValue)
            query = query.Where(cn => cn.OriginalInvoiceId == request.OriginalInvoiceId.Value);

        var total = await query.CountAsync(ct);
        var totalPages = (int)Math.Ceiling(total / (double)request.PageSize);

        var creditNotes = await query
            .OrderByDescending(cn => cn.CreatedAt)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync(ct);

        var invoiceIds = creditNotes.Select(cn => cn.OriginalInvoiceId).Distinct().ToList();
        var clientIds = creditNotes.Select(cn => cn.ClientId).Distinct().ToList();

        var invoices = await db.Invoices
            .Where(i => invoiceIds.Contains(i.Id))
            .Select(i => new { i.Id, i.Number })
            .ToDictionaryAsync(i => i.Id, i => i.Number, ct);

        var clients = await db.Clients
            .Where(c => clientIds.Contains(c.Id))
            .ToDictionaryAsync(c => c.Id, c => c.Name, ct);

        var items = creditNotes.Select(cn => new CreditNoteDto(
            cn.Id,
            cn.Number,
            cn.OriginalInvoiceId,
            invoices.GetValueOrDefault(cn.OriginalInvoiceId, string.Empty),
            clients.GetValueOrDefault(cn.ClientId, string.Empty),
            cn.Currency.ToString(),
            cn.Amount.Amount,
            cn.Reason,
            cn.IssuedAt,
            cn.Status,
            cn.SentAt,
            cn.CreatedAt)).ToList();

        return new ListCreditNotesResult(items, total, totalPages);
    }
}
