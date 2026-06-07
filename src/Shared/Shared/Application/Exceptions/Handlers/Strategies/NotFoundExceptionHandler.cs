using _116.Shared.Application.Exceptions.Handlers.Contracts;
using _116.Shared.Application.Exceptions.Messages;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;

namespace _116.Shared.Application.Exceptions.Handlers.Strategies;

/// <summary>
/// Strategy for handling NotFoundException instances.
/// </summary>
public sealed class NotFoundExceptionHandler : BaseExceptionStrategy<NotFoundException>
{
    /// <inheritdoc />
    public override ProblemDetails CreateProblemDetails(NotFoundException exception, HttpContext context)
    {
        string detail = exception.Message;

        if (exception.EntityName is not null)
        {
            var msg = context.RequestServices.GetRequiredService<SharedExceptionMessage>();
            detail = exception.KeyName is not null
                ? msg.EntityNotFoundByKey(exception.EntityName, exception.KeyName, exception.KeyValue!)
                : msg.EntityNotFoundById(exception.EntityName, exception.Key);
        }

        return CreateStandardProblemDetails(
            title: nameof(NotFoundException),
            detail: detail,
            statusCode: StatusCodes.Status404NotFound,
            context: context
        );
    }
}
