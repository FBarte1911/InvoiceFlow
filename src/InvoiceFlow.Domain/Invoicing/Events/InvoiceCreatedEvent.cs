using InvoiceFlow.Domain.Shared;

namespace InvoiceFlow.Domain.Invoicing.Events;

public sealed record InvoiceCreatedEvent(Guid InvoiceId, string TenantId) : IDomainEvent;
