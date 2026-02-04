using _116.Identity.Application.Auth.Specifications;
using _116.Identity.Domain.Entities;
using _116.Unit.Tests.Common.Builders.Entities;
using AwesomeAssertions;
using Xunit;

namespace _116.Unit.Tests.Modules.Identity.Application.Auth.Specifications;

/// <summary>
/// Unit tests for UserCredential specifications.
/// </summary>
public class UserCredentialSpecificationsTests
{
    #region UserByEmailSpecification Tests

    [Fact]
    public void UserByEmailSpecification_WithMatchingEmail_ShouldReturnTrue()
    {
        // Arrange
        string email = "test@example.com";
        UserEntity user = new UserBuilder().WithEmail(email).Build();
        UserByEmailSpecification spec = new(email);

        // Act
        bool result = spec.IsSatisfiedBy(user);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void UserByEmailSpecification_WithDifferentEmail_ShouldReturnFalse()
    {
        // Arrange
        UserEntity user = new UserBuilder().WithEmail("test@example.com").Build();
        UserByEmailSpecification spec = new("other@example.com");

        // Act
        bool result = spec.IsSatisfiedBy(user);

        // Assert
        result.Should().BeFalse();
    }

    #endregion

    #region UserByUserNameSpecification Tests

    [Fact]
    public void UserByUserNameSpecification_WithMatchingUserName_ShouldReturnTrue()
    {
        // Arrange
        string userName = "testuser";
        UserEntity user = new UserBuilder().WithUserName(userName).Build();
        UserByUserNameSpecification spec = new(userName);

        // Act
        bool result = spec.IsSatisfiedBy(user);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void UserByUserNameSpecification_WithDifferentUserName_ShouldReturnFalse()
    {
        // Arrange
        UserEntity user = new UserBuilder().WithUserName("testuser").Build();
        UserByUserNameSpecification spec = new("otheruser");

        // Act
        bool result = spec.IsSatisfiedBy(user);

        // Assert
        result.Should().BeFalse();
    }

    #endregion

    #region UserByPhoneNumberSpecification Tests

    [Fact]
    public void UserByPhoneNumberSpecification_WithMatchingPhoneNumber_ShouldReturnTrue()
    {
        // Arrange
        string phoneNumber = "+1234567890";
        UserEntity user = new UserBuilder().Build();
        user.GetType().GetProperty("FullPhoneNumber")!.SetValue(user, phoneNumber);
        UserByPhoneNumberSpecification spec = new(phoneNumber);

        // Act
        bool result = spec.IsSatisfiedBy(user);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void UserByPhoneNumberSpecification_WithDifferentPhoneNumber_ShouldReturnFalse()
    {
        // Arrange
        UserEntity user = new UserBuilder().Build();
        user.GetType().GetProperty("FullPhoneNumber")!.SetValue(user, "+1234567890");
        UserByPhoneNumberSpecification spec = new("+9876543210");

        // Act
        bool result = spec.IsSatisfiedBy(user);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void UserByPhoneNumberSpecification_WithNullPhoneNumber_ShouldReturnFalse()
    {
        // Arrange
        UserEntity user = new UserBuilder().Build();
        user.GetType().GetProperty("FullPhoneNumber")!.SetValue(user, null);
        UserByPhoneNumberSpecification spec = new("+1234567890");

        // Act
        bool result = spec.IsSatisfiedBy(user);

        // Assert
        result.Should().BeFalse();
    }

    #endregion

    #region UserByIdSpecification Tests

    [Fact]
    public void UserByIdSpecification_WithMatchingId_ShouldReturnTrue()
    {
        // Arrange
        Guid userId = Guid.NewGuid();
        UserEntity user = new UserBuilder().WithId(userId).Build();
        UserByIdSpecification spec = new(userId);

        // Act
        bool result = spec.IsSatisfiedBy(user);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void UserByIdSpecification_WithDifferentId_ShouldReturnFalse()
    {
        // Arrange
        UserEntity user = new UserBuilder().WithId(Guid.NewGuid()).Build();
        UserByIdSpecification spec = new(Guid.NewGuid());

        // Act
        bool result = spec.IsSatisfiedBy(user);

        // Assert
        result.Should().BeFalse();
    }

    #endregion

    #region UserByCredentialsSpecification Tests

    [Fact]
    public void UserByCredentialsSpecification_WithEmail_ShouldMatchByEmail()
    {
        // Arrange
        string email = "test@example.com";
        UserEntity user = new UserBuilder().WithEmail(email).Build();
        UserByCredentialsSpecification spec = new(email);

        // Act
        bool result = spec.IsSatisfiedBy(user);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void UserByCredentialsSpecification_WithUserName_ShouldMatchByUserName()
    {
        // Arrange
        string userName = "testuser";
        UserEntity user = new UserBuilder().WithUserName(userName).Build();
        UserByCredentialsSpecification spec = new(userName);

        // Act
        bool result = spec.IsSatisfiedBy(user);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void UserByCredentialsSpecification_WithWrongEmail_ShouldReturnFalse()
    {
        // Arrange
        UserEntity user = new UserBuilder().WithEmail("test@example.com").Build();
        UserByCredentialsSpecification spec = new("other@example.com");

        // Act
        bool result = spec.IsSatisfiedBy(user);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void UserByCredentialsSpecification_WithWrongUserName_ShouldReturnFalse()
    {
        // Arrange
        UserEntity user = new UserBuilder().WithUserName("testuser").Build();
        UserByCredentialsSpecification spec = new("otheruser");

        // Act
        bool result = spec.IsSatisfiedBy(user);

        // Assert
        result.Should().BeFalse();
    }

    #endregion

    #region LINQ Integration Tests

    [Fact]
    public void UserByEmailSpecification_WithLinq_ShouldFilterCorrectly()
    {
        // Arrange
        string email = "test@example.com";
        List<UserEntity> users =
        [
            new UserBuilder().WithEmail(email).Build(),
            new UserBuilder().WithEmail("other@example.com").Build(),
            new UserBuilder().WithEmail("third@example.com").Build(),
        ];

        UserByEmailSpecification spec = new(email);

        // Act
        List<UserEntity> filtered = users.Where(spec.ToExpression().Compile()).ToList();

        // Assert
        filtered.Should().HaveCount(1);
        filtered[0].Email.Should().Be(email);
    }

    [Fact]
    public void UserByUserNameSpecification_WithLinq_ShouldFilterCorrectly()
    {
        // Arrange
        string userName = "testuser";
        List<UserEntity> users =
        [
            new UserBuilder().WithUserName(userName).Build(),
            new UserBuilder().WithUserName(userName).Build(),
            new UserBuilder().WithUserName("otheruser").Build(),
        ];

        UserByUserNameSpecification spec = new(userName);

        // Act
        List<UserEntity> filtered = users.Where(spec.ToExpression().Compile()).ToList();

        // Assert
        filtered.Should().HaveCount(2);
        filtered.All(u => u.UserName == userName).Should().BeTrue();
    }

    [Fact]
    public void UserByCredentialsSpecification_WithEmail_WithLinq_ShouldFilterCorrectly()
    {
        // Arrange
        string email = "test@example.com";
        List<UserEntity> users =
        [
            new UserBuilder().WithEmail(email).Build(),
            new UserBuilder().WithEmail("other@example.com").Build(),
        ];

        UserByCredentialsSpecification spec = new(email);

        // Act
        List<UserEntity> filtered = users.Where(spec.ToExpression().Compile()).ToList();

        // Assert
        filtered.Should().HaveCount(1);
        filtered[0].Email.Should().Be(email);
    }

    [Fact]
    public void UserByCredentialsSpecification_WithUserName_WithLinq_ShouldFilterCorrectly()
    {
        // Arrange
        string userName = "testuser";
        List<UserEntity> users =
        [
            new UserBuilder().WithUserName(userName).Build(),
            new UserBuilder().WithUserName("otheruser").Build(),
        ];

        UserByCredentialsSpecification spec = new(userName);

        // Act
        List<UserEntity> filtered = users.Where(spec.ToExpression().Compile()).ToList();

        // Assert
        filtered.Should().HaveCount(1);
        filtered[0].UserName.Should().Be(userName);
    }

    #endregion
}
