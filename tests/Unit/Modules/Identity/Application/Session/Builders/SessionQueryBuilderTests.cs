using _116.Identity.Application.Session.Builders;
using _116.Identity.Domain.Entities;
using _116.Shared.Application.Specifications;
using AwesomeAssertions;
using Xunit;

namespace _116.Unit.Tests.Modules.Identity.Application.Session.Builders;

/// <summary>
/// Unit tests for <see cref="SessionQueryBuilder"/>.
/// </summary>
public class SessionQueryBuilderTests
{
    private static readonly Guid TestUserId = Guid.NewGuid();
    private static readonly DateTime TestDate = DateTime.UtcNow;

    #region Builder Pattern Tests

    [Fact]
    public void WithUserId_ShouldReturnBuilder()
    {
        // Arrange
        SessionQueryBuilder builder = new();

        // Act
        ISessionQueryBuilder result = builder.WithUserId(TestUserId);

        // Assert
        result.Should().Be(builder, "fluent interface should return the same builder instance");
    }

    [Fact]
    public void WithStatus_ShouldReturnBuilder()
    {
        // Arrange
        SessionQueryBuilder builder = new();

        // Act
        ISessionQueryBuilder result = builder.WithStatus("Active");

        // Assert
        result.Should().Be(builder, "fluent interface should return the same builder instance");
    }

    [Fact]
    public void WithIpAddress_ShouldReturnBuilder()
    {
        // Arrange
        SessionQueryBuilder builder = new();

        // Act
        ISessionQueryBuilder result = builder.WithIpAddress("192.168.1.1");

        // Assert
        result.Should().Be(builder, "fluent interface should return the same builder instance");
    }

    [Fact]
    public void WithFromDate_ShouldReturnBuilder()
    {
        // Arrange
        SessionQueryBuilder builder = new();

        // Act
        ISessionQueryBuilder result = builder.WithFromDate(TestDate);

        // Assert
        result.Should().Be(builder, "fluent interface should return the same builder instance");
    }

    [Fact]
    public void WithToDate_ShouldReturnBuilder()
    {
        // Arrange
        SessionQueryBuilder builder = new();

        // Act
        ISessionQueryBuilder result = builder.WithToDate(TestDate);

        // Assert
        result.Should().Be(builder, "fluent interface should return the same builder instance");
    }

    [Fact]
    public void WithActiveStatus_ShouldReturnBuilder()
    {
        // Arrange
        SessionQueryBuilder builder = new();

        // Act
        ISessionQueryBuilder result = builder.WithActiveStatus(true);

        // Assert
        result.Should().Be(builder, "fluent interface should return the same builder instance");
    }

    #endregion

    #region Build - Empty Builder Tests

    [Fact]
    public void Build_WithNoFilters_ShouldReturnNull()
    {
        // Arrange
        SessionQueryBuilder builder = new();

        // Act
        Specification<SessionEntity>? specification = builder.Build();

        // Assert
        specification.Should().BeNull("no filters were added to the builder");
    }

    #endregion

    #region WithUserId Tests

    [Fact]
    public void WithUserId_WithValidUserId_ShouldBuildSpecification()
    {
        // Arrange
        SessionQueryBuilder builder = new();

        // Act
        Specification<SessionEntity>? specification = builder.WithUserId(TestUserId).Build();

        // Assert
        specification.Should().NotBeNull("user ID filter was provided");
    }

    [Fact]
    public void WithUserId_WithEmptyGuid_ShouldBuildSpecification()
    {
        // Arrange
        SessionQueryBuilder builder = new();

        // Act - Empty GUID is still a valid value for the filter
        Specification<SessionEntity>? specification = builder.WithUserId(Guid.Empty).Build();

        // Assert
        specification.Should().NotBeNull("even empty GUID creates a specification");
    }

    #endregion

    #region WithStatus Tests

    [Fact]
    public void WithStatus_WithActiveStatus_ShouldBuildSpecification()
    {
        // Arrange
        SessionQueryBuilder builder = new();

        // Act
        Specification<SessionEntity>? specification = builder.WithStatus("Active").Build();

        // Assert
        specification.Should().NotBeNull("active status filter was provided");
    }

