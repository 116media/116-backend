using _116.Identity.Application.Shared.Errors.Messages;
using _116.Shared.Application.Exceptions.Handlers.Contracts;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;

namespace _116.Identity.Application.Auth.Exceptions.Handlers;

/// <summary>
/// Strategy for an unsupported social provider. Maps to 400 with a localized detail resolved from
/// <see cref="ValidationErrorMessage" />, naming the provider that has no verifier.
/// </summary>
public sealed class UnsupportedProviderExceptionHandler : BaseExceptionStrategy<UnsupportedProviderException>
{
    /// <inheritdoc />
    public override ProblemDetails CreateProblemDetails(UnsupportedProviderException exception, HttpContext context)
    {
        var msg = context.RequestServices.GetRequiredService<ValidationErrorMessage>();

        return CreateStandardProblemDetails(
            context: context,
            statusCode: StatusCodes.Status400BadRequest,
            title: nameof(UnsupportedProviderException),
            detail: msg.UnsupportedProvider(exception.Provider.ToString())
        );
    }
}
