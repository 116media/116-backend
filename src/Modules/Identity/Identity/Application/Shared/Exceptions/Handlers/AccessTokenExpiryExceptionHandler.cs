using _116.Shared.Application.Exceptions.Handlers.Contracts;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace _116.Identity.Application.Shared.Exceptions.Handlers;

public class AccessTokenExpiryExceptionHandler : BaseExceptionStrategy<AccessTokenExpiryException>
{
    public override ProblemDetails CreateProblemDetails(AccessTokenExpiryException exception, HttpContext context)
    {
        return CreateStandardProblemDetails(
            nameof(AccessTokenExpiryException),
            detail: exception.Message,
            statusCode: StatusCodes.Status401Unauthorized,
            context: context
        );
    }
}
