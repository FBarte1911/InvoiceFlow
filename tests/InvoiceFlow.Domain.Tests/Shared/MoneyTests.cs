using InvoiceFlow.Domain.Shared;
using Xunit;

namespace InvoiceFlow.Domain.Tests.Shared;

public sealed class MoneyTests
{
    [Fact]
    public void Of_NegativeAmount_ShouldThrow() =>
        Assert.Throws<ArgumentException>(() => Money.Of(Currency.USD, -1m));

    [Fact]
    public void Add_SameCurrency_ShouldReturnSum()
    {
        var a = Money.Of(Currency.USD, 100m);
        var b = Money.Of(Currency.USD, 50m);
        Assert.Equal(150m, a.Add(b).Amount);
    }

    [Fact]
    public void Add_DifferentCurrencies_ShouldThrow()
    {
        var a = Money.Of(Currency.USD, 100m);
        var b = Money.Of(Currency.UYU, 50m);
        Assert.Throws<InvalidOperationException>(() => a.Add(b));
    }

    [Fact]
    public void Multiply_ShouldReturnScaledAmount()
    {
        var money = Money.Of(Currency.USD, 100m);
        Assert.Equal(300m, money.Multiply(3).Amount);
    }

    [Fact]
    public void Equality_SameValues_ShouldBeEqual()
    {
        var a = Money.Of(Currency.USD, 100m);
        var b = Money.Of(Currency.USD, 100m);
        Assert.Equal(a, b);
    }
}
