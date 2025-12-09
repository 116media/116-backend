using _116.Shared.Application.Exceptions.Handlers.Contracts;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace _116.Shared.Application.Exceptions.Handlers.Strategies;

/// <summary>
/// Strategy for handling BadGatewayException instances.
/// </summary>
public sealed class BadGatewayExceptionHandler : BaseExceptionStrategy<BadGatewayException>
{
    /// <inheritdoc />
    public override ProblemDetails CreateProblemDetails(BadGatewayException exception, HttpContext context)
    {
        return CreateStandardProblemDetails(
            title: nameof(BadGatewayException),
            detail: exception.Message,
            statusCode: StatusCodes.Status502BadGateway,
            context: context
        );
    }
}
