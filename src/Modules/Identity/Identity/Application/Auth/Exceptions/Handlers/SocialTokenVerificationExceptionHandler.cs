using _116.Identity.Application.Shared.Errors.Messages;
using _116.Shared.Application.Exceptions.Handlers.Contracts;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;

namespace _116.Identity.Application.Auth.Exceptions.Handlers;

/// <summary>
/// Strategy for a social provider token that failed verification. Maps to 401 with a localized,
/// non-revealing detail resolved from <see cref="AuthenticationErrorMessage" />.
/// </summary>
public sealed class SocialTokenVerificationExceptionHandler : BaseExceptionStrategy<SocialTokenVerificationException>
{
    /// <inheritdoc />
    public override ProblemDetails CreateProblemDetails(SocialTokenVerificationException exception, HttpContext context)
    {
        var msg = context.RequestServices.GetRequiredService<AuthenticationErrorMessage>();

        return CreateStandardProblemDetails(
            context: context,
            detail: msg.InvalidProviderToken(),
            title: nameof(SocialTokenVerificationException),
            statusCode: StatusCodes.Status401Unauthorized
        );
    }
}
