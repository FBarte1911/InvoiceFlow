namespace InvoiceFlow.Application.Common.Interfaces;

public interface ICurrentTenant
{
    string Id { get; }
    string? Name { get; }
    bool IsAuthenticated { get; }
}
