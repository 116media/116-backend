using System.Reflection;

namespace _116.Identity.Application.Auth.Validators;

/// <summary>
/// Shared validation utils/helper methods used across validation extension classes.
/// </summary>
public static class ValidationUtils
{
    /// <summary>
    /// Validates that a URL is in proper format.
    /// </summary>
    /// <param name="url">The URL to validate.</param>
    /// <returns>True if the URL is valid or null/empty, false otherwise.</returns>
    public static bool ValidUrl(string? url)
    {
        if (string.IsNullOrWhiteSpace(value: url))
        {
            return true;
        }

        return Uri.TryCreate(uriString: url, uriKind: UriKind.Absolute, out Uri? result) &&
               (result.Scheme == Uri.UriSchemeHttp || result.Scheme == Uri.UriSchemeHttps);
    }

    /// <summary>
    /// Gets a property value from an instance using reflection.
    /// </summary>
    /// <typeparam name="T">The type of the instance.</typeparam>
    /// <param name="instance">The instance to get the property value from.</param>
    /// <param name="propertyName">The name of the property to retrieve.</param>
    /// <returns>The property value as a string, or null if the property is not found or cannot be cast to string.</returns>
    public static string? GetPropertyValue<T>(T instance, string propertyName)
    {
        PropertyInfo? property = typeof(T).GetProperty(name: propertyName);
        return property?.GetValue(obj: instance) as string;
    }
}
