using InvoiceFlow.Application.Invoicing.Dtos;
using MediatR;

namespace InvoiceFlow.Application.Invoicing.Queries.GetDashboardMetrics;

public sealed record GetDashboardMetricsQuery : IRequest<DashboardMetricsResult>;

public sealed record DashboardMetricsResult(
    decimal PaidThisMonth,
    int PaidCount,
    decimal PendingAmount,
    int PendingCount,
    decimal OverdueAmount,
    int OverdueCount,
    int MonthlyInvoiceCount,
    IReadOnlyList<InvoiceDto> RecentInvoices);
