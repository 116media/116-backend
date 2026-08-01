using _116.Mailer.Application.Shared.Persistence;
using _116.Mailer.Application.Shared.Repositories;
using _116.Mailer.Application.Shared.Services;
using _116.Mailer.Contracts.Application;
using _116.Mailer.Domain.Entities;
using _116.Mailer.Infrastructure.Services;
using AwesomeAssertions;
using Moq;
using Xunit;

namespace _116.Unit.Tests.Modules.Mailer.Infrastructure.Services;

/// <summary>
/// Unit tests for <see cref="Notifier" />: renders, persists a self-contained
/// unread row, lifts the optional link path token, and commits exactly once.
/// </summary>
public class NotifierTests
{
    private readonly Mock<INotificationRenderer> _renderer = new();
    private readonly Mock<INotificationRepository> _repository = new();
    private readonly Mock<IMailerUnitOfWork> _unitOfWork = new();

    [Fact]
    public async Task NotifyAsync_ShouldPersistTheRenderedNotificationAndCommit()
    {
        var userId = Guid.NewGuid();
        _renderer
            .Setup(r =>
                r.Render(EnumNotificationType.CommentReply, It.IsAny<IReadOnlyDictionary<string, string>>(), "fr")
            )
            .Returns(new RenderedNotification("Nouvelle réponse", "Aline a répondu."));

        NotificationEntity? captured = null;
        _repository
            .Setup(r => r.AddAsync(It.IsAny<NotificationEntity>(), It.IsAny<CancellationToken>()))
            .Callback<NotificationEntity, CancellationToken>((e, _) => captured = e);

        var notifier = new Notifier(_renderer.Object, _repository.Object, _unitOfWork.Object);

        await notifier.NotifyAsync(
            userId: userId,
            type: EnumNotificationType.CommentReply,
            tokens: new Dictionary<string, string> { ["replierName"] = "Aline", ["linkPath"] = "/articles/eloko-oyo" },
            culture: "fr",
            cancellationToken: CancellationToken.None
        );

        captured.Should().NotBeNull();
        captured!.UserId.Should().Be(userId);
        captured.Type.Should().Be(EnumNotificationType.CommentReply);
        captured.Title.Should().Be("Nouvelle réponse");
        captured.Body.Should().Be("Aline a répondu.");
        captured.LinkPath.Should().Be("/articles/eloko-oyo");
        captured.ReadAt.Should().BeNull();

        _unitOfWork.Verify(u => u.CommitAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task NotifyAsync_WithoutALinkPathToken_ShouldPersistANullLink()
    {
        _renderer
            .Setup(r =>
                r.Render(
                    It.IsAny<EnumNotificationType>(),
                    It.IsAny<IReadOnlyDictionary<string, string>>(),
                    It.IsAny<string>()
                )
            )
            .Returns(new RenderedNotification("Password changed", "Your password was changed."));

        NotificationEntity? captured = null;
        _repository
            .Setup(r => r.AddAsync(It.IsAny<NotificationEntity>(), It.IsAny<CancellationToken>()))
            .Callback<NotificationEntity, CancellationToken>((e, _) => captured = e);

        var notifier = new Notifier(_renderer.Object, _repository.Object, _unitOfWork.Object);

        await notifier.NotifyAsync(
            userId: Guid.NewGuid(),
            type: EnumNotificationType.PasswordChanged,
            tokens: new Dictionary<string, string>(),
            culture: "en",
            cancellationToken: CancellationToken.None
        );

        captured.Should().NotBeNull();
        captured!.LinkPath.Should().BeNull();
    }

    [Fact]
    public async Task NotifyAsync_WhenRenderingThrows_ShouldPersistNothing()
    {
        _renderer
            .Setup(r =>
                r.Render(
                    It.IsAny<EnumNotificationType>(),
                    It.IsAny<IReadOnlyDictionary<string, string>>(),
                    It.IsAny<string>()
                )
            )
            .Throws(new InvalidOperationException("unresolved placeholder"));

        var notifier = new Notifier(_renderer.Object, _repository.Object, _unitOfWork.Object);

        Func<Task> act = () =>
            notifier.NotifyAsync(
                Guid.NewGuid(),
                EnumNotificationType.PasswordChanged,
                new Dictionary<string, string>(),
                "en",
                CancellationToken.None
            );

        await act.Should().ThrowAsync<InvalidOperationException>();
        _repository.Verify(r => r.AddAsync(It.IsAny<NotificationEntity>(), It.IsAny<CancellationToken>()), Times.Never);
        _unitOfWork.Verify(u => u.CommitAsync(It.IsAny<CancellationToken>()), Times.Never);
    }
}
