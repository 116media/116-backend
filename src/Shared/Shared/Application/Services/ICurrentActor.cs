namespace _116.Shared.Application.Services;

/// <summary>
/// Provides information about the actor triggering the current operation.
/// </summary>
public interface ICurrentActor
{
    /// <summary>
    /// The user identifier extracted from the JWT <c>NameIdentifier</c> claim,
    /// or <c>null</c> when the request is unauthenticated or there is no HTTP context.
    /// </summary>
    string? UserId { get; }

    /// <summary>
    /// Whether the current request carries a valid authenticated identity.
    /// </summary>
    bool IsAuthenticated { get; }

    /// <summary>
    /// Whether the operation originates from an HTTP request (authenticated or not).
    /// Returns <c>false</c> for background jobs, seeders, and migrations.
    /// </summary>
    bool HasHttpContext { get; }
}
