using _116.Mailer.Contracts.Application;

namespace _116.Mailer.Application.Shared.Services;

/// <summary>
/// The rendered parts of an in-app notification.
/// </summary>
/// <param name="Title">The rendered title.</param>
/// <param name="Body">The rendered body.</param>
public record RenderedNotification(string Title, string Body);

/// <summary>
/// Produces the fully rendered title and body for a notification type in a
/// given culture.
/// </summary>
public interface INotificationRenderer
{
    /// <summary>
    /// Renders a notification type with the given tokens. Throws when a
    /// required token is missing or an unresolved placeholder survives
    /// substitution — a missing token is a programming error, never a runtime
    /// state.
    /// </summary>
    /// <param name="type">The notification type to render, from the catalog.</param>
    /// <param name="tokens">The dynamic values the notification copy requires.</param>
    /// <param name="culture">The two-letter culture (e.g. "en", "fr").</param>
    /// <returns>The rendered title and body.</returns>
    RenderedNotification Render(EnumNotificationType type, IReadOnlyDictionary<string, string> tokens, string culture);
}
