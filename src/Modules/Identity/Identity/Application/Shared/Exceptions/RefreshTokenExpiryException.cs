using _116.Shared.Application.Exceptions;

namespace _116.Identity.Application.Shared.Exceptions;

public sealed class RefreshTokenExpiryException : AuthorizationException
{
    public RefreshTokenExpiryException(string message)
        : base(message) { }
}
