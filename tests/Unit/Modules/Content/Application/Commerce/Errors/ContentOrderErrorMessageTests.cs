using _116.Content.Application.Shared.Errors.Messages;
using AwesomeAssertions;
using Xunit;

namespace _116.Unit.Tests.Modules.Content.Application.Commerce.Errors;

/// <summary>
/// Unit tests for <see cref="ContentOrderErrorMessage"/> static message factory methods.
/// </summary>
public class ContentOrderErrorMessageTests
{
    [Fact]
    public void AlreadySubmitted_ShouldReturnExpectedMessage()
    {
        string message = ContentOrderErrorMessage.AlreadySubmitted();

        message.Should().Be("Order has already been submitted.");
    }

    [Fact]
    public void AlreadyPaid_ShouldReturnExpectedMessage()
    {
        string message = ContentOrderErrorMessage.AlreadyPaid();

        message.Should().Be("Order is already paid.");
    }

    [Fact]
    public void AlreadyCancelled_ShouldReturnExpectedMessage()
    {
        string message = ContentOrderErrorMessage.AlreadyCancelled();

        message.Should().Be("Order has already been cancelled.");
    }

    [Fact]
    public void CannotCancelPaidOrder_ShouldReturnExpectedMessage()
    {
        string message = ContentOrderErrorMessage.CannotCancelPaidOrder();

        message.Should().Be("A paid order cannot be cancelled.");
    }

    [Fact]
    public void CannotAddItemToNonDraftOrder_ShouldReturnExpectedMessage()
    {
        string message = ContentOrderErrorMessage.CannotAddItemToNonDraftOrder();

        message.Should().Be("Items can only be added to orders in Draft status.");
    }

    [Fact]
    public void MustHaveAtLeastOneItemWithTier_ShouldReturnExpectedMessage()
    {
        string message = ContentOrderErrorMessage.MustHaveAtLeastOneItemWithTier();

        message
            .Should()
            .Be("The order must have at least one item with at least one pricing tier before it can be submitted.");
    }

    [Fact]
    public void PaymentAlreadyVerified_ShouldReturnExpectedMessage()
    {
        string message = ContentOrderErrorMessage.PaymentAlreadyVerified();

        message.Should().Be("Payment has already been verified.");
    }

    [Fact]
    public void PaymentAlreadyRejected_ShouldReturnExpectedMessage()
    {
        string message = ContentOrderErrorMessage.PaymentAlreadyRejected();

        message.Should().Be("Payment has already been rejected.");
    }
}
