using MediatR;

namespace InvoiceFlow.Application.Subscriptions.Commands.UpdateTaxDefaults;

public sealed record UpdateTaxDefaultsCommand(decimal TaxRate, string TaxLabel) : IRequest;
