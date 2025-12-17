using _116.Shared.Application.Exceptions.Handlers.Contracts;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace _116.Auth.Application.Shared.Exceptions.Handlers;

/// <summary>
/// Strategy for handling AuthenticationException instances.
/// </summary>
public sealed class AccountNotVerifiedExceptionHandler : BaseExceptionStrategy<AccountNotVerifiedException>
{
    /// <inheritdoc />
    public override ProblemDetails CreateProblemDetails(AccountNotVerifiedException exception, HttpContext context)
    {
        return CreateStandardProblemDetails(
            title: nameof(AccountNotVerifiedException),
            detail: exception.Message,
            statusCode: StatusCodes.Status403Forbidden,
            context: context
        );
    }
}
