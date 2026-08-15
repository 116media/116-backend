using _116.Shared.Application.Exceptions.Handlers.Contracts;
using _116.Shared.Application.Exceptions.Messages;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;

namespace _116.Shared.Application.Exceptions.Handlers.Strategies;

/// <summary>
/// Strategy for client-cancelled requests. A disconnect is not a server error, so it is mapped to
/// 499 (client closed request) rather than a logged 500. The title follows the <c>nameof(TException)</c>
/// convention and the detail comes from the localized <see cref="SharedExceptionMessage"/>.
/// </summary>
public sealed class OperationCanceledExceptionHandler : BaseExceptionStrategy<OperationCanceledException>
{
    private const int StatusClientClosedRequest = 499;

    /// <inheritdoc />
    public override ProblemDetails CreateProblemDetails(OperationCanceledException exception, HttpContext context)
    {
        var msg = context.RequestServices.GetRequiredService<SharedExceptionMessage>();

        return CreateStandardProblemDetails(
            title: nameof(OperationCanceledException),
            detail: msg.RequestCancelled(),
            statusCode: StatusClientClosedRequest,
            context: context
        );
    }
}
