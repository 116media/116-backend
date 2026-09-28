using _116.BuildingBlocks.Application.Exceptions;
using _116.BuildingBlocks.Presentation.Exceptions.Handlers.Contracts;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace _116.BuildingBlocks.Presentation.Exceptions.Handlers.Strategies;

/// <summary>
/// Strategy for handling InternalServerException instances.
/// </summary>
public sealed class InternalServerExceptionHandler : BaseExceptionStrategy<InternalServerException>
{
    /// <inheritdoc />
    public override ProblemDetails CreateProblemDetails(InternalServerException exception, HttpContext context)
    {
        return CreateStandardProblemDetails(
            title: nameof(InternalServerException),
            detail: exception.Message,
            statusCode: StatusCodes.Status500InternalServerError,
            context: context
        );
    }
}
