using _116.Shared.Application.Exceptions.Handlers.Contracts;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace _116.Identity.Application.Shared.Exceptions.Handlers;

public class RefreshTokenExpiryExceptionHandler : BaseExceptionStrategy<RefreshTokenExpiryException>
{
    public override ProblemDetails CreateProblemDetails(RefreshTokenExpiryException exception, HttpContext context)
    {
        return CreateStandardProblemDetails(
            nameof(RefreshTokenExpiryException),
            detail: exception.Message,
            statusCode: StatusCodes.Status403Forbidden,
            context: context
        );
    }
}
