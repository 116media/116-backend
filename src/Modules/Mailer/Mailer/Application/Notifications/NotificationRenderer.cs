using System.Globalization;
using System.Text.RegularExpressions;
using _116.Mailer.Application.Notifications.Messages;
using _116.Mailer.Application.Shared.Services;
using _116.Mailer.Contracts.Domain;

namespace _116.Mailer.Application.Notifications;

/// <summary>
/// Renders in-app notification copy from the localized resource catalog by
/// <c>{{token}}</c> substitution. Values are inserted raw — the feed renders
/// plain text, not HTML; any placeholder surviving substitution fails the
/// render, so a missing token or a resource typo can never reach a user.
/// </summary>
/// <param name="messages">The localized notification resource facade.</param>
public partial class NotificationRenderer(NotificationMessage messages) : INotificationRenderer
{
    /// <inheritdoc />
    public RenderedNotification Render(
        EnumNotificationType type,
        IReadOnlyDictionary<string, string> tokens,
        string culture
    )
    {
        CultureInfo previous = CultureInfo.CurrentUICulture;

        try
        {
            CultureInfo.CurrentUICulture = ResolveCulture(culture);

            string name = type.ToString();
            string title = Substitute(messages.Title(name), tokens);
            string body = Substitute(messages.Body(name), tokens);

            EnsureFullyResolved(type, title);
            EnsureFullyResolved(type, body);

            return new RenderedNotification(title, body);
        }
        finally
        {
            CultureInfo.CurrentUICulture = previous;
        }
    }

    /// <summary>
    /// Replaces every <c>{{token}}</c> occurrence in the source with its value.
    /// </summary>
    private static string Substitute(string source, IReadOnlyDictionary<string, string> tokens)
    {
        string result = source;

        foreach ((string token, string value) in tokens)
        {
            result = result.Replace($"{{{{{token}}}}}", value);
        }

        return result;
    }

    /// <summary>
    /// Throws when any <c>{{placeholder}}</c> survived substitution — a missing
    /// token or a resource typo, both programming errors.
    /// </summary>
    private static void EnsureFullyResolved(EnumNotificationType type, string rendered)
    {
        Match leftover = PlaceholderRegex().Match(rendered);

        if (leftover.Success)
        {
            throw new InvalidOperationException(
                $"Notification '{type}' rendered with unresolved placeholder '{leftover.Value}'."
            );
        }
    }

    /// <summary>
    /// Maps a two-letter culture to a <see cref="CultureInfo" />, falling back
    /// to the invariant culture (neutral resources) for unknown values.
    /// </summary>
    private static CultureInfo ResolveCulture(string culture)
    {
        try
        {
            return CultureInfo.GetCultureInfo(culture);
        }
        catch (CultureNotFoundException)
        {
            return CultureInfo.InvariantCulture;
        }
    }

    [GeneratedRegex(@"\{\{[a-zA-Z0-9]+\}\}")]
    private static partial Regex PlaceholderRegex();
}
