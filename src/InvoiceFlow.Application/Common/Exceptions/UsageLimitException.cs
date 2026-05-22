namespace InvoiceFlow.Application.Common.Exceptions;

public sealed class UsageLimitException : Exception
{
    public UsageLimitException(string message) : base(message) { }
}
