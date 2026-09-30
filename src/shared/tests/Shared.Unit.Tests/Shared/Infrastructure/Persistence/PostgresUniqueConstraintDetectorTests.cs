using _116.BuildingBlocks.Infrastructure.Persistence;
using AwesomeAssertions;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using Xunit;

namespace _116.Shared.Unit.Tests.Shared.Infrastructure.Persistence;

/// <summary>
/// Unit tests for <see cref="PostgresUniqueConstraintDetector" />: SQLSTATE 23505 is the lost
/// uniqueness race the strategy answers with 409, every other failure is a defect.
/// </summary>
public class PostgresUniqueConstraintDetectorTests
{
    private readonly PostgresUniqueConstraintDetector _detector = new();

    private static DbUpdateException Wrapping(string sqlState) =>
        new(
            "save failed",
            new PostgresException(
                messageText: "duplicate key value violates unique constraint",
                severity: "ERROR",
                invariantSeverity: "ERROR",
                sqlState: sqlState
            )
        );

    [Fact]
    public void IsUniqueConstraintViolation_ForSqlState23505_ShouldBeTrue()
    {
        _detector.IsUniqueConstraintViolation(Wrapping("23505")).Should().BeTrue();
    }

    [Theory]
    [InlineData("23503")]
    [InlineData("23502")]
    [InlineData("08006")]
    public void IsUniqueConstraintViolation_ForAnyOtherSqlState_ShouldBeFalse(string sqlState)
    {
        _detector.IsUniqueConstraintViolation(Wrapping(sqlState)).Should().BeFalse();
    }

    [Fact]
    public void IsUniqueConstraintViolation_WithNoInnerException_ShouldBeFalse()
    {
        _detector.IsUniqueConstraintViolation(new DbUpdateException("save failed")).Should().BeFalse();
    }

    [Fact]
    public void IsUniqueConstraintViolation_ForAnUnrelatedFailure_ShouldBeFalse()
    {
        _detector.IsUniqueConstraintViolation(new InvalidOperationException("a defect")).Should().BeFalse();
    }
}
