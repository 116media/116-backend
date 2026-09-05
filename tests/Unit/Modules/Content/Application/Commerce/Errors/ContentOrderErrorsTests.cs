using _116.Content.Application.Shared.Errors;
using _116.Content.Application.Shared.Errors.Messages;
using _116.Shared.Application.Exceptions;
using _116.Tests.Fixtures.Helpers;
using AwesomeAssertions;
using Xunit;

namespace _116.Unit.Tests.Modules.Content.Application.Commerce.Errors;

/// <summary>
/// Unit tests for <see cref="ContentOrderErrors"/> factory methods.
/// </summary>
public class ContentOrderErrorsTests
{
    private readonly ContentOrderErrors _errors = TestErrorsFactory.CreateContentOrderErrors();
    private readonly ContentOrderErrorMessage _message = LocalizerFactory.CreateMessage<ContentOrderErrorMessage>();

    [Fact]
    public void NotFound_ShouldReturnNotFoundException()
    {
        Guid id = Guid.NewGuid();

        NotFoundException ex = _errors.NotFound(id);

        ex.Should().NotBeNull();
        ex.Should().BeOfType<NotFoundException>();
    }

    [Fact]
    public void ItemNotFound_ShouldReturnNotFoundExceptionWithItemId()
    {
        Guid itemId = Guid.NewGuid();

        NotFoundException ex = _errors.ItemNotFound(itemId);

        ex.Should().NotBeNull();
        ex.Should().BeOfType<NotFoundException>();
    }

    [Fact]
    public void PaymentNotFound_ShouldReturnNotFoundExceptionWithOrderId()
    {
        Guid orderId = Guid.NewGuid();

        NotFoundException ex = _errors.PaymentNotFound(orderId);

        ex.Should().NotBeNull();
        ex.Should().BeOfType<NotFoundException>();
    }

    [Fact]
    public void CannotAddItemToNonDraftOrder_ShouldReturnBadRequestException()
    {
        BadRequestException ex = _errors.CannotAddItemToNonDraftOrder();

        ex.Should().NotBeNull();
        ex.Message.Should().Contain(_message.CannotAddItemToNonDraftOrder());
    }

    [Fact]
    public void MustHaveAtLeastOneItemWithTier_ShouldReturnBadRequestException()
    {
        BadRequestException ex = _errors.MustHaveAtLeastOneItemWithTier();

        ex.Should().NotBeNull();
        ex.Message.Should().Contain(_message.MustHaveAtLeastOneItemWithTier());
    }
}
