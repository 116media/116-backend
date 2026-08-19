using _116.Content.Application.Commerce.EventHandlers;
using _116.Content.Application.Commerce.Services;
using _116.Content.Domain.Enums;
using _116.Content.Domain.Events;
using Moq;
using Xunit;

namespace _116.Unit.Tests.Modules.Content.Application.Commerce.EventHandlers;

/// <summary>
/// Unit tests for <see cref="ContentPromotionRemovedEmailHandler"/>.
/// </summary>
public class ContentPromotionRemovedEmailHandlerTests
{
    private readonly Mock<ICommerceCustomerNotifier> _notifierMock = new();
    private readonly ContentPromotionRemovedEmailHandler _handler;

    public ContentPromotionRemovedEmailHandlerTests()
    {
        _handler = new ContentPromotionRemovedEmailHandler(_notifierMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldDelegateThePayloadToTheNotifier()
    {
        // Arrange
        var customerId = Guid.NewGuid();
        var domainEvent = new ContentPromotionRemovedEvent(
            Guid.NewGuid(),
            EnumCoreContentType.Article,
            customerId,
            "Some title",
            "policy violation"
        );

        // Act
        await _handler.Handle(domainEvent, CancellationToken.None);

        // Assert
        _notifierMock.Verify(
            x =>
                x.NotifyPromotionRemovedAsync(
                    customerId,
                    "Some title",
                    "policy violation",
                    It.IsAny<CancellationToken>()
                ),
            Times.Once
        );
    }

    [Fact]
    public async Task Handle_WithNullCustomer_ShouldStillDelegate_TheNotifierOwnsTheNoOp()
    {
        // Arrange
        var domainEvent = new ContentPromotionRemovedEvent(
            Guid.NewGuid(),
            EnumCoreContentType.Lyrics,
            null,
            "Some title",
            "policy violation"
        );

        // Act
        await _handler.Handle(domainEvent, CancellationToken.None);

        // Assert
        _notifierMock.Verify(
            x => x.NotifyPromotionRemovedAsync(null, "Some title", "policy violation", It.IsAny<CancellationToken>()),
            Times.Once
        );
    }
}
