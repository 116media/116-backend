using _116.Shared.Application.Exceptions.Handlers.Contracts;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace _116.Shared.Application.Exceptions.Handlers.Strategies;

/// <summary>
/// Strategy for handling ConflictException instances.
/// </summary>
public sealed class ConflictExceptionHandler : BaseExceptionStrategy<ConflictException>
{
    /// <inheritdoc />
    public override ProblemDetails CreateProblemDetails(ConflictException exception, HttpContext context)
    {
        return CreateStandardProblemDetails(
            title: nameof(ConflictException),
            detail: exception.Message,
            statusCode: StatusCodes.Status409Conflict,
            context: context
        );
    }
}
