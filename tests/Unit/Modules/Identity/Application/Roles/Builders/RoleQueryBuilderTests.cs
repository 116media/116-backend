using _116.Identity.Application.Roles.Builders;
using _116.Identity.Application.Roles.Builders.Contracts;
using _116.Identity.Domain.Entities;
using _116.Shared.Application.Specifications;
using _116.Tests.Fixtures.Factories.Identity;
using _116.Unit.Tests.Common.Helpers;
using AwesomeAssertions;
using Xunit;

namespace _116.Unit.Tests.Modules.Identity.Application.Roles.Builders;

/// <summary>
/// Unit tests for <see cref="RoleQueryBuilder"/>.
/// </summary>
public class RoleQueryBuilderTests
{
    #region Builder Pattern Tests

    [Fact]
    public void WithSearch_ShouldReturnBuilder()
    {
        // Arrange
        RoleQueryBuilder builder = new();

        // Act
        IRoleQueryBuilder result = builder.WithSearch("admin");

        // Assert
        result.Should().Be(builder, "fluent interface should return the same builder instance");
    }

    [Fact]
    public void WithActiveStatus_ShouldReturnBuilder()
    {
        // Arrange
        RoleQueryBuilder builder = new();

        // Act
        IRoleQueryBuilder result = builder.WithActiveStatus(true);

        // Assert
        result.Should().Be(builder, "fluent interface should return the same builder instance");
    }

    [Fact]
    public void WithDeletedStatus_ShouldReturnBuilder()
    {
        // Arrange
        RoleQueryBuilder builder = new();

        // Act
        IRoleQueryBuilder result = builder.WithDeletedStatus(false);

        // Assert
        result.Should().Be(builder, "fluent interface should return the same builder instance");
    }

    #endregion

    #region Build - Empty Builder Tests

    [Fact]
    public void Build_WithNoFilters_ShouldReturnNull()
    {
        // Arrange
        RoleQueryBuilder builder = new();

        // Act
        Specification<RoleEntity>? specification = builder.Build();

        // Assert
        specification.Should().BeNull("no filters were added to the builder");
    }

    #endregion

    #region WithSearch Tests

    [Fact]
    public void WithSearch_WithValidSearchTerm_ShouldMatchNameOrDescriptionCaseInsensitively()
    {
        // Arrange
        RoleQueryBuilder builder = new();
        RoleEntity matchByName = RoleFactory.Create("ContentAdmin", "Manages editorial workflows");
        RoleEntity matchByDescription = RoleFactory.Create("Editor", "Administers published content");
        RoleEntity noMatch = RoleFactory.Create("Visitor", "Read-only access");

        // Act
        Specification<RoleEntity>? specification = builder.WithSearch("admin").Build();

        // Assert
        specification.Should().NotBeNull("search term was provided");
        specification!.IsSatisfiedInMemoryBy(matchByName).Should().BeTrue();
        specification.IsSatisfiedInMemoryBy(matchByDescription).Should().BeTrue();
        specification.IsSatisfiedInMemoryBy(noMatch).Should().BeFalse();
    }

    [Fact]
    public void WithSearch_WithNullSearchTerm_ShouldNotBuildSpecification()
    {
        // Arrange
        RoleQueryBuilder builder = new();

        // Act
        Specification<RoleEntity>? specification = builder.WithSearch(null).Build();

        // Assert
        specification.Should().BeNull("null search term should be ignored");
    }

    [Fact]
    public void WithSearch_WithEmptySearchTerm_ShouldNotBuildSpecification()
    {
        // Arrange
        RoleQueryBuilder builder = new();

        // Act
        Specification<RoleEntity>? specification = builder.WithSearch(string.Empty).Build();

        // Assert
        specification.Should().BeNull("empty search term should be ignored");
    }

    [Fact]
    public void WithSearch_WithWhitespaceSearchTerm_ShouldNotBuildSpecification()
    {
        // Arrange
        RoleQueryBuilder builder = new();

        // Act
        Specification<RoleEntity>? specification = builder.WithSearch("   ").Build();

        // Assert
        specification.Should().BeNull("whitespace search term should be ignored");
    }

    #endregion

    #region WithActiveStatus Tests

    [Fact]
    public void WithActiveStatus_WithTrue_ShouldMatchOnlyActiveRoles()
    {
        // Arrange
        RoleQueryBuilder builder = new();
        RoleEntity activeRole = RoleFactory.Create();
        RoleEntity inactiveRole = RoleFactory.CreateInactive();

        // Act
        Specification<RoleEntity>? specification = builder.WithActiveStatus(true).Build();

        // Assert
        specification.Should().NotBeNull("active status filter was provided");
        specification!.IsSatisfiedBy(activeRole).Should().BeTrue();
        specification.IsSatisfiedBy(inactiveRole).Should().BeFalse();
    }

    [Fact]
    public void WithActiveStatus_WithFalse_ShouldMatchOnlyInactiveRoles()
    {
        // Arrange
        RoleQueryBuilder builder = new();
        RoleEntity inactiveRole = RoleFactory.CreateInactive();
        RoleEntity activeRole = RoleFactory.Create();

        // Act
        Specification<RoleEntity>? specification = builder.WithActiveStatus(false).Build();

        // Assert
        specification.Should().NotBeNull("inactive status filter was provided");
        specification!.IsSatisfiedBy(inactiveRole).Should().BeTrue();
        specification.IsSatisfiedBy(activeRole).Should().BeFalse();
    }

