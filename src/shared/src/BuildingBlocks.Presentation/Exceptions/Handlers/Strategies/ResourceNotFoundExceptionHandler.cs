using _116.BuildingBlocks.Application.Exceptions;
using _116.BuildingBlocks.Application.Exceptions.Messages;
using _116.BuildingBlocks.Presentation.Exceptions.Handlers.Contracts;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;

namespace _116.BuildingBlocks.Presentation.Exceptions.Handlers.Strategies;

/// <summary>
/// Strategy for handling ResourceNotFoundException instances.
/// </summary>
public sealed class ResourceNotFoundExceptionHandler : BaseExceptionStrategy<ResourceNotFoundException>
{
    /// <inheritdoc />
    public override ProblemDetails CreateProblemDetails(ResourceNotFoundException exception, HttpContext context)
    {
        var msg = context.RequestServices.GetRequiredService<SharedExceptionMessage>();

        return CreateStandardProblemDetails(
            title: nameof(ResourceNotFoundException),
            detail: msg.ResourceNotFound(),
            statusCode: StatusCodes.Status404NotFound,
            context: context
        );
    }
}
