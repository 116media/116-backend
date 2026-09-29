using _116.BuildingBlocks.Application.Exceptions;
using _116.BuildingBlocks.Presentation.Exceptions.Handlers.Contracts;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace _116.BuildingBlocks.Presentation.Exceptions.Handlers.Strategies;

/// <summary>
/// Strategy for handling AuthenticationException instances.
/// </summary>
public sealed class AuthenticationExceptionHandler : BaseExceptionStrategy<AuthenticationException>
{
    /// <inheritdoc />
    public override ProblemDetails CreateProblemDetails(AuthenticationException exception, HttpContext context)
    {
        return CreateStandardProblemDetails(
            title: nameof(AuthenticationException),
            detail: exception.Message,
            statusCode: StatusCodes.Status401Unauthorized,
            context: context
        );
    }
}
