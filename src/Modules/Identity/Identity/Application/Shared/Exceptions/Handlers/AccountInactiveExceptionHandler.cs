using _116.Shared.Application.Exceptions.Handlers.Contracts;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace _116.Identity.Application.Shared.Exceptions.Handlers;

/// <summary>
/// Strategy for handling <see cref="AccountInactiveException"/> instances.
/// </summary>
public sealed class AccountInactiveExceptionHandler : BaseExceptionStrategy<AccountInactiveException>
{
    /// <inheritdoc />
    public override ProblemDetails CreateProblemDetails(AccountInactiveException exception, HttpContext context)
    {
        return CreateStandardProblemDetails(
            title: nameof(AccountInactiveException),
            detail: exception.Message,
            statusCode: StatusCodes.Status423Locked,
            context: context
        );
    }
}
