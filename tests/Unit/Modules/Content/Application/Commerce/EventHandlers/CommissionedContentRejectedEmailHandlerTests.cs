using _116.Content.Application.Commerce.EventHandlers;
using _116.Content.Application.Commerce.Services;
using _116.Content.Domain.Enums;
using _116.Content.Domain.Events;
using Moq;
using Xunit;

namespace _116.Unit.Tests.Modules.Content.Application.Commerce.EventHandlers;

/// <summary>
/// Unit tests for <see cref="CommissionedContentRejectedEmailHandler"/>.
/// </summary>
public class CommissionedContentRejectedEmailHandlerTests
{
    private readonly Mock<ICommerceCustomerNotifier> _notifierMock = new();
    private readonly CommissionedContentRejectedEmailHandler _handler;

    public CommissionedContentRejectedEmailHandlerTests()
    {
        _handler = new CommissionedContentRejectedEmailHandler(_notifierMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldDelegateThePayloadToTheNotifier()
    {
        // Arrange
        var customerId = Guid.NewGuid();
        var domainEvent = new CommissionedContentRejectedEvent(
            Guid.NewGuid(),
            EnumCoreContentType.Video,
            customerId,
            "Some title",
            "wrong footage"
        );

        // Act
        await _handler.Handle(domainEvent, CancellationToken.None);

        // Assert
        _notifierMock.Verify(
            x => x.NotifyContentRejectedAsync(customerId, "Some title", "wrong footage", It.IsAny<CancellationToken>()),
            Times.Once
        );
    }
}
