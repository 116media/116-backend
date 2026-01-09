using _116.Shared.Application.Exceptions.Handlers.Contracts;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace _116.Shared.Application.Exceptions.Handlers.Strategies;

/// <summary>
/// Strategy for handling AuthorizationException instances.
/// </summary>
public sealed class AuthorizationExceptionHandler : BaseExceptionStrategy<AuthorizationException>
{
    /// <inheritdoc />
    public override ProblemDetails CreateProblemDetails(AuthorizationException exception, HttpContext context)
    {
        return CreateStandardProblemDetails(
            title: nameof(AuthorizationException),
            detail: exception.Message,
            statusCode: StatusCodes.Status403Forbidden,
            context: context
        );
    }
}
