using _116.BuildingBlocks.Application.Exceptions;
using _116.Mailer.Application.Notifications.UseCases.Public.Commands.MarkNotificationRead;
using _116.Mailer.Application.Shared.Errors;
using _116.Mailer.Application.Shared.Errors.Messages;
using _116.Mailer.Application.Shared.Persistence;
using _116.Mailer.Application.Shared.Repositories;
using _116.Mailer.Contracts.Domain.Enums;
using _116.Mailer.Domain.Entities;
using _116.Mailer.TestData.Builders.Entities;
using _116.Tests.TestData.Helpers;
using AwesomeAssertions;
using Moq;
using Xunit;

namespace _116.Mailer.Unit.Tests.Application.Notifications;

/// <summary>
/// Unit tests for <see cref="PublicMarkNotificationReadHandler" />: first
/// mark commits, re-mark is a committed-nothing no-op, and a row missing for
/// this user resolves to a not-found error.
/// </summary>
public class PublicMarkNotificationReadHandlerTests
{
    private static readonly NotificationErrors Errors = new(LocalizerFactory.CreateMessage<NotificationErrorMessage>());

    private readonly Mock<INotificationRepository> _repository = new();
    private readonly Mock<IMailerUnitOfWork> _unitOfWork = new();

    private static NotificationEntity CreateNotification(Guid userId)
    {
        return new NotificationBuilder()
            .WithId(Guid.NewGuid())
            .WithUserId(userId)
            .WithType(EnumNotificationType.PasswordChanged)
            .WithContent("Password changed", "Your password was changed.")
            .WithLinkPath(null)
            .Build();
    }

    private PublicMarkNotificationReadHandler CreateHandler()
    {
        return new PublicMarkNotificationReadHandler(_repository.Object, _unitOfWork.Object, Errors);
    }

    [Fact]
    public async Task Handle_AnUnreadOwnNotification_ShouldMarkItReadAndCommit()
    {
        var userId = Guid.NewGuid();
        NotificationEntity notification = CreateNotification(userId);
        _repository
            .Setup(r => r.GetForUserAsync(notification.Id, userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(notification);

        PublicMarkNotificationReadResult result = await CreateHandler()
            .Handle(new PublicMarkNotificationReadCommand(userId, notification.Id), CancellationToken.None);

        result.IsRead.Should().BeTrue();
        notification.ReadAt.Should().NotBeNull();
        _unitOfWork.Verify(u => u.CommitAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_AnAlreadyReadNotification_ShouldSucceedWithoutCommitting()
    {
        var userId = Guid.NewGuid();
        NotificationEntity notification = CreateNotification(userId);
        var originalReadTime = new DateTime(2026, 8, 17, 12, 0, 0, DateTimeKind.Utc);
        notification.MarkRead(originalReadTime);
        _repository
            .Setup(r => r.GetForUserAsync(notification.Id, userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(notification);

        PublicMarkNotificationReadResult result = await CreateHandler()
            .Handle(new PublicMarkNotificationReadCommand(userId, notification.Id), CancellationToken.None);

        result.IsRead.Should().BeTrue();
        notification.ReadAt.Should().Be(originalReadTime);
        _unitOfWork.Verify(u => u.CommitAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ANotificationMissingForThisUser_ShouldThrowNotFound()
    {
        var userId = Guid.NewGuid();
        _repository
            .Setup(r => r.GetForUserAsync(It.IsAny<Guid>(), userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((NotificationEntity?)null);

        Func<Task> act = () =>
            CreateHandler()
                .Handle(new PublicMarkNotificationReadCommand(userId, Guid.NewGuid()), CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>();
        _unitOfWork.Verify(u => u.CommitAsync(It.IsAny<CancellationToken>()), Times.Never);
    }
}
