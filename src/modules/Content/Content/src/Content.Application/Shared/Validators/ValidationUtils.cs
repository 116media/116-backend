using System.Reflection;

namespace _116.Content.Application.Shared.Validators;

/// <summary>
/// Shared validation utility methods used across Content module validation extension classes.
/// </summary>
public static class ValidationUtils
{
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
