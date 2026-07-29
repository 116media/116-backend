using _116.Content.Application.Commerce.Services;
using AwesomeAssertions;
using Xunit;

namespace _116.Unit.Tests.Modules.Content.Application.Commerce.Services;

/// <summary>
/// Unit tests for <see cref="CommerceCustomerNotifier" />'s formatting helpers
/// — the pieces every customer email is built from.
/// </summary>
public class CommerceCustomerNotifierTests
{
    [Fact]
    public void OrderReference_ShouldBeTheFirstEightHexCharsUppercased()
    {
        var orderId = Guid.Parse("abcdef12-3456-7890-abcd-ef1234567890");

        CommerceCustomerNotifier.OrderReference(orderId).Should().Be("ABCDEF12");
    }

    [Fact]
    public void OrderReference_ShouldBeStableForTheSameOrder()
    {
        var orderId = Guid.NewGuid();

        CommerceCustomerNotifier.OrderReference(orderId).Should().Be(CommerceCustomerNotifier.OrderReference(orderId));
    }

    [Theory]
    [InlineData(150, "150.00")]
    [InlineData(99.5, "99.50")]
    [InlineData(0.125, "0.13")]
    public void FormatAmount_ShouldRenderTwoInvariantDecimals(decimal amount, string expected)
    {
        CommerceCustomerNotifier.FormatAmount(amount).Should().Be(expected);
    }

    [Fact]
    public void PaymentMethods_ShouldListEveryEnumMember()
    {
        CommerceCustomerNotifier.PaymentMethods().Should().Be("BankTransfer, MobileMoney, Cash");
    }
}
