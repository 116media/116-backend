using Microsoft.Extensions.Localization;

namespace _116.Mailer.Application.Notifications.Messages;

/// <summary>
/// Localizer facade for in-app notification resources. Keys follow the
/// <c>&lt;Type&gt;Title</c> / <c>&lt;Type&gt;Body</c> convention; the renderer
/// addresses them by type name rather than through per-type methods, because
/// the catalog is uniform by construction. A missing resource throws — a
/// catalog member without copy is a programming error, never a runtime state.
/// </summary>
/// <param name="localizer">The string localizer bound to this resource set.</param>
public class NotificationMessage(IStringLocalizer<NotificationMessage> localizer)
{
    /// <summary>
    /// Gets the localized title source for a notification type.
    /// </summary>
    /// <param name="type">The notification catalog name.</param>
    /// <returns>The title resource with unresolved tokens.</returns>
    public string Title(string type)
    {
        return Resolve($"{type}Title");
    }

    /// <summary>
    /// Gets the localized body source for a notification type.
    /// </summary>
    /// <param name="type">The notification catalog name.</param>
    /// <returns>The body resource with unresolved tokens.</returns>
    public string Body(string type)
    {
        return Resolve($"{type}Body");
    }

    /// <summary>
    /// Resolves a resource key, failing loudly when it exists in no culture —
    /// reserved catalog members must never render as their raw key name.
    /// </summary>
    private string Resolve(string key)
    {
        LocalizedString resource = localizer[key];

        if (resource.ResourceNotFound)
        {
            throw new InvalidOperationException($"Notification resource '{key}' is missing.");
        }

        return resource;
    }
}
