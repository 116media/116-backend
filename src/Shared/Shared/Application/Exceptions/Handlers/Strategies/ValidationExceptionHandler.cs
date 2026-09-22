using System.Text.Json;
using _116.Shared.Application.Exceptions.Handlers.Contracts;
using _116.Shared.Application.Exceptions.Messages;
using FluentValidation;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;

namespace _116.Shared.Application.Exceptions.Handlers.Strategies;

/// <summary>
/// Strategy for handling FluentValidation ValidationException instances. Renders the failures as
/// one camelCase entry per field carrying messages only; the submitted value is never echoed,
/// neither in the errors nor in the detail.
/// </summary>
public sealed class ValidationExceptionHandler : BaseExceptionStrategy<ValidationException>
{
    /// <inheritdoc />
    public override ProblemDetails CreateProblemDetails(ValidationException exception, HttpContext context)
    {
        var msg = context.RequestServices.GetRequiredService<SharedExceptionMessage>();

        ProblemDetails problemDetails = CreateStandardProblemDetails(
            title: nameof(ValidationException),
            detail: msg.ValidationFailed(),
            statusCode: StatusCodes.Status400BadRequest,
            context: context
        );

        if (exception.Errors?.Any() == true)
        {
            problemDetails.Extensions["errors"] = exception
                .Errors.GroupBy(failure => failure.PropertyName)
                .ToDictionary(
                    group => JsonNamingPolicy.CamelCase.ConvertName(group.Key),
                    group => group.Select(failure => failure.ErrorMessage).Distinct().ToArray()
                );
        }

        return problemDetails;
    }
}
