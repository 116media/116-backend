using _116.BuildingBlocks.Application.Exceptions;

namespace _116.Identity.Application.Shared.Exceptions;

public class AccessTokenExpiryException : AuthenticationException
{
    public AccessTokenExpiryException(string message)
        : base(message) { }
}
