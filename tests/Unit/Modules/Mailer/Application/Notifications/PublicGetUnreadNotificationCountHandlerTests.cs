using _116.Mailer.Application.Notifications.UseCases.Public.Queries.GetUnreadNotificationCount;
using _116.Mailer.Application.Shared.Repositories;
using AwesomeAssertions;
using Moq;
using Xunit;

namespace _116.Unit.Tests.Modules.Mailer.Application.Notifications;

/// <summary>
/// Unit tests for <see cref="PublicGetUnreadNotificationCountHandler" />:
/// returns the user-scoped unread count.
/// </summary>
public class PublicGetUnreadNotificationCountHandlerTests
{
    private readonly Mock<INotificationRepository> _repository = new();

    [Fact]
    public async Task Handle_ShouldReturnTheRepositoryCount()
    {
        var userId = Guid.NewGuid();
        _repository.Setup(r => r.CountUnreadAsync(userId, It.IsAny<CancellationToken>())).ReturnsAsync(5);

        var handler = new PublicGetUnreadNotificationCountHandler(_repository.Object);

        PublicGetUnreadNotificationCountResult result = await handler.Handle(
            new PublicGetUnreadNotificationCountQuery(userId),
            CancellationToken.None
        );

        result.Count.Should().Be(5);
        _repository.Verify(r => r.CountUnreadAsync(userId, It.IsAny<CancellationToken>()), Times.Once);
    }
}
