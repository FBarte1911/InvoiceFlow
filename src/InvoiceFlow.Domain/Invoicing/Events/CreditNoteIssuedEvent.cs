using InvoiceFlow.Domain.Shared;

namespace InvoiceFlow.Domain.Invoicing.Events;

public sealed record CreditNoteIssuedEvent(Guid CreditNoteId, string TenantId, Guid OriginalInvoiceId) : IDomainEvent;
