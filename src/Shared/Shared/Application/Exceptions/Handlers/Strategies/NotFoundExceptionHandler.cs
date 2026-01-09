using _116.Shared.Application.Exceptions.Handlers.Contracts;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace _116.Shared.Application.Exceptions.Handlers.Strategies;

/// <summary>
/// Strategy for handling NotFoundException instances.
/// </summary>
public sealed class NotFoundExceptionHandler : BaseExceptionStrategy<NotFoundException>
{
    /// <inheritdoc />
    public override ProblemDetails CreateProblemDetails(NotFoundException exception, HttpContext context)
    {
        return CreateStandardProblemDetails(
            title: nameof(NotFoundException),
            detail: exception.Message,
            statusCode: StatusCodes.Status404NotFound,
            context: context
        );
    }
}
