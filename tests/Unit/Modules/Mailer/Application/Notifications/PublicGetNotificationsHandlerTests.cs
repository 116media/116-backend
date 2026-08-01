using _116.Mailer.Application.Notifications.UseCases.Public.Queries.GetNotifications;
using _116.Mailer.Application.Shared.Repositories;
using _116.Mailer.Contracts.Application;
using _116.Mailer.Domain.Entities;
using _116.Shared.Application.Pagination;
using AwesomeAssertions;
using Moq;
using Xunit;

namespace _116.Unit.Tests.Modules.Mailer.Application.Notifications;

/// <summary>
/// Unit tests for <see cref="PublicGetNotificationsHandler" />: pages the
/// user-scoped repository read and maps entities to feed DTOs.
/// </summary>
public class PublicGetNotificationsHandlerTests
{
    private readonly Mock<INotificationRepository> _repository = new();

    private static NotificationEntity CreateNotification(Guid userId)
    {
        return NotificationEntity.Create(
            id: Guid.NewGuid(),
            userId: userId,
            type: EnumNotificationType.PasswordChanged,
            title: "Password changed",
            body: "Your password was changed.",
            linkPath: null
        );
    }

    [Fact]
    public async Task Handle_ShouldPageTheUserScopedReadAndMapToDtos()
    {
        var userId = Guid.NewGuid();
        NotificationEntity notification = CreateNotification(userId);
        _repository
            .Setup(r => r.GetPagedForUserAsync(userId, 1, 20, false, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PaginatedResult<NotificationEntity>(1, 20, 21, [notification]));

        var handler = new PublicGetNotificationsHandler(_repository.Object);

        PublicGetNotificationsResult result = await handler.Handle(
            new PublicGetNotificationsQuery(userId, PageIndex: 1, PageSize: 20, UnreadOnly: false),
            CancellationToken.None
        );

        result.Notifications.PageIndex.Should().Be(1);
        result.Notifications.PageSize.Should().Be(20);
        result.Notifications.Count.Should().Be(21);
        result
            .Notifications.Items.Should()
            .ContainSingle(dto => dto.Id == notification.Id && dto.Title == "Password changed");
    }

    [Fact]
    public async Task Handle_ShouldForwardTheUnreadOnlyFilter()
    {
        var userId = Guid.NewGuid();
        _repository
            .Setup(r => r.GetPagedForUserAsync(userId, 0, 10, true, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PaginatedResult<NotificationEntity>(0, 10, 0, []));

        var handler = new PublicGetNotificationsHandler(_repository.Object);

        await handler.Handle(
            new PublicGetNotificationsQuery(userId, PageIndex: 0, PageSize: 10, UnreadOnly: true),
            CancellationToken.None
        );

        _repository.Verify(r => r.GetPagedForUserAsync(userId, 0, 10, true, It.IsAny<CancellationToken>()), Times.Once);
    }
}
