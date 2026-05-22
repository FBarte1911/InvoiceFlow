namespace InvoiceFlow.Domain.Shared;

public sealed class Money : ValueObject
{
    public Currency Currency { get; }
    public decimal Amount { get; }

    private Money(Currency currency, decimal amount)
    {
        if (amount < 0) throw new ArgumentException("Amount cannot be negative.", nameof(amount));
        Currency = currency;
        Amount = Math.Round(amount, 2);
    }

    public static Money Of(Currency currency, decimal amount) => new(currency, amount);
    public static Money Zero(Currency currency) => new(currency, 0);

    public Money Add(Money other)
    {
        if (Currency != other.Currency) throw new InvalidOperationException("Cannot add amounts in different currencies.");
        return new Money(Currency, Amount + other.Amount);
    }

    public Money Subtract(Money other)
    {
        if (Currency != other.Currency) throw new InvalidOperationException("Cannot subtract amounts in different currencies.");
        return new Money(Currency, Amount - other.Amount);
    }

    public Money Multiply(decimal factor) => new(Currency, Amount * factor);

    public override string ToString() => $"{Amount:F2} {Currency}";

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Currency;
        yield return Amount;
    }
}
