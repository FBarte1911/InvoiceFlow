using InvoiceFlow.Application.Common.Interfaces;
using InvoiceFlow.Application.Invoicing.Dtos;
using InvoiceFlow.Domain.Invoicing;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace InvoiceFlow.Application.Invoicing.Queries.GetReportMetrics;

public sealed class GetReportMetricsQueryHandler(IApplicationDbContext db)
    : IRequestHandler<GetReportMetricsQuery, ReportMetricsResult>
{
    public async Task<ReportMetricsResult> Handle(GetReportMetricsQuery request, CancellationToken ct)
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

        var paidInvoices = dtos.Count(i => i.Status == InvoiceStatus.Paid);
        var totalIncome  = dtos.Where(i => i.Status == InvoiceStatus.Paid).Sum(i => i.Total);
        var sentTotal    = dtos.Count(i => i.Status is InvoiceStatus.Sent or InvoiceStatus.Paid or InvoiceStatus.Overdue);
        var collectionRate = sentTotal > 0 ? (decimal)paidInvoices / sentTotal * 100 : 0;

        var topClients = dtos
            .GroupBy(i => i.ClientId)
            .Select(g =>
            {
                var client = clients.GetValueOrDefault(g.Key);
                return new ClientRevenueStats(
                    Name:          client?.Name  ?? g.First().ClientName,
                    Email:         client?.Email ?? g.First().ClientEmail,
                    TotalInvoices: g.Count(),
                    TotalPaid:     g.Where(i => i.Status == InvoiceStatus.Paid).Sum(i => i.Total),
                    TotalPending:  g.Where(i => i.Status is InvoiceStatus.Sent or InvoiceStatus.Overdue).Sum(i => i.Total));
            })
            .OrderByDescending(c => c.TotalPaid)
            .Take(10)
            .ToList();

        var monthlyStats = dtos
            .GroupBy(i => new { i.IssueDate.Year, i.IssueDate.Month })
            .OrderByDescending(g => g.Key.Year).ThenByDescending(g => g.Key.Month)
            .Select(g => new MonthlyRevenueStats(
                Label:   new DateTime(g.Key.Year, g.Key.Month, 1).ToString("MMMM yyyy"),
                Count:   g.Count(),
                Paid:    g.Where(i => i.Status == InvoiceStatus.Paid).Sum(i => i.Total),
                Pending: g.Where(i => i.Status is InvoiceStatus.Sent or InvoiceStatus.Overdue).Sum(i => i.Total)))
            .ToList();

        return new ReportMetricsResult(
            TotalIncome:    totalIncome,
            AvgTicket:      paidInvoices > 0 ? totalIncome / paidInvoices : 0,
            CollectionRate: collectionRate,
            TotalInvoices:  dtos.Count,
            PaidInvoices:   paidInvoices,
            PendingInvoices: dtos.Count(i => i.Status is InvoiceStatus.Sent or InvoiceStatus.Overdue),
            SentInvoices:   dtos.Count(i => i.Status == InvoiceStatus.Sent),
            OverdueInvoices: dtos.Count(i => i.Status == InvoiceStatus.Overdue),
            DraftInvoices:  dtos.Count(i => i.Status == InvoiceStatus.Draft),
            TopClients:     topClients,
            MonthlyStats:   monthlyStats);
    }
}
