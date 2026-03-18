using _116.Content.Application.Shared.Errors;
using _116.Content.Application.Shared.Errors.Messages;
using _116.Shared.Application.Exceptions;
using AwesomeAssertions;
using Xunit;

namespace _116.Unit.Tests.Modules.Content.Application.Commerce.Errors;

/// <summary>
/// Unit tests for <see cref="ContentOrderErrors"/> static factory methods.
/// </summary>
public class ContentOrderErrorsTests
{
    [Fact]
    public void NotFound_ShouldReturnNotFoundException()
    {
        Guid id = Guid.NewGuid();

        NotFoundException ex = ContentOrderErrors.NotFound(id);

        ex.Should().NotBeNull();
        ex.Should().BeOfType<NotFoundException>();
    }

    [Fact]
    public void ItemNotFound_ShouldReturnNotFoundExceptionWithItemId()
    {
        Guid itemId = Guid.NewGuid();

        NotFoundException ex = ContentOrderErrors.ItemNotFound(itemId);

        ex.Should().NotBeNull();
        ex.Should().BeOfType<NotFoundException>();
    }

    [Fact]
    public void PaymentNotFound_ShouldReturnNotFoundExceptionWithOrderId()
    {
        Guid orderId = Guid.NewGuid();

        NotFoundException ex = ContentOrderErrors.PaymentNotFound(orderId);

        ex.Should().NotBeNull();
        ex.Should().BeOfType<NotFoundException>();
    }

    [Fact]
    public void AlreadySubmitted_ShouldReturnConflictException()
    {
        ConflictException ex = ContentOrderErrors.AlreadySubmitted();

        ex.Should().NotBeNull();
        ex.Message.Should().Contain(ContentOrderErrorMessage.AlreadySubmitted());
    }

    [Fact]
    public void AlreadyPaid_ShouldReturnConflictException()
    {
        ConflictException ex = ContentOrderErrors.AlreadyPaid();

        ex.Should().NotBeNull();
        ex.Message.Should().Contain(ContentOrderErrorMessage.AlreadyPaid());
    }

    [Fact]
    public void AlreadyCancelled_ShouldReturnConflictException()
    {
        ConflictException ex = ContentOrderErrors.AlreadyCancelled();

        ex.Should().NotBeNull();
        ex.Message.Should().Contain(ContentOrderErrorMessage.AlreadyCancelled());
    }

    [Fact]
    public void CannotCancelPaidOrder_ShouldReturnBadRequestException()
    {
        BadRequestException ex = ContentOrderErrors.CannotCancelPaidOrder();

        ex.Should().NotBeNull();
        ex.Message.Should().Contain(ContentOrderErrorMessage.CannotCancelPaidOrder());
    }

    [Fact]
    public void CannotAddItemToNonDraftOrder_ShouldReturnBadRequestException()
    {
        BadRequestException ex = ContentOrderErrors.CannotAddItemToNonDraftOrder();

        ex.Should().NotBeNull();
        ex.Message.Should().Contain(ContentOrderErrorMessage.CannotAddItemToNonDraftOrder());
    }

    [Fact]
    public void MustHaveAtLeastOneItemWithTier_ShouldReturnBadRequestException()
    {
        BadRequestException ex = ContentOrderErrors.MustHaveAtLeastOneItemWithTier();

        ex.Should().NotBeNull();
        ex.Message.Should().Contain(ContentOrderErrorMessage.MustHaveAtLeastOneItemWithTier());
    }

    [Fact]
    public void PaymentAlreadyVerified_ShouldReturnConflictException()
    {
        ConflictException ex = ContentOrderErrors.PaymentAlreadyVerified();

        ex.Should().NotBeNull();
        ex.Message.Should().Contain(ContentOrderErrorMessage.PaymentAlreadyVerified());
    }

    [Fact]
    public void PaymentAlreadyRejected_ShouldReturnConflictException()
    {
        ConflictException ex = ContentOrderErrors.PaymentAlreadyRejected();

        ex.Should().NotBeNull();
        ex.Message.Should().Contain(ContentOrderErrorMessage.PaymentAlreadyRejected());
    }
}
