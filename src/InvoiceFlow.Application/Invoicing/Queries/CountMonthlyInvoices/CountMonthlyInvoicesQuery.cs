using MediatR;

namespace InvoiceFlow.Application.Invoicing.Queries.CountMonthlyInvoices;

public sealed record CountMonthlyInvoicesQuery(int Year, int Month) : IRequest<int>;
