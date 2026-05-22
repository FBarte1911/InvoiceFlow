using InvoiceFlow.Application.Common.Interfaces;
using InvoiceFlow.Application.Invoicing.Dtos;
using InvoiceFlow.Domain.Invoicing;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace InvoiceFlow.Application.Invoicing.Queries.GetDashboardMetrics;

public sealed class GetDashboardMetricsQueryHandler(IApplicationDbContext db)
    : IRequestHandler<GetDashboardMetricsQuery, DashboardMetricsResult>
{
    public async Task<DashboardMetricsResult> Handle(GetDashboardMetricsQuery request, CancellationToken ct)
    {
        var invoices = await db.Invoices
            .Include(i => i.Items)
            .OrderByDescending(i => i.CreatedAt)
            .ToListAsync(ct);

        var clientIds = invoices.Select(i => i.ClientId).Distinct().ToList();
        var clients = await db.Clients
            .Where(c => clientIds.Contains(c.Id))
            .ToDictionaryAsync(c => c.Id, ct);

        var dtos = invoices
            .Where(i => clients.ContainsKey(i.ClientId))
            .Select(i => i.ToDto(clients[i.ClientId]))
            .ToList();

        var now = DateTime.Today;
        var paidThisMonth = dtos
            .Where(i => i.Status == InvoiceStatus.Paid
                     && i.PaidAt?.Month == now.Month
                     && i.PaidAt?.Year == now.Year)
            .ToList();

        return new DashboardMetricsResult(
            PaidThisMonth:       paidThisMonth.Sum(i => i.Total),
            PaidCount:           paidThisMonth.Count,
            PendingAmount:       dtos.Where(i => i.Status == InvoiceStatus.Sent).Sum(i => i.Total),
            PendingCount:        dtos.Count(i => i.Status == InvoiceStatus.Sent),
            OverdueAmount:       dtos.Where(i => i.Status == InvoiceStatus.Overdue).Sum(i => i.Total),
            OverdueCount:        dtos.Count(i => i.Status == InvoiceStatus.Overdue),
            MonthlyInvoiceCount: dtos.Count(i => i.IssueDate.Month == now.Month && i.IssueDate.Year == now.Year),
            RecentInvoices:      dtos.Take(5).ToList());
    }
}
