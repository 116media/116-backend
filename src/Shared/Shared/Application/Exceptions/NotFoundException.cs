namespace _116.Shared.Application.Exceptions;

/// <summary>
/// Exception that represents a not found error, typically indicating that a requested resource does not exist.
/// </summary>
public class NotFoundException : Exception
{
    /// <summary>
    /// Gets additional details about the not found error, if provided.
    /// </summary>
    public string? Details { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="NotFoundException"/> class with a custom error message.
    /// </summary>
    /// <param name="message">The message that describes the error.</param>
    /// <example>
    /// <code>
    /// throw new NotFoundException("Custom not found message");
    /// </code>
    /// </example>
    public NotFoundException(string message)
        : base(message) { }

    /// <summary>
    /// Initializes a new instance of the <see cref="NotFoundException"/> class with a custom message and additional details.
    /// </summary>
    /// <param name="message">The error message that describes the not found error.</param>
    /// <param name="details">Additional context or information about the not found error.</param>
    public NotFoundException(string message, string details)
        : base(message)
    {
        Details = details;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="NotFoundException"/> class with an entity name and a key.
    /// Automatically removes the word "Entity" from the entity name if present.
    /// </summary>
    /// <param name="entityName">The name of the entity type.</param>
    /// <param name="key">The unique identifier of the entity that was not found.</param>
    /// <example>
    /// <code>
    /// throw new NotFoundException("Product", 42);
    /// // Output: Could not find Product with id: 42
    /// </code>
    /// </example>
    public NotFoundException(string entityName, object? key)
        : base($"Could not find {CleanEntityName(entityName)} with id: {key}") { }

    /// <summary>
    /// Initializes a new instance of the <see cref="NotFoundException"/>
    /// class with an entity name, a key name, and a key value.
    /// Useful for scenarios where the key is not just an ID (e.g., username, email).
    /// </summary>
    /// <param name="entityName">The name of the entity type.</param>
    /// <param name="keyName">The name of the key or property used in the lookup.</param>
    /// <param name="keyValue">The value of the key that was used to find the entity.</param>
    /// <example>
    /// <code>
    /// throw new NotFoundException("Basket", "username", "jean.vincent");
    /// // Output: Could not find Basket with username: jean.vincent
    /// </code>
    /// </example>
    public NotFoundException(string entityName, string keyName, object keyValue)
        : base($"Could not find {CleanEntityName(entityName)} with {keyName}: {keyValue}") { }

    /// <summary>
    /// Removes the word "Entity" (case-insensitive) from the entity name, if present.
    /// </summary>
    /// <param name="name">The original entity name.</param>
    /// <returns>The cleaned entity name.</returns>
    private static string CleanEntityName(string name)
    {
        return name.Replace("entity", "", StringComparison.OrdinalIgnoreCase).Trim();
    }
}
