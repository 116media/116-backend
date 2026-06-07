using _116.Shared.Application.Exceptions.Handlers.Contracts;
using _116.Shared.Application.Exceptions.Messages;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;

namespace _116.Shared.Application.Exceptions.Handlers.Strategies;

/// <summary>
/// Strategy for handling <see cref="FormatException" /> instances, typically thrown when a route
/// parameter cannot be parsed as a valid UUID.
/// </summary>
public sealed class FormatExceptionStrategy : BaseExceptionStrategy<FormatException>
{
    /// <inheritdoc />
    public override ProblemDetails CreateProblemDetails(FormatException exception, HttpContext context)
    {
        var msg = context.RequestServices.GetRequiredService<SharedExceptionMessage>();

        return CreateStandardProblemDetails(
            title: nameof(InvalidFormatException),
            detail: msg.InvalidIdentifier(),
            statusCode: StatusCodes.Status400BadRequest,
            context: context
        );
    }
}