    [Fact]
    public void WithStatus_WithExpiredStatus_ShouldBuildSpecification()
    {
        // Arrange
        SessionQueryBuilder builder = new();

        // Act
        Specification<SessionEntity>? specification = builder.WithStatus("Expired").Build();

        // Assert
        specification.Should().NotBeNull("expired status filter was provided");
    }

    [Fact]
    public void WithStatus_WithMixedCaseActive_ShouldBuildSpecification()
    {
        // Arrange
        SessionQueryBuilder builder = new();

        // Act
        Specification<SessionEntity>? specification = builder.WithStatus("ACTIVE").Build();

        // Assert
        specification.Should().NotBeNull("status comparison should be case-insensitive");
    }

    [Fact]
    public void WithStatus_WithMixedCaseExpired_ShouldBuildSpecification()
    {
        // Arrange
        SessionQueryBuilder builder = new();

        // Act
        Specification<SessionEntity>? specification = builder.WithStatus("expired").Build();

        // Assert
        specification.Should().NotBeNull("status comparison should be case-insensitive");
    }

    [Fact]
    public void WithStatus_WithInvalidStatus_ShouldNotBuildSpecification()
    {
        // Arrange
        SessionQueryBuilder builder = new();

        // Act - Invalid status returns null! in the switch expression, resulting in no specification
        Specification<SessionEntity>? specification = builder.WithStatus("InvalidStatus").Build();

        // Assert
        specification.Should().BeNull("invalid status value does not create a specification");
    }

    [Fact]
    public void WithStatus_WithNullStatus_ShouldNotBuildSpecification()
    {
        // Arrange
        SessionQueryBuilder builder = new();

        // Act
        Specification<SessionEntity>? specification = builder.WithStatus(null).Build();

        // Assert
        specification.Should().BeNull("null status should be ignored");
    }

    [Fact]
    public void WithStatus_WithEmptyStatus_ShouldNotBuildSpecification()
    {
        // Arrange
        SessionQueryBuilder builder = new();

        // Act
        Specification<SessionEntity>? specification = builder.WithStatus(string.Empty).Build();

        // Assert
        specification.Should().BeNull("empty status should be ignored");
    }

    [Fact]
    public void WithStatus_WithWhitespaceStatus_ShouldNotBuildSpecification()
    {
        // Arrange
        SessionQueryBuilder builder = new();

        // Act
        Specification<SessionEntity>? specification = builder.WithStatus("   ").Build();

        // Assert
        specification.Should().BeNull("whitespace status should be ignored");
    }

    #endregion

    #region WithIpAddress Tests

    [Fact]
    public void WithIpAddress_WithValidIpAddress_ShouldBuildSpecification()
    {
        // Arrange
        SessionQueryBuilder builder = new();

        // Act
        Specification<SessionEntity>? specification = builder.WithIpAddress("192.168.1.1").Build();

        // Assert
        specification.Should().NotBeNull("IP address filter was provided");
    }

    [Fact]
    public void WithIpAddress_WithNullIpAddress_ShouldNotBuildSpecification()
    {
        // Arrange
        SessionQueryBuilder builder = new();

        // Act
        Specification<SessionEntity>? specification = builder.WithIpAddress(null).Build();

        // Assert
        specification.Should().BeNull("null IP address should be ignored");
    }

    [Fact]
    public void WithIpAddress_WithEmptyIpAddress_ShouldNotBuildSpecification()
    {
        // Arrange
        SessionQueryBuilder builder = new();

        // Act
        Specification<SessionEntity>? specification = builder.WithIpAddress(string.Empty).Build();

        // Assert
        specification.Should().BeNull("empty IP address should be ignored");
    }

    [Fact]
    public void WithIpAddress_WithWhitespaceIpAddress_ShouldNotBuildSpecification()
    {
        // Arrange
        SessionQueryBuilder builder = new();

        // Act
        Specification<SessionEntity>? specification = builder.WithIpAddress("   ").Build();

        // Assert
        specification.Should().BeNull("whitespace IP address should be ignored");
    }

    #endregion

    #region WithFromDate Tests

    [Fact]
    public void WithFromDate_WithValidDate_ShouldBuildSpecification()
    {
        // Arrange
        SessionQueryBuilder builder = new();

        // Act
        Specification<SessionEntity>? specification = builder.WithFromDate(TestDate).Build();

        // Assert
        specification.Should().NotBeNull("from date filter was provided");
    }

