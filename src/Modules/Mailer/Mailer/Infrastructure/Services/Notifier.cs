using _116.Mailer.Application.Shared.Persistence;
using _116.Mailer.Application.Shared.Repositories;
using _116.Mailer.Application.Shared.Services;
using _116.Mailer.Contracts.Application;
using _116.Mailer.Contracts.Domain;
using _116.Mailer.Domain.Entities;

namespace _116.Mailer.Infrastructure.Services;

/// <summary>
/// The <see cref="INotifier" /> implementation: renders the notification copy
/// and persists a self-contained unread row in the Mailer module's own
/// context. The write commits immediately — event handlers call it after the
/// triggering business change has committed, so a rolled-back operation never
/// leaves a notification behind.
/// </summary>
/// <param name="renderer">The notification copy renderer.</param>
/// <param name="notificationRepository">The notification persistence port.</param>
/// <param name="unitOfWork">The Mailer module unit of work.</param>
public class Notifier(
    INotificationRenderer renderer,
    INotificationRepository notificationRepository,
    IMailerUnitOfWork unitOfWork
) : INotifier
{
    /// <summary>
    /// The token that carries the optional relative frontend path. It is
    /// lifted into the stored row's link column rather than substituted into
    /// the copy.
    /// </summary>
    private const string LinkPathToken = "linkPath";

    /// <inheritdoc />
    public async Task NotifyAsync(
        Guid userId,
        EnumNotificationType type,
        IReadOnlyDictionary<string, string> tokens,
        string culture,
        CancellationToken cancellationToken
    )
    {
        RenderedNotification rendered = renderer.Render(type, tokens, culture);

        tokens.TryGetValue(LinkPathToken, out string? linkPath);

        NotificationEntity notification = NotificationEntity.Create(
            id: Guid.NewGuid(),
            userId: userId,
            type: type,
            title: rendered.Title,
            body: rendered.Body,
            linkPath: linkPath
        );

        await notificationRepository.AddAsync(notification, cancellationToken);
        await unitOfWork.CommitAsync(cancellationToken);
    }
}
