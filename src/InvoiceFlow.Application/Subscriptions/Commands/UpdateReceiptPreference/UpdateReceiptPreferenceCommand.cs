using MediatR;

namespace InvoiceFlow.Application.Subscriptions.Commands.UpdateReceiptPreference;

public sealed record UpdateReceiptPreferenceCommand(bool SendReceiptOnPaid) : IRequest;
