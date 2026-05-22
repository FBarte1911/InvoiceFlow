using InvoiceFlow.Application.Common.Interfaces;
using InvoiceFlow.Application.Invoicing.Dtos;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace InvoiceFlow.Application.Invoicing.Queries.GetPaymentsByInvoice;

public sealed class GetPaymentsByInvoiceQueryHandler(IApplicationDbContext dbContext)
    : IRequestHandler<GetPaymentsByInvoiceQuery, IReadOnlyList<PaymentDto>>
{
    public async Task<IReadOnlyList<PaymentDto>> Handle(GetPaymentsByInvoiceQuery request, CancellationToken cancellationToken)
    {
        return await dbContext.Payments
            .Where(p => p.InvoiceId == request.InvoiceId)
            .OrderBy(p => p.PaidAt)
            .Select(p => new PaymentDto(
                p.Id,
                p.Amount.Amount,
                p.Amount.Currency.ToString(),
                p.PaidAt,
                p.Method,
                p.Notes))
            .ToListAsync(cancellationToken);
    }
}
