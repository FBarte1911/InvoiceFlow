using InvoiceFlow.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace InvoiceFlow.Application.Invoicing.Queries.CountMonthlyInvoices;

public sealed class CountMonthlyInvoicesQueryHandler(IApplicationDbContext db)
    : IRequestHandler<CountMonthlyInvoicesQuery, int>
{
    public Task<int> Handle(CountMonthlyInvoicesQuery request, CancellationToken ct) =>
        db.Invoices
          .CountAsync(i => i.IssueDate.Year == request.Year
                        && i.IssueDate.Month == request.Month, ct);
}
