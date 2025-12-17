using _116.Shared.Application.Exceptions.Handlers.Contracts;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace _116.Identity.Application.Shared.Exceptions.Handlers;

/// <summary>
/// Strategy for handling <see cref="UserNotLoggedInException"/> instances.
/// </summary>
public sealed class UserNotLoggedInExceptionHandler : BaseExceptionStrategy<UserNotLoggedInException>
{
    /// <inheritdoc />
    public override ProblemDetails CreateProblemDetails(UserNotLoggedInException exception, HttpContext context)
    {
        return CreateStandardProblemDetails(
            title: nameof(UserNotLoggedInException),
            detail: exception.Message,
            statusCode: StatusCodes.Status403Forbidden,
            context: context
        );
    }
}