    [Fact]
    public void WithActiveStatus_WithNull_ShouldNotBuildSpecification()
    {
        // Arrange
        RoleQueryBuilder builder = new();

        // Act
        Specification<RoleEntity>? specification = builder.WithActiveStatus(null).Build();

        // Assert
        specification.Should().BeNull("null status should be ignored");
    }

    #endregion

    #region WithDeletedStatus Tests

    [Fact]
    public void WithDeletedStatus_WithTrue_ShouldMatchOnlyDeletedRoles()
    {
        // Arrange
        RoleQueryBuilder builder = new();
        RoleEntity deletedRole = RoleFactory.CreateDeleted();
        RoleEntity liveRole = RoleFactory.Create();

        // Act
        Specification<RoleEntity>? specification = builder.WithDeletedStatus(true).Build();

        // Assert
        specification.Should().NotBeNull("deleted status filter was provided");
        specification!.IsSatisfiedBy(deletedRole).Should().BeTrue();
        specification.IsSatisfiedBy(liveRole).Should().BeFalse();
    }

    [Fact]
    public void WithDeletedStatus_WithFalse_ShouldMatchOnlyLiveRoles()
    {
        // Arrange
        RoleQueryBuilder builder = new();
        RoleEntity liveRole = RoleFactory.Create();
        RoleEntity deletedRole = RoleFactory.CreateDeleted();

        // Act
        Specification<RoleEntity>? specification = builder.WithDeletedStatus(false).Build();

        // Assert
        specification.Should().NotBeNull("not-deleted status filter was provided");
        specification!.IsSatisfiedBy(liveRole).Should().BeTrue();
        specification.IsSatisfiedBy(deletedRole).Should().BeFalse();
    }

    [Fact]
    public void WithDeletedStatus_WithNull_ShouldNotBuildSpecification()
    {
        // Arrange
        RoleQueryBuilder builder = new();

        // Act
        Specification<RoleEntity>? specification = builder.WithDeletedStatus(null).Build();

        // Assert
        specification.Should().BeNull("null status should be ignored");
    }

    #endregion

    #region Specification Chaining Tests

    [Fact]
    public void Build_WithActiveAndNotDeleted_ShouldMatchOnlyRolesSatisfyingBoth()
    {
        // Arrange
        RoleEntity match = RoleFactory.Create();
        RoleEntity inactive = RoleFactory.CreateInactive();
        RoleEntity deleted = RoleFactory.CreateDeleted();

        // Act
        Specification<RoleEntity>? specification = new RoleQueryBuilder()
            .WithActiveStatus(true)
            .WithDeletedStatus(false)
            .Build();

        // Assert
        specification.Should().NotBeNull();
        specification!.IsSatisfiedBy(match).Should().BeTrue();
        specification.IsSatisfiedBy(inactive).Should().BeFalse();
        specification.IsSatisfiedBy(deleted).Should().BeFalse();
    }

    [Fact]
    public void Build_WithMultipleFilters_ShouldCombineSpecifications()
    {
        // Arrange
        RoleQueryBuilder builder = new();

        // Act
        Specification<RoleEntity>? specification = builder
            .WithSearch("admin")
            .WithActiveStatus(true)
            .WithDeletedStatus(false)
            .Build();

        // Assert
        specification.Should().NotBeNull("multiple filters were added");
    }

    [Fact]
    public void Build_WithMixedFilters_ShouldOnlyIncludeValidOnes()
    {
        // Arrange
        RoleQueryBuilder builder = new();

        // Act
        Specification<RoleEntity>? specification = builder
            .WithSearch("admin")
            .WithActiveStatus(null) // This should be ignored
            .WithDeletedStatus(false)
            .Build();

        // Assert
        specification.Should().NotBeNull("valid filters were provided even though one was null");
    }

    [Fact]
    public void Build_WithAllNullFilters_ShouldReturnNull()
    {
        // Arrange
        RoleQueryBuilder builder = new();

        // Act
        Specification<RoleEntity>? specification = builder
            .WithSearch(null)
            .WithActiveStatus(null)
            .WithDeletedStatus(null)
            .Build();

        // Assert
        specification.Should().BeNull("all filters were null");
    }

    [Fact]
    public void Build_CalledMultipleTimes_ShouldReturnSameSpecification()
    {
        // Arrange
        RoleQueryBuilder builder = new();
        builder.WithSearch("admin");

        // Act
        Specification<RoleEntity>? first = builder.Build();
        Specification<RoleEntity>? second = builder.Build();

        // Assert
        first.Should().Be(second, "builder state should not change between Build() calls");
    }

    #endregion

    #region Builder Reusability Tests

    [Fact]
    public void Builder_CanBeReused_WithDifferentFilters()
    {
        // Arrange
        RoleQueryBuilder builder = new();

        // Act - First usage
        Specification<RoleEntity>? firstSpec = builder.WithSearch("admin").Build();

        // Act - Second usage (should chain on top of first)
        Specification<RoleEntity>? secondSpec = builder.WithActiveStatus(true).Build();

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
        Specification<RoleEntity>? specification = new RoleQueryBuilder()
            .WithSearch("admin")
            .WithActiveStatus(true)
            .WithDeletedStatus(false)
            .Build();

        // Assert
        specification.Should().NotBeNull("all methods in the fluent chain were called");
    }

    [Fact]
    public void FluentChaining_WithPartialNulls_ShouldWorkCorrectly()
    {
        // Arrange & Act
        Specification<RoleEntity>? specification = new RoleQueryBuilder()
            .WithSearch(null)
            .WithActiveStatus(true)
            .WithDeletedStatus(null)
            .WithSearch("admin")
            .Build();

        // Assert
        specification.Should().NotBeNull("valid filters were included in the chain");
    }

    #endregion
}
