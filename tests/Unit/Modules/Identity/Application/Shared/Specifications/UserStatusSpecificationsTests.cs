using _116.Identity.Application.Shared.Specifications;
using _116.Identity.Domain.Entities;
using _116.Unit.Tests.Common.Factories;
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
        UserEntity user = UserFactory.Create();
        user.GetType().GetProperty("IsActive")!.SetValue(user, true);
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
        UserEntity user = UserFactory.Create();
        user.GetType().GetProperty("IsActive")!.SetValue(user, false);
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
        UserEntity user = UserFactory.Create();
        user.GetType().GetProperty("IsVerified")!.SetValue(user, true);
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
        UserEntity user = UserFactory.Create();
        user.GetType().GetProperty("IsVerified")!.SetValue(user, false);
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
        UserEntity user = UserFactory.Create();
        user.GetType().GetProperty("IsActive")!.SetValue(user, true);
        user.GetType().GetProperty("IsVerified")!.SetValue(user, true);
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
        UserEntity user = UserFactory.Create();
        user.GetType().GetProperty("IsActive")!.SetValue(user, false);
        user.GetType().GetProperty("IsVerified")!.SetValue(user, true);
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
        UserEntity user = UserFactory.Create();
        user.GetType().GetProperty("IsActive")!.SetValue(user, true);
        user.GetType().GetProperty("IsVerified")!.SetValue(user, false);
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
        UserEntity user = UserFactory.Create();
        user.GetType().GetProperty("IsActive")!.SetValue(user, false);
        user.GetType().GetProperty("IsVerified")!.SetValue(user, false);
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
        UserEntity activeUser1 = UserFactory.Create();
        activeUser1.GetType().GetProperty("IsActive")!.SetValue(activeUser1, true);

        UserEntity activeUser2 = UserFactory.Create();
        activeUser2.GetType().GetProperty("IsActive")!.SetValue(activeUser2, true);

        UserEntity inactiveUser = UserFactory.Create();
        inactiveUser.GetType().GetProperty("IsActive")!.SetValue(inactiveUser, false);

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
        UserEntity verifiedUser1 = UserFactory.Create();
        verifiedUser1.GetType().GetProperty("IsVerified")!.SetValue(verifiedUser1, true);

        UserEntity verifiedUser2 = UserFactory.Create();
        verifiedUser2.GetType().GetProperty("IsVerified")!.SetValue(verifiedUser2, true);

        UserEntity unverifiedUser = UserFactory.Create();
        unverifiedUser.GetType().GetProperty("IsVerified")!.SetValue(unverifiedUser, false);

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
        UserEntity activeAndVerified = UserFactory.Create();
        activeAndVerified.GetType().GetProperty("IsActive")!.SetValue(activeAndVerified, true);
        activeAndVerified.GetType().GetProperty("IsVerified")!.SetValue(activeAndVerified, true);

        UserEntity activeButUnverified = UserFactory.Create();
        activeButUnverified.GetType().GetProperty("IsActive")!.SetValue(activeButUnverified, true);
        activeButUnverified.GetType().GetProperty("IsVerified")!.SetValue(activeButUnverified, false);

        UserEntity inactiveButVerified = UserFactory.Create();
        inactiveButVerified.GetType().GetProperty("IsActive")!.SetValue(inactiveButVerified, false);
        inactiveButVerified.GetType().GetProperty("IsVerified")!.SetValue(inactiveButVerified, true);

        UserEntity inactiveAndUnverified = UserFactory.Create();
        inactiveAndUnverified.GetType().GetProperty("IsActive")!.SetValue(inactiveAndUnverified, false);
        inactiveAndUnverified.GetType().GetProperty("IsVerified")!.SetValue(inactiveAndUnverified, false);

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
