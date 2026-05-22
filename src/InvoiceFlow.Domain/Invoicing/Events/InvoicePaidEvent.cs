using InvoiceFlow.Domain.Shared;

namespace InvoiceFlow.Domain.Invoicing.Events;

public sealed record InvoicePaidEvent(Guid InvoiceId, string TenantId, DateTime PaidAt) : IDomainEvent;
