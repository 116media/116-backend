using _116.Identity.Application.Auth.Specifications;
using _116.Identity.Domain.Entities;
using _116.Unit.Tests.Common.Constants;
using _116.Unit.Tests.Common.Factories;
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
        UserEntity user = UserFactory.Create(email);
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
        UserEntity user = UserFactory.Create("test@example.com");
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
        UserEntity user = UserFactory.Create(TestConstants.User.ValidEmail, userName);
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
        UserEntity user = UserFactory.Create(TestConstants.User.ValidEmail, "testuser");
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
        UserEntity user = UserFactory.Create();
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
        UserEntity user = UserFactory.Create();
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
        UserEntity user = UserFactory.Create();
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
        UserEntity user = UserFactory.CreateWithId(userId);
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
        UserEntity user = UserFactory.CreateWithId(Guid.NewGuid());
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
        UserEntity user = UserFactory.Create(email);
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
        UserEntity user = UserFactory.Create(TestConstants.User.ValidEmail, userName);
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
        UserEntity user = UserFactory.Create("test@example.com");
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
        UserEntity user = UserFactory.Create(TestConstants.User.ValidEmail, "testuser");
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
            UserFactory.Create(email),
            UserFactory.Create("other@example.com"),
            UserFactory.Create("third@example.com"),
        ];

        UserByEmailSpecification spec = new(email);

        // Act
        List<UserEntity> filtered = users.Where(spec.ToExpression().Compile()).ToList();

        // Assert
        filtered.Should().ContainSingle();
        filtered[0].Email.Should().Be(email);
    }

    [Fact]
    public void UserByUserNameSpecification_WithLinq_ShouldFilterCorrectly()
    {
        // Arrange
        string userName = "testuser";
        List<UserEntity> users =
        [
            UserFactory.Create(TestConstants.User.ValidEmail, userName),
            UserFactory.Create("second@example.com", userName),
            UserFactory.Create("third@example.com", "otheruser"),
        ];

        UserByUserNameSpecification spec = new(userName);

        // Act
        List<UserEntity> filtered = users.Where(spec.ToExpression().Compile()).ToList();

        // Assert
        filtered.Should().HaveCount(2);
        filtered.Should().OnlyContain(u => u.UserName == userName);
    }

    [Fact]
    public void UserByCredentialsSpecification_WithEmail_WithLinq_ShouldFilterCorrectly()
    {
        // Arrange
        string email = "test@example.com";
        List<UserEntity> users = [UserFactory.Create(email), UserFactory.Create("other@example.com")];

        UserByCredentialsSpecification spec = new(email);

        // Act
        List<UserEntity> filtered = users.Where(spec.ToExpression().Compile()).ToList();

        // Assert
        filtered.Should().ContainSingle();
        filtered[0].Email.Should().Be(email);
    }

    [Fact]
    public void UserByCredentialsSpecification_WithUserName_WithLinq_ShouldFilterCorrectly()
    {
        // Arrange
        string userName = "testuser";
        List<UserEntity> users =
        [
            UserFactory.Create(TestConstants.User.ValidEmail, userName),
            UserFactory.Create("other@example.com", "otheruser"),
        ];

        UserByCredentialsSpecification spec = new(userName);

        // Act
        List<UserEntity> filtered = users.Where(spec.ToExpression().Compile()).ToList();

        // Assert
        filtered.Should().ContainSingle();
        filtered[0].UserName.Should().Be(userName);
    }

    #endregion
}
