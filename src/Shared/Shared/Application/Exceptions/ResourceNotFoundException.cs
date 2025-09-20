namespace _116.Shared.Application.Exceptions;

/// <summary>
/// Exception that represents a resource not found error, specifically for non-existing API endpoints.
/// This exception is thrown when a user attempts to access an endpoint that doesn't exist in the API.
/// </summary>
public class ResourceNotFoundException : NotFoundException
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ResourceNotFoundException"/> class for non-existing endpoints.
    /// </summary>
    /// <param name="path">The requested path that doesn't exist.</param>
    /// <param name="method">The HTTP method used for the request.</param>
    /// <example>
    /// <code>
    /// throw new ResourceNotFoundException("/api/v1/nonexistent", "GET");
    /// // Output: The requested resource 'GET /api/v1/nonexistent' was not found.
    /// </code>
    /// </example>
    public ResourceNotFoundException(string path, string method)
        : base($"The requested resource '{method} {path}' was not found.")
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ResourceNotFoundException"/> class with a custom message.
    /// </summary>
    /// <param name="message">The custom error message.</param>
    /// <example>
    /// <code>
    /// throw new ResourceNotFoundException("Custom resource not found message");
    /// </code>
    /// </example>
    public ResourceNotFoundException(string message) : base(message) { }
}
