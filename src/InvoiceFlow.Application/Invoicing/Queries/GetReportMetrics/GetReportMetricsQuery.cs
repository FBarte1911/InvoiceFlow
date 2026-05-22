using MediatR;

namespace InvoiceFlow.Application.Invoicing.Queries.GetReportMetrics;

public sealed record GetReportMetricsQuery : IRequest<ReportMetricsResult>;

public sealed record ReportMetricsResult(
    decimal TotalIncome,
    decimal AvgTicket,
    decimal CollectionRate,
    int TotalInvoices,
    int PaidInvoices,
    int PendingInvoices,
    int SentInvoices,
    int OverdueInvoices,
    int DraftInvoices,
    IReadOnlyList<ClientRevenueStats> TopClients,
    IReadOnlyList<MonthlyRevenueStats> MonthlyStats);

public sealed record ClientRevenueStats(
    string Name,
    string Email,
    int TotalInvoices,
    decimal TotalPaid,
    decimal TotalPending);

public sealed record MonthlyRevenueStats(
    string Label,
    int Count,
    decimal Paid,
    decimal Pending);
