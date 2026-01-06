using _116.Shared.Application.Exceptions.Handlers.Contracts;
using FluentValidation;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace _116.Shared.Application.Exceptions.Handlers.Strategies;

/// <summary>
/// Strategy for handling FluentValidation ValidationException instances.
/// </summary>
public sealed class ValidationExceptionHandler : BaseExceptionStrategy<ValidationException>
{
    /// <inheritdoc />
    public override ProblemDetails CreateProblemDetails(ValidationException exception, HttpContext context)
    {
        ProblemDetails problemDetails = CreateStandardProblemDetails(
            title: nameof(ValidationException),
            detail: exception.Message,
            statusCode: StatusCodes.Status400BadRequest,
            context: context
        );

        // Add validation errors as extensions
        if (exception.Errors?.Any() == true)
        {
            problemDetails.Extensions["errors"] = exception.Errors;
        }

        return problemDetails;
    }
}
