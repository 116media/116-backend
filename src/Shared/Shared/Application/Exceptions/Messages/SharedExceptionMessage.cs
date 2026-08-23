using Microsoft.Extensions.Localization;

namespace _116.Shared.Application.Exceptions.Messages;

/// <summary>
/// Provides localized error messages for shared exception types
/// such as rate limiting, not found, and method not allowed.
/// </summary>
public class SharedExceptionMessage(IStringLocalizer<SharedExceptionMessage> localizer)
{
    /// <summary>
    /// Exposes the underlying localizer for shared extensions.
    /// </summary>
    public IStringLocalizer Localizer => localizer;

    /// <summary>
    /// Error message indicating that the rate limit has been exceeded.
    /// </summary>
    /// <param name="seconds">The number of seconds before the client can retry.</param>
    public string RateLimitExceeded(int seconds) => string.Format(localizer["RateLimitExceeded"], seconds);

    /// <summary>
    /// User-friendly, localized "not found" message for an entity. Resolves the entity
    /// to a friendly localized label (e.g. "User" -> "the requested user account") and
    /// never exposes the raw entity class name, the lookup key, or the searched value —
    /// those stay in the exception message for logs only.
    /// </summary>
    /// <param name="entityName">The entity's technical name (e.g. "User", "Article").</param>
    /// <returns>A friendly, localized not-found message.</returns>
    public string EntityNotFound(string entityName) =>
        string.Format(localizer["EntityNotFound"], ResolveEntityLabel(entityName));

    /// <summary>
    /// User-friendly, localized "not found" message for a request that matched no API route.
    /// Never echoes the requested method or path — those stay in the exception message for
    /// logs only.
    /// </summary>
    /// <returns>A friendly, localized resource-not-found message.</returns>
    public string ResourceNotFound() => localizer["ResourceNotFound"];

    /// <summary>
    /// Error message indicating that a provided identifier has an invalid format.
    /// </summary>
    public string InvalidIdentifier() => localizer["InvalidIdentifier"];

    /// <summary>
    /// Maps a technical entity name to its friendly localized label (keyed
    /// <c>Entity_{entityName}</c> in the resources), falling back to a generic
    /// "requested resource" label when no specific label is defined.
    /// </summary>
    /// <param name="entityName">The entity's technical name.</param>
    /// <returns>The friendly localized label to interpolate into the message.</returns>
    private string ResolveEntityLabel(string entityName)
    {
        LocalizedString label = localizer[$"Entity_{entityName}"];
        return label.ResourceNotFound ? localizer["Entity_Default"] : label.Value;
    }
}
