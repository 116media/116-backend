using System.Globalization;
using System.Text.RegularExpressions;
using _116.Mailer.Application.Notifications.Messages;
using _116.Mailer.Application.Shared.Services;
using _116.Mailer.Contracts.Domain.Enums;

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
        string locale
    )
    {
        CultureInfo previous = CultureInfo.CurrentUICulture;

        try
        {
            CultureInfo.CurrentUICulture = ResolveCulture(locale);

            string name = type.ToString();
            string titleTemplate = messages.Title(name);
            string bodyTemplate = messages.Body(name);

            EnsureEveryPlaceholderHasAToken(type, titleTemplate, tokens);
            EnsureEveryPlaceholderHasAToken(type, bodyTemplate, tokens);

            string title = Substitute(titleTemplate, tokens);
            string body = Substitute(bodyTemplate, tokens);

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
    /// Throws when the template declares a <c>{{placeholder}}</c> no token supplies. Checked
    /// against the template before substitution, so token values containing <c>{{text}}</c>
    /// are delivered literally rather than mistaken for an unresolved placeholder.
    /// </summary>
    private static void EnsureEveryPlaceholderHasAToken(
        EnumNotificationType type,
        string source,
        IReadOnlyDictionary<string, string> tokens
    )
    {
        foreach (Match placeholder in PlaceholderRegex().Matches(source))
        {
            string name = placeholder.Value[2..^2];

            if (!tokens.ContainsKey(name))
            {
                throw new InvalidOperationException(
                    $"Notification '{type}' declares placeholder '{placeholder.Value}' with no token."
                );
            }
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
