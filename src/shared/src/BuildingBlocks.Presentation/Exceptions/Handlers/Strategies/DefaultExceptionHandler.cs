using _116.BuildingBlocks.Application.Exceptions;
using _116.BuildingBlocks.Application.Exceptions.Messages;
using _116.BuildingBlocks.Presentation.Exceptions.Handlers.Contracts;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace _116.BuildingBlocks.Presentation.Exceptions.Handlers.Strategies;

/// <summary>
/// Default strategy for unregistered exception types (the base <see cref="Exception"/> fallback).
/// Outside Development the exception message is withheld: unmapped exceptions (Npgsql, EF, SDK errors)
/// carry connection strings, SQL and schema detail that must never reach a client. The detail is taken
/// from the localized <see cref="SharedExceptionMessage"/>, matching every other strategy.
/// </summary>
public sealed class DefaultExceptionHandler : IExceptionStrategy
{
    /// <inheritdoc />
    public Type ExceptionType => typeof(Exception);

    /// <inheritdoc />
    public ProblemDetails CreateProblemDetails(Exception exception, HttpContext context)
    {
        IHostEnvironment environment = context.RequestServices.GetRequiredService<IHostEnvironment>();
        bool isDevelopment = environment.IsDevelopment();

        string detail = isDevelopment
            ? exception.Message
            : context.RequestServices.GetRequiredService<SharedExceptionMessage>().UnexpectedError();

        return new ProblemDetails
        {
            Title = isDevelopment ? exception.GetType().Name : nameof(InternalServerException),
            Detail = detail,
            Status = StatusCodes.Status500InternalServerError,
            Instance = context.Request.Path,
            Extensions = { ["traceId"] = context.TraceIdentifier, ["timestamp"] = DateTime.UtcNow },
        };
    }
}