    [Fact]
    public void WithFromDate_WithNullDate_ShouldNotBuildSpecification()
    {
        // Arrange
        SessionQueryBuilder builder = new();

        // Act
        Specification<SessionEntity>? specification = builder.WithFromDate(null).Build();

        // Assert
        specification.Should().BeNull("null from date should be ignored");
    }

    #endregion

    #region WithToDate Tests

    [Fact]
    public void WithToDate_WithValidDate_ShouldBuildSpecification()
    {
        // Arrange
        SessionQueryBuilder builder = new();

        // Act
        Specification<SessionEntity>? specification = builder.WithToDate(TestDate).Build();

        // Assert
        specification.Should().NotBeNull("to date filter was provided");
    }

    [Fact]
    public void WithToDate_WithNullDate_ShouldNotBuildSpecification()
    {
        // Arrange
        SessionQueryBuilder builder = new();

        // Act
        Specification<SessionEntity>? specification = builder.WithToDate(null).Build();

        // Assert
        specification.Should().BeNull("null to date should be ignored");
    }

    #endregion

    #region WithActiveStatus Tests

    [Fact]
    public void WithActiveStatus_WithTrue_ShouldBuildSpecification()
    {
        // Arrange
        SessionQueryBuilder builder = new();

        // Act
        Specification<SessionEntity>? specification = builder.WithActiveStatus(true).Build();

        // Assert
        specification.Should().NotBeNull("active status filter was provided");
    }

    [Fact]
    public void WithActiveStatus_WithFalse_ShouldBuildSpecification()
    {
        // Arrange
        SessionQueryBuilder builder = new();

        // Act
        Specification<SessionEntity>? specification = builder.WithActiveStatus(false).Build();

        // Assert
        specification.Should().NotBeNull("inactive status filter was provided");
    }

    [Fact]
    public void WithActiveStatus_WithNull_ShouldNotBuildSpecification()
    {
        // Arrange
        SessionQueryBuilder builder = new();

        // Act
        Specification<SessionEntity>? specification = builder.WithActiveStatus(null).Build();

        // Assert
        specification.Should().BeNull("null status should be ignored");
    }

    #endregion

    #region Specification Chaining Tests

    [Fact]
    public void Build_WithAllFilters_ShouldCombineSpecifications()
    {
        // Arrange
        SessionQueryBuilder builder = new();
        DateTime fromDate = DateTime.UtcNow.AddDays(-30);
        DateTime toDate = DateTime.UtcNow;

        // Act
        Specification<SessionEntity>? specification = builder
            .WithUserId(TestUserId)
            .WithStatus("Active")
            .WithIpAddress("192.168.1.1")
            .WithFromDate(fromDate)
            .WithToDate(toDate)
            .WithActiveStatus(true)
            .Build();

        // Assert
        specification.Should().NotBeNull("multiple filters were added");
    }

    [Fact]
    public void Build_WithMixedFilters_ShouldOnlyIncludeValidOnes()
    {
        // Arrange
        SessionQueryBuilder builder = new();

        // Act
        Specification<SessionEntity>? specification = builder
            .WithUserId(TestUserId)
            .WithStatus(null) // This should be ignored
            .WithIpAddress("192.168.1.1")
            .WithFromDate(null) // This should be ignored
            .WithActiveStatus(true)
            .Build();

        // Assert
        specification.Should().NotBeNull("valid filters were provided even though some were null");
    }

    [Fact]
    public void Build_WithAllNullOrEmptyFilters_ShouldReturnNull()
    {
        // Arrange
        SessionQueryBuilder builder = new();

        // Act
        Specification<SessionEntity>? specification = builder
            .WithStatus(null)
            .WithIpAddress(null)
            .WithFromDate(null)
            .WithToDate(null)
            .WithActiveStatus(null)
            .Build();

        // Assert
        specification.Should().BeNull("all filters were null or empty");
    }

