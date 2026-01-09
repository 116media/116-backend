namespace _116.Shared.Application.Exceptions;

/// <summary>
/// Exception that represents a method not allowed error, specifically for when a user
/// attempts to access an existing endpoint with an unsupported HTTP method.
/// </summary>
public class MethodNotAllowedException : Exception
{
    /// <summary>
    /// Initializes a new instance of the <see cref="MethodNotAllowedException"/> class for wrong HTTP method.
    /// </summary>
    /// <param name="path">The requested path.</param>
    /// <param name="method">The HTTP method that was attempted.</param>
    /// <example>
    /// <code>
    /// throw new MethodNotAllowedException("/api/v1/public/auth/verify-otp", "GET");
    /// // Output: The HTTP method 'GET' is not allowed for '/api/v1/public/auth/verify-otp'
    /// </code>
    /// </example>
    public MethodNotAllowedException(string path, string method)
        : base($"The HTTP method '{method}' is not allowed for '{path}'") { }

    /// <summary>
    /// Initializes a new instance of the <see cref="MethodNotAllowedException"/> class with a custom message.
    /// </summary>
    /// <param name="message">The custom error message.</param>
    /// <example>
    /// <code>
    /// throw new MethodNotAllowedException("Custom method not allowed message");
    /// </code>
    /// </example>
    public MethodNotAllowedException(string message)
        : base(message) { }
}
