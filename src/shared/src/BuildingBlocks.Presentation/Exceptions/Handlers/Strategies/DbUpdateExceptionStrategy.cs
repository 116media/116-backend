using _116.BuildingBlocks.Application.Exceptions;
using _116.BuildingBlocks.Application.Exceptions.Messages;
using _116.BuildingBlocks.Application.Persistence;
using _116.BuildingBlocks.Presentation.Exceptions.Handlers.Contracts;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace _116.BuildingBlocks.Presentation.Exceptions.Handlers.Strategies;

/// <summary>
/// Strategy for <see cref="DbUpdateException" />: a unique-constraint violation is a lost
/// check-then-act race (two concurrent identical writes), answered as 409 so retrying clients see
/// the same conflict the application-level pre-checks report. Every other database failure stays a
/// 500 — a foreign-key or not-null violation is a defect, and demoting it would hide bugs.
/// </summary>
public sealed class DbUpdateExceptionStrategy : BaseExceptionStrategy<DbUpdateException>
{
    /// <inheritdoc />
    public override ProblemDetails CreateProblemDetails(DbUpdateException exception, HttpContext context)
    {
        var msg = context.RequestServices.GetRequiredService<SharedExceptionMessage>();
        var detector = context.RequestServices.GetRequiredService<IUniqueConstraintDetector>();

        // The detail never echoes the constraint or column names the driver reports.
        bool isUniqueViolation = detector.IsUniqueConstraintViolation(exception);

        return CreateStandardProblemDetails(
            title: isUniqueViolation ? "ConflictException" : nameof(DbUpdateException),
            detail: isUniqueViolation ? msg.DuplicateResourceConflict() : msg.UnexpectedError(),
            statusCode: isUniqueViolation ? StatusCodes.Status409Conflict : StatusCodes.Status500InternalServerError,
            context: context
        );
    }
}
