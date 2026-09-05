using _116.Shared.Application.Exceptions.Handlers.Strategies;
using _116.Shared.Application.Exceptions.Messages;
using AwesomeAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;
using Xunit;

namespace _116.Unit.Tests.Shared.Exceptions.Handlers.Strategies;

/// <summary>
/// Unit tests for <see cref="DbUpdateExceptionStrategy"/>: a lost unique-constraint race answers
/// 409, every other database failure stays a 500, and the response never echoes constraint names.
/// </summary>
public class DbUpdateExceptionStrategyTests
{
    private readonly DbUpdateExceptionStrategy _strategy = new();

    private static DefaultHttpContext CreateContext()
    {
        ServiceProvider provider = new ServiceCollection()
            .AddLogging()
            .AddLocalization()
            .AddScoped<SharedExceptionMessage>()
            .BuildServiceProvider();

        return new DefaultHttpContext
        {
            RequestServices = provider,
            Request = { Path = "/api/test" },
            TraceIdentifier = "test-trace-id",
        };
    }

    /// <summary>
    /// Builds a <see cref="PostgresException" /> carrying the given SQLSTATE.
    /// </summary>
    /// <param name="sqlState">The SQLSTATE to report.</param>
    /// <returns>The provider exception.</returns>
    private static PostgresException PostgresError(string sqlState) =>
        new(
            messageText: "duplicate key value violates unique constraint \"ix_article_likes\"",
            severity: "ERROR",
            invariantSeverity: "ERROR",
            sqlState: sqlState
        );

    [Fact]
    public void ExceptionType_ShouldReturnDbUpdateExceptionType()
    {
        // Act & Assert
        _strategy.ExceptionType.Should().Be(typeof(DbUpdateException));
    }

    [Fact]
    public void CreateProblemDetails_ForAUniqueViolation_ShouldReturn409WithoutTheConstraintName()
    {
        // Arrange
        DefaultHttpContext context = CreateContext();
        var exception = new DbUpdateException("update failed", PostgresError(PostgresErrorCodes.UniqueViolation));

        // Act
        ProblemDetails problem = _strategy.CreateProblemDetails(exception, context);

        // Assert
        problem.Status.Should().Be(StatusCodes.Status409Conflict);
        problem.Title.Should().Be("ConflictException");
        problem.Detail.Should().NotContain("ix_article_likes");
    }

    [Fact]
    public void CreateProblemDetails_ForAnotherSqlState_ShouldStayA500()
    {
        // Arrange — a foreign-key violation is a defect, not a client conflict
        DefaultHttpContext context = CreateContext();
        var exception = new DbUpdateException("update failed", PostgresError(PostgresErrorCodes.ForeignKeyViolation));

        // Act
        ProblemDetails problem = _strategy.CreateProblemDetails(exception, context);

        // Assert
        problem.Status.Should().Be(StatusCodes.Status500InternalServerError);
    }

    [Fact]
    public void CreateProblemDetails_WithoutAPostgresInnerException_ShouldStayA500()
    {
        // Arrange
        DefaultHttpContext context = CreateContext();
        var exception = new DbUpdateException("update failed", new InvalidOperationException("boom"));

        // Act
        ProblemDetails problem = _strategy.CreateProblemDetails(exception, context);

        // Assert
        problem.Status.Should().Be(StatusCodes.Status500InternalServerError);
    }
}
