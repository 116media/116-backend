using _116.Shared.Application.Exceptions.Handlers.Contracts;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace _116.Auth.Application.Shared.Exceptions.Handlers;

/// <summary>
/// Strategy for handling AccessDeniedException instances.
/// </summary>
public sealed class AccessDeniedExceptionHandler : BaseExceptionStrategy<AccessDeniedException>
{
    /// <inheritdoc />
    public override ProblemDetails CreateProblemDetails(AccessDeniedException exception, HttpContext context)
    {
        return CreateStandardProblemDetails(
            title: nameof(AccessDeniedException),
            detail: exception.Message,
            statusCode: StatusCodes.Status403Forbidden,
            context: context
        );
    }
}

