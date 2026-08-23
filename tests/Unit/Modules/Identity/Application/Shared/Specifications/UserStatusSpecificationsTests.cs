using _116.Identity.Application.Shared.Specifications;
using _116.Identity.Domain.Entities;
using _116.Tests.Fixtures.Builders.Entities.Identity;
using AwesomeAssertions;
using Xunit;

namespace _116.Unit.Tests.Modules.Identity.Application.Shared.Specifications;

/// <summary>
/// Unit tests for UserStatus specifications.
/// </summary>
public class UserStatusSpecificationsTests
{
    #region UserIsActiveSpecification Tests

    [Fact]
    public void UserIsActiveSpecification_WithActiveUser_ShouldReturnTrue()
    {
        // Arrange
        UserEntity user = new UserBuilder().AsActive().Build();
        UserIsActiveSpecification spec = new();

        // Act
        bool result = spec.IsSatisfiedBy(user);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void UserIsActiveSpecification_WithInactiveUser_ShouldReturnFalse()
    {
        // Arrange
        UserEntity user = new UserBuilder().AsInactive().Build();
        UserIsActiveSpecification spec = new();

        // Act
        bool result = spec.IsSatisfiedBy(user);

        // Assert
        result.Should().BeFalse();
    }

    #endregion

    #region UserIsVerifiedSpecification Tests

    [Fact]
    public void UserIsVerifiedSpecification_WithVerifiedUser_ShouldReturnTrue()
    {
        // Arrange
        UserEntity user = new UserBuilder().AsVerified().Build();
        UserIsVerifiedSpecification spec = new();

        // Act
        bool result = spec.IsSatisfiedBy(user);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void UserIsVerifiedSpecification_WithUnverifiedUser_ShouldReturnFalse()
    {
        // Arrange
        UserEntity user = new UserBuilder().AsUnverified().Build();
        UserIsVerifiedSpecification spec = new();

        // Act
        bool result = spec.IsSatisfiedBy(user);

        // Assert
        result.Should().BeFalse();
    }

    #endregion

    #region UserIsActiveAndVerifiedSpecification Tests

    [Fact]
    public void UserIsActiveAndVerifiedSpecification_WithActiveAndVerifiedUser_ShouldReturnTrue()
    {
        // Arrange
        UserEntity user = new UserBuilder().AsActive().AsVerified().Build();
        UserIsActiveAndVerifiedSpecification spec = new();

        // Act
        bool result = spec.IsSatisfiedBy(user);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void UserIsActiveAndVerifiedSpecification_WithInactiveUser_ShouldReturnFalse()
    {
        // Arrange
        UserEntity user = new UserBuilder().AsInactive().AsVerified().Build();
        UserIsActiveAndVerifiedSpecification spec = new();

        // Act
        bool result = spec.IsSatisfiedBy(user);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void UserIsActiveAndVerifiedSpecification_WithUnverifiedUser_ShouldReturnFalse()
    {
        // Arrange
        UserEntity user = new UserBuilder().AsActive().AsUnverified().Build();
        UserIsActiveAndVerifiedSpecification spec = new();

        // Act
        bool result = spec.IsSatisfiedBy(user);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void UserIsActiveAndVerifiedSpecification_WithInactiveAndUnverifiedUser_ShouldReturnFalse()
    {
        // Arrange
        UserEntity user = new UserBuilder().AsInactive().AsUnverified().Build();
        UserIsActiveAndVerifiedSpecification spec = new();

        // Act
        bool result = spec.IsSatisfiedBy(user);

        // Assert
        result.Should().BeFalse();
    }

    #endregion

    #region LINQ Integration Tests

    [Fact]
    public void UserIsActiveSpecification_WithLinq_ShouldFilterCorrectly()
    {
        // Arrange
        UserEntity activeUser1 = new UserBuilder().AsActive().Build();
        UserEntity activeUser2 = new UserBuilder().AsActive().Build();
        UserEntity inactiveUser = new UserBuilder().AsInactive().Build();

        List<UserEntity> users = [activeUser1, activeUser2, inactiveUser];
        UserIsActiveSpecification spec = new();

        // Act
        List<UserEntity> filtered = users.Where(spec.ToExpression().Compile()).ToList();

        // Assert
        filtered.Should().HaveCount(2);
        filtered.Should().OnlyContain(u => u.IsActive);
    }

    [Fact]
    public void UserIsVerifiedSpecification_WithLinq_ShouldFilterCorrectly()
    {
        // Arrange
        UserEntity verifiedUser1 = new UserBuilder().AsVerified().Build();
        UserEntity verifiedUser2 = new UserBuilder().AsVerified().Build();
        UserEntity unverifiedUser = new UserBuilder().AsUnverified().Build();

        List<UserEntity> users = [verifiedUser1, verifiedUser2, unverifiedUser];
        UserIsVerifiedSpecification spec = new();

        // Act
        List<UserEntity> filtered = users.Where(spec.ToExpression().Compile()).ToList();

        // Assert
        filtered.Should().HaveCount(2);
        filtered.Should().OnlyContain(u => u.IsVerified);
    }

    [Fact]
    public void UserIsActiveAndVerifiedSpecification_WithLinq_ShouldFilterCorrectly()
    {
        // Arrange
        UserEntity activeAndVerified = new UserBuilder().AsActive().AsVerified().Build();
        UserEntity activeButUnverified = new UserBuilder().AsActive().AsUnverified().Build();
        UserEntity inactiveButVerified = new UserBuilder().AsInactive().AsVerified().Build();
        UserEntity inactiveAndUnverified = new UserBuilder().AsInactive().AsUnverified().Build();

        List<UserEntity> users = [activeAndVerified, activeButUnverified, inactiveButVerified, inactiveAndUnverified];
        UserIsActiveAndVerifiedSpecification spec = new();

        // Act
        List<UserEntity> filtered = users.Where(spec.ToExpression().Compile()).ToList();

        // Assert
        filtered.Should().ContainSingle();
        filtered[0].IsActive.Should().BeTrue();
        filtered[0].IsVerified.Should().BeTrue();
    }

    #endregion
}
