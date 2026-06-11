using _116.Identity.Application.Shared.Errors.Messages;
using _116.Identity.Application.Shared.Exceptions;
using _116.Shared.Application.Exceptions;

namespace _116.Identity.Application.Shared.Errors;

/// <summary>
/// Session domain error factory providing simple, readable exception creation.
/// Usage: SessionErrors.InvalidRefreshToken() or SessionErrors.SessionNotFound(sessionId)
/// </summary>
public class SessionErrors(AuthenticationErrorMessage authentication)
{
    /// <summary>
    /// Throws when a refresh token is invalid or expired.
    /// </summary>
    public RefreshTokenExpiryException InvalidRefreshToken()
    {
        return new RefreshTokenExpiryException(authentication.InvalidRefreshToken());
    }

    /// <summary>
    /// Throws when a session is not found.
    /// </summary>
    public NotFoundException SessionNotFound(Guid sessionId)
    {
        return new NotFoundException("Session", key: sessionId);
    }

    /// <summary>
    /// Throws when device ID is missing from the request.
    /// </summary>
    public BadRequestException DeviceIdRequired()
    {
        return new BadRequestException(authentication.DeviceIdRequired());
    }
}
