using _116.Content.Application.Commerce.EventHandlers;
using _116.Content.Application.Commerce.Services;
using _116.Content.Domain.Events;
using Moq;
using Xunit;

namespace _116.Unit.Tests.Modules.Content.Application.Commerce.EventHandlers;

/// <summary>
/// Unit tests for <see cref="VideoShootScheduledEmailHandler"/>.
/// </summary>
public class VideoShootScheduledEmailHandlerTests
{
    private readonly Mock<ICommerceCustomerNotifier> _notifierMock = new();
    private readonly VideoShootScheduledEmailHandler _handler;

    public VideoShootScheduledEmailHandlerTests()
    {
        _handler = new VideoShootScheduledEmailHandler(_notifierMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldDelegateThePayloadToTheNotifierAsUtc()
    {
        // Arrange
        var customerId = Guid.NewGuid();
        DateTimeOffset shootDate = DateTimeOffset.UtcNow.AddDays(10);
        var domainEvent = new VideoShootScheduledEvent(Guid.NewGuid(), customerId, "Some title", shootDate);

        // Act
        await _handler.Handle(domainEvent, CancellationToken.None);

        // Assert
        _notifierMock.Verify(
            x =>
                x.NotifyShootScheduledAsync(
                    customerId,
                    "Some title",
                    shootDate.UtcDateTime,
                    It.IsAny<CancellationToken>()
                ),
            Times.Once
        );
    }
}
