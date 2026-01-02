using _116.Identity.Application.Shared.Errors.Messages;
using _116.Shared.Application.Exceptions;

namespace _116.Identity.Application.Shared.Errors;

/// <summary>
/// Session domain error factory providing simple, readable exception creation.
/// Usage: SessionErrors.InvalidRefreshToken() or SessionErrors.SessionNotFound(sessionId)
/// </summary>
public static class SessionErrors
{
    /// <summary>
    /// Throws when a refresh token is invalid or expired.
    /// </summary>
    public static AuthorizationException InvalidRefreshToken()
    {
        return new AuthorizationException(AuthenticationErrorMessage.InvalidRefreshToken());
    }

    /// <summary>
    /// Throws when a session is not found.
    /// </summary>
    public static NotFoundException SessionNotFound(Guid sessionId)
    {
        return new NotFoundException("Session", key: sessionId);
    }
}
