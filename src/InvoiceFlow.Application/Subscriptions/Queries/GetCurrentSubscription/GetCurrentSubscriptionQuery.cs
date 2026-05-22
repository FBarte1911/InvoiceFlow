using InvoiceFlow.Application.Subscriptions.Dtos;
using MediatR;

namespace InvoiceFlow.Application.Subscriptions.Queries.GetCurrentSubscription;

public sealed record GetCurrentSubscriptionQuery : IRequest<SubscriptionDto?>;
