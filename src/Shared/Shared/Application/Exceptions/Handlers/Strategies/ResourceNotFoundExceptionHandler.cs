using _116.Shared.Application.Exceptions.Handlers.Contracts;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace _116.Shared.Application.Exceptions.Handlers.Strategies;

/// <summary>
/// Strategy for handling ResourceNotFoundException instances.
/// </summary>
public sealed class ResourceNotFoundExceptionHandler : BaseExceptionStrategy<ResourceNotFoundException>
{
    /// <inheritdoc />
    public override ProblemDetails CreateProblemDetails(ResourceNotFoundException exception, HttpContext context)
    {
        return CreateStandardProblemDetails(
            title: nameof(ResourceNotFoundException),
            detail: exception.Message,
            statusCode: StatusCodes.Status404NotFound,
            context: context
        );
    }
}
