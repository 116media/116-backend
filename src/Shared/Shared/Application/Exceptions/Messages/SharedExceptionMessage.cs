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
    /// Error message indicating that an entity was not found by its identifier.
    /// </summary>
    /// <param name="entityName">The display name of the entity.</param>
    /// <param name="key">The identifier value.</param>
    public string EntityNotFoundById(string entityName, object? key) =>
        string.Format(localizer["EntityNotFoundById"], entityName, key);

    /// <summary>
    /// Error message indicating that an entity was not found by a named key.
    /// </summary>
    /// <param name="entityName">The display name of the entity.</param>
    /// <param name="keyName">The name of the lookup key.</param>
    /// <param name="keyValue">The value that was searched for.</param>
    public string EntityNotFoundByKey(string entityName, string keyName, object keyValue) =>
        string.Format(localizer["EntityNotFoundByKey"], entityName, keyName, keyValue);

    /// <summary>
    /// Error message indicating that a provided identifier has an invalid format.
    /// </summary>
    public string InvalidIdentifier() => localizer["InvalidIdentifier"];
}
