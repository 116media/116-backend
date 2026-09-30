using _116.Identity.Infrastructure.Persistence;
using AwesomeAssertions;
using Npgsql;
using Xunit;

namespace _116.Identity.Unit.Tests.Infrastructure.Persistence;

/// <summary>
/// Unit tests for <see cref="PostgresTransientFaultDetector" />: the SQLSTATEs that mean the
/// store was unreachable, and the failures that only look like it.
/// </summary>
public class PostgresTransientFaultDetectorTests
{
    private readonly PostgresTransientFaultDetector _detector = new();

    private static PostgresException PostgresFailure(string sqlState) =>
        new(messageText: "connection failure", severity: "FATAL", invariantSeverity: "FATAL", sqlState: sqlState);

    [Theory]
    [InlineData("08000")]
    [InlineData("08001")]
    [InlineData("08003")]
    [InlineData("08004")]
    [InlineData("08006")]
    [InlineData("08007")]
    [InlineData("57P01")]
    [InlineData("57P02")]
    [InlineData("57P03")]
    public void IsUnreachable_ForAConnectionClassSqlState_ShouldBeTrue(string sqlState)
    {
        _detector.IsUnreachable(PostgresFailure(sqlState)).Should().BeTrue();
    }

    [Theory]
    [InlineData("23505")]
    [InlineData("23503")]
    [InlineData("23502")]
    public void IsUnreachable_ForAConstraintViolation_ShouldBeFalse(string sqlState)
    {
        _detector.IsUnreachable(PostgresFailure(sqlState)).Should().BeFalse();
    }

    [Fact]
    public void IsUnreachable_ForATimeout_ShouldBeTrue()
    {
        _detector.IsUnreachable(new TimeoutException()).Should().BeTrue();
    }

    [Theory]
    [InlineData(typeof(TaskCanceledException))]
    [InlineData(typeof(OperationCanceledException))]
    public void IsUnreachable_ForAClientDisconnect_ShouldBeFalse(Type exceptionType)
    {
        var exception = (Exception)Activator.CreateInstance(exceptionType)!;

        _detector.IsUnreachable(exception).Should().BeFalse();
    }

    [Fact]
    public void IsUnreachable_ForAnUnrelatedFailure_ShouldBeFalse()
    {
        _detector.IsUnreachable(new InvalidOperationException("a defect")).Should().BeFalse();
    }
}
