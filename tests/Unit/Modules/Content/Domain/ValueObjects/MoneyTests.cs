using _116.Content.Domain.Exceptions;
using _116.Content.Domain.StateMachines;
using _116.Content.Domain.ValueObjects;
using AwesomeAssertions;
using Xunit;

namespace _116.Unit.Tests.Modules.Content.Domain.ValueObjects;

/// <summary>
/// Unit tests for the <see cref="Money"/> value object.
/// </summary>
public class MoneyTests
{
    #region Constructor Tests

    [Theory]
    [InlineData(0)]
    [InlineData(0.01)]
    [InlineData(9.99)]
    [InlineData(1000)]
    public void Constructor_WithNonNegativeAmount_ShouldKeepIt(decimal amount)
    {
        // Act
        Money money = new(amount);

        // Assert
        money.Amount.Should().Be(amount);
    }

    [Theory]
    [InlineData(-0.01)]
    [InlineData(-1)]
    [InlineData(-9999.99)]
    public void Constructor_WithNegativeAmount_ShouldThrowNegativeMoneyAmount(decimal amount)
    {
        // Act
        Action act = () => new Money(amount);

        // Assert
        act.Should().ThrowExactly<ContentRuleException>().Which.Code.Should().Be(ContentRuleCodes.NegativeMoneyAmount);
    }

    [Fact]
    public void Zero_ShouldBeAnAmountOfZero()
    {
        Money.Zero.Amount.Should().Be(0m);
    }

    #endregion

    #region TryFrom Tests

    [Fact]
    public void TryFrom_WithNonNegativeAmount_ShouldReturnTheAmount()
    {
        Money? money = Money.TryFrom(9.99m);

        money.Should().NotBeNull();
        money!.Amount.Should().Be(9.99m);
    }

    [Fact]
    public void TryFrom_WithAnAbsentAmount_ShouldReturnNullInsteadOfThrowing()
    {
        Money.TryFrom(null).Should().BeNull();
    }

    [Fact]
    public void TryFrom_WithANegativeAmount_ShouldReturnNullInsteadOfThrowing()
    {
        Money.TryFrom(-1m).Should().BeNull();
    }

    #endregion

    #region Arithmetic Tests

    [Fact]
    public void Addition_ShouldSumTheAmounts()
    {
        (new Money(10.50m) + new Money(4.50m)).Amount.Should().Be(15m);
    }

    [Fact]
    public void Multiplication_ByAQuantity_ShouldScaleTheAmount()
    {
        (new Money(9.99m) * 3).Amount.Should().Be(29.97m);
    }

    [Fact]
    public void Multiplication_ByANegativeQuantity_ShouldThrowNegativeMoneyAmount()
    {
        Action act = () =>
        {
            Money _ = new Money(9.99m) * -1;
        };

        act.Should().ThrowExactly<ContentRuleException>().Which.Code.Should().Be(ContentRuleCodes.NegativeMoneyAmount);
    }

    [Fact]
    public void Sum_ShouldTotalTheSequence()
    {
        Money.Sum([new Money(10m), new Money(5.50m), new Money(0.50m)]).Amount.Should().Be(16m);
    }

    [Fact]
    public void Sum_OfAnEmptySequence_ShouldBeZero()
    {
        Money.Sum([]).Should().Be(Money.Zero);
    }

    #endregion

    #region Conversion and Equality Tests

    [Fact]
    public void ImplicitConversion_ToDecimal_ShouldYieldTheUnderlyingAmount()
    {
        decimal amount = new Money(9.99m);

        amount.Should().Be(9.99m);
    }

    [Fact]
    public void ImplicitConversion_FromNegativeDecimal_ShouldThrowNegativeMoneyAmount()
    {
        Action act = () =>
        {
            Money _ = -1m;
        };

        act.Should().ThrowExactly<ContentRuleException>().Which.Code.Should().Be(ContentRuleCodes.NegativeMoneyAmount);
    }

    [Fact]
    public void Equality_ShouldCompareByAmount()
    {
        new Money(9.99m).Should().Be(new Money(9.99m));
        new Money(9.99m).Should().NotBe(new Money(10m));
    }

    [Fact]
    public void ToString_ShouldFormatTheAmountWithoutACurrencySymbol()
    {
        new Money(9.99m).ToString().Should().Be("9.99");
        new Money(10m).ToString().Should().Be("10");
    }

    #endregion
}
