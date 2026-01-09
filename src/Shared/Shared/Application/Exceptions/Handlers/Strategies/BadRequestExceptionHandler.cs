using _116.Shared.Application.Exceptions.Handlers.Contracts;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace _116.Shared.Application.Exceptions.Handlers.Strategies;

/// <summary>
/// Strategy for handling BadRequestException instances.
/// </summary>
public sealed class BadRequestExceptionHandler : BaseExceptionStrategy<BadRequestException>
{
    /// <inheritdoc />
    public override ProblemDetails CreateProblemDetails(BadRequestException exception, HttpContext context)
    {
        return CreateStandardProblemDetails(
            title: nameof(BadRequestException),
            detail: exception.Message,
            statusCode: StatusCodes.Status400BadRequest,
            context: context
        );
    }
}
