using _116.Identity.Application.Session.Factories;

namespace _116.Unit.Tests.Common.Helpers;

/// <summary>
/// Shared test helpers for authentication and session-related tests.
/// </summary>
public static class AuthTestHelpers
{
    /// <summary>
    /// Creates a default SessionResult for testing purposes.
    /// </summary>
    public static SessionResult CreateDefaultSessionResult()
    {
        return new SessionResult(
            RefreshToken: "refresh-token",
            AccessToken: "access-token",
            AccessTokenExpiresAt: DateTime.UtcNow.AddHours(1),
            RefreshTokenExpiresAt: DateTime.UtcNow.AddDays(7)
        );
    }
}