    [Fact]
    public void Build_CalledMultipleTimes_ShouldReturnSameSpecification()
    {
        // Arrange
        SessionQueryBuilder builder = new();
        builder.WithUserId(TestUserId);

        // Act
        Specification<SessionEntity>? first = builder.Build();
        Specification<SessionEntity>? second = builder.Build();

        // Assert
        first.Should().Be(second, "builder state should not change between Build() calls");
    }

    #endregion

    #region Date Range Tests

    [Fact]
    public void Build_WithBothDates_ShouldCombineBothDateSpecifications()
    {
        // Arrange
        SessionQueryBuilder builder = new();
        DateTime fromDate = DateTime.UtcNow.AddDays(-30);
        DateTime toDate = DateTime.UtcNow;

        // Act
        Specification<SessionEntity>? specification = builder.WithFromDate(fromDate).WithToDate(toDate).Build();

        // Assert
        specification.Should().NotBeNull("both date filters were provided");
    }

    [Fact]
    public void Build_WithOnlyFromDate_ShouldBuildSpecification()
    {
        // Arrange
        SessionQueryBuilder builder = new();
        DateTime fromDate = DateTime.UtcNow.AddDays(-30);

        // Act
        Specification<SessionEntity>? specification = builder.WithFromDate(fromDate).WithToDate(null).Build();

        // Assert
        specification.Should().NotBeNull("from date filter was provided");
    }

    [Fact]
    public void Build_WithOnlyToDate_ShouldBuildSpecification()
    {
        // Arrange
        SessionQueryBuilder builder = new();
        DateTime toDate = DateTime.UtcNow;

        // Act
        Specification<SessionEntity>? specification = builder.WithFromDate(null).WithToDate(toDate).Build();

        // Assert
        specification.Should().NotBeNull("to date filter was provided");
    }

    #endregion

    #region Builder Reusability Tests

    [Fact]
    public void Builder_CanBeReused_WithDifferentFilters()
    {
        // Arrange
        SessionQueryBuilder builder = new();

        // Act - First usage
        Specification<SessionEntity>? firstSpec = builder.WithUserId(TestUserId).Build();

        // Act - Second usage (should chain on top of first)
        Specification<SessionEntity>? secondSpec = builder.WithActiveStatus(true).Build();

        // Assert
        firstSpec.Should().NotBeNull();
        secondSpec.Should().NotBeNull();
        secondSpec.Should().NotBe(firstSpec, "second usage added another filter");
    }

    #endregion

    #region Fluent Interface Chaining Tests

    [Fact]
    public void FluentChaining_ShouldWorkCorrectly()
    {
        // Arrange & Act
        Specification<SessionEntity>? specification = new SessionQueryBuilder()
            .WithUserId(TestUserId)
            .WithStatus("Active")
            .WithIpAddress("192.168.1.1")
            .WithActiveStatus(true)
            .Build();

        // Assert
        specification.Should().NotBeNull("all methods in the fluent chain were called");
    }

    [Fact]
    public void FluentChaining_WithPartialNulls_ShouldWorkCorrectly()
    {
        // Arrange & Act
        Specification<SessionEntity>? specification = new SessionQueryBuilder()
            .WithStatus(null)
            .WithUserId(TestUserId)
            .WithIpAddress(null)
            .WithActiveStatus(true)
            .WithFromDate(null)
            .Build();

        // Assert
        specification.Should().NotBeNull("valid filters were included in the chain");
    }

    #endregion

    #region Edge Cases

    [Fact]
    public void WithUserId_CalledMultipleTimes_ShouldCombineSpecifications()
    {
        // Arrange
        SessionQueryBuilder builder = new();
        Guid userId1 = Guid.NewGuid();
        Guid userId2 = Guid.NewGuid();

        // Act
        Specification<SessionEntity>? specification = builder.WithUserId(userId1).WithUserId(userId2).Build();

        // Assert
        specification.Should().NotBeNull("multiple user ID filters create AND combination");
    }

    [Fact]
    public void WithActiveStatus_CalledMultipleTimes_ShouldCombineSpecifications()
    {
        // Arrange
        SessionQueryBuilder builder = new();

        // Act - Both filters will be combined with AND
        Specification<SessionEntity>? specification = builder.WithActiveStatus(true).WithActiveStatus(false).Build();

        // Assert
        specification.Should().NotBeNull("multiple status filters create AND combination");
    }

    #endregion
}
