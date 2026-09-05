using _116.Mailer.Application.Notifications.UseCases.Public.Commands.MarkAllNotificationsRead;
using _116.Mailer.Application.Shared.Persistence;
using _116.Mailer.Application.Shared.Repositories;
using _116.Mailer.Contracts.Domain;
using _116.Mailer.Domain.Entities;
using AwesomeAssertions;
using Moq;
using Xunit;

namespace _116.Unit.Tests.Modules.Mailer.Application.Notifications;

/// <summary>
/// Unit tests for <see cref="PublicMarkAllNotificationsReadHandler" />: marks
/// every unread row with one commit and commits nothing when nothing is
/// unread.
/// </summary>
public class PublicMarkAllNotificationsReadHandlerTests
{
    private readonly Mock<INotificationRepository> _repository = new();
    private readonly Mock<IMailerUnitOfWork> _unitOfWork = new();

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

    private PublicMarkAllNotificationsReadHandler CreateHandler()
    {
        return new PublicMarkAllNotificationsReadHandler(_repository.Object, _unitOfWork.Object);
    }

    [Fact]
    public async Task Handle_WithUnreadNotifications_ShouldMarkEveryRowAndCommitOnce()
    {
        var userId = Guid.NewGuid();
        List<NotificationEntity> unread = [CreateNotification(userId), CreateNotification(userId)];
        _repository.Setup(r => r.GetUnreadForUserAsync(userId, It.IsAny<CancellationToken>())).ReturnsAsync(unread);

        PublicMarkAllNotificationsReadResult result = await CreateHandler()
            .Handle(new PublicMarkAllNotificationsReadCommand(userId), CancellationToken.None);

        result.MarkedCount.Should().Be(2);
        unread.Should().OnlyContain(n => n.ReadAt != null);
        unread.Select(n => n.ReadAt).Distinct().Should().HaveCount(1);
        _unitOfWork.Verify(u => u.CommitAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WithNothingUnread_ShouldMarkZeroAndCommitNothing()
    {
        var userId = Guid.NewGuid();
        _repository.Setup(r => r.GetUnreadForUserAsync(userId, It.IsAny<CancellationToken>())).ReturnsAsync([]);

        PublicMarkAllNotificationsReadResult result = await CreateHandler()
            .Handle(new PublicMarkAllNotificationsReadCommand(userId), CancellationToken.None);

        result.MarkedCount.Should().Be(0);
        _unitOfWork.Verify(u => u.CommitAsync(It.IsAny<CancellationToken>()), Times.Never);
    }
}
