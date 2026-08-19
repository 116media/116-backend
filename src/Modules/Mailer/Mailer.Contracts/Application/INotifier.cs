namespace _116.Mailer.Contracts.Application;

/// <summary>
/// Writes in-app notification rows for platform users. The title and body are
/// rendered and localized at write time, so the stored row is self-contained
/// and stays stable even if the copy that produced it changes later. Called
/// from domain event handlers only — business handlers never write a
/// notification row directly.
/// </summary>
public interface INotifier
{
    /// <summary>
    /// Renders the notification copy for the given type in the given culture
    /// and persists it as an unread row for the user. The write is committed
    /// immediately in the Mailer module's own context; call it after the
    /// triggering business change has been committed. A missing required token
    /// throws — that is a programming error, never a runtime state. The
    /// optional <c>linkPath</c> token carries a relative frontend path stored
    /// alongside the copy; it is never an absolute URL.
    /// </summary>
    /// <param name="userId">The recipient platform user.</param>
    /// <param name="type">The notification type, from the catalog.</param>
    /// <param name="tokens">The dynamic values the notification copy requires.</param>
    /// <param name="culture">The two-letter request culture (e.g. "en", "fr").</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    Task NotifyAsync(
        Guid userId,
        EnumNotificationType type,
        IReadOnlyDictionary<string, string> tokens,
        string culture,
        CancellationToken cancellationToken
    );
}
