using InvoiceFlow.Domain.Shared;

namespace InvoiceFlow.Domain.Invoicing.Events;

public sealed record InvoiceSentEvent(Guid InvoiceId, string TenantId, SendChannel Channel) : IDomainEvent;
