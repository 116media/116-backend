using _116.Shared.Application.Exceptions.Handlers.Contracts;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace _116.Shared.Application.Exceptions.Handlers.Strategies;

/// <summary>
/// Strategy for handling MethodNotAllowedException instances.
/// </summary>
public sealed class MethodNotAllowedExceptionHandler : BaseExceptionStrategy<MethodNotAllowedException>
{
    /// <inheritdoc />
    public override ProblemDetails CreateProblemDetails(MethodNotAllowedException exception, HttpContext context)
    {
        return CreateStandardProblemDetails(
            title: nameof(MethodNotAllowedException),
            detail: exception.Message,
            statusCode: StatusCodes.Status405MethodNotAllowed,
            context: context
        );
    }
}
