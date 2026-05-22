using InvoiceFlow.Domain.Invoicing;
using MediatR;

namespace InvoiceFlow.Application.Invoicing.Commands.RegisterPayment;

public sealed record RegisterPaymentCommand(
    Guid InvoiceId,
    decimal Amount,
    DateTime PaidAt,
    PaymentMethod Method,
    string? Notes) : IRequest;
