using _116.Identity.Domain.Entities;
using _116.Identity.Domain.Enums;
using _116.Shared.Application.Exceptions;
using _116.Tests.Fixtures.Builders.Entities;
using _116.Tests.Fixtures.Constants;
using _116.Tests.Fixtures.Factories;
using AwesomeAssertions;
using Xunit;

namespace _116.Unit.Tests.Modules.Identity.Domain.Entities;

/// <summary>
/// Unit tests for <see cref="UserEntity"/>.
/// </summary>
public class UserEntityTests
{
    #region Create Tests

    [Fact]
    public void Create_WithValidParameters_ShouldCreateUser()
    {
        // Arrange
        Guid id = Guid.NewGuid();
        string email = TestConstants.User.ValidEmail;
        string userName = TestConstants.User.ValidUserName;
        string passwordHash = TestConstants.User.DefaultPasswordHash;

        // Act
        UserEntity user = UserEntity.Create(id, email, userName, passwordHash);

        // Assert
        user.Id.Should().Be(id);
        user.Email.Should().Be(email.ToLowerInvariant());
        user.UserName.Should().Be(userName);
        user.PasswordHash.Should().Be(passwordHash);
        user.AuthProvider.Should().Be(EnumAuthProvider.Local);
        user.IsVerified.Should().BeFalse();
        user.IsActive.Should().BeTrue(); // Default is true per UserConstants.DefaultIsActive
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WithInvalidEmail_ShouldThrowException(string? invalidEmail)
    {
        // Arrange
        Guid id = Guid.NewGuid();

        // Act
        Action act = () =>
            UserEntity.Create(
                id,
                invalidEmail!,
                TestConstants.User.ValidUserName,
                TestConstants.User.DefaultPasswordHash
            );

        // Assert
        act.Should().Throw<Exception>();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WithInvalidUserName_ShouldThrowException(string? invalidUserName)
    {
        // Arrange
        Guid id = Guid.NewGuid();

        // Act
        Action act = () =>
            UserEntity.Create(
                id,
                TestConstants.User.ValidEmail,
                invalidUserName!,
                TestConstants.User.DefaultPasswordHash
            );

        // Assert
        act.Should().Throw<Exception>();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WithInvalidPasswordHash_ShouldThrowException(string? invalidPasswordHash)
    {
        // Arrange
        Guid id = Guid.NewGuid();

        // Act
        Action act = () =>
            UserEntity.Create(
                id,
                TestConstants.User.ValidEmail,
                TestConstants.User.ValidUserName,
                invalidPasswordHash!
            );

        // Assert
        act.Should().Throw<Exception>();
    }

    #endregion

    #region CreateExternal Tests

    [Fact]
    public void CreateExternal_WithValidParameters_ShouldCreateExternalUser()
    {
        // Arrange
        Guid id = Guid.NewGuid();
        string userName = TestConstants.User.ValidUserName;
        string email = TestConstants.User.ValidEmail;

        // Act
        UserEntity user = UserEntity.CreateExternal(id, userName, EnumAuthProvider.Google, email);

        // Assert
        user.Id.Should().Be(id);
        user.Email.Should().Be(email.ToLowerInvariant());
        user.UserName.Should().Be(userName);
        user.PasswordHash.Should().BeNull();
        user.AuthProvider.Should().Be(EnumAuthProvider.Google);
        user.IsVerified.Should().BeTrue(); // External users are auto-verified
    }

    [Fact]
    public void CreateExternal_WithoutEmail_ShouldCreateExternalUser()
    {
        // Arrange
        Guid id = Guid.NewGuid();
        string userName = TestConstants.User.ValidUserName;

        // Act
        UserEntity user = UserEntity.CreateExternal(id, userName, EnumAuthProvider.Facebook);

        // Assert
        user.Id.Should().Be(id);
        user.Email.Should().BeNull();
        user.UserName.Should().Be(userName);
        user.AuthProvider.Should().Be(EnumAuthProvider.Facebook);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void CreateExternal_WithInvalidUserName_ShouldThrowException(string? invalidUserName)
    {
        // Arrange
        Guid id = Guid.NewGuid();

        // Act
        Action act = () => UserEntity.CreateExternal(id, invalidUserName!, EnumAuthProvider.Google);

        // Assert
        act.Should().Throw<Exception>();
    }

    #endregion

    #region UpdateEmail Tests

    [Fact]
    public void UpdateEmail_WithValidEmail_ShouldUpdateEmailAndResetVerification()
    {
        // Arrange
        UserEntity user = UserFactory.CreateExternal(EnumAuthProvider.Local);
        string newEmail = "newemail@example.com";

        // Act
        user.UpdateEmail(newEmail);

        // Assert
        user.Email.Should().Be(newEmail.ToLowerInvariant());
        user.IsVerified.Should().BeFalse(); // Should reset verification
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void UpdateEmail_WithInvalidEmail_ShouldThrowException(string? invalidEmail)
    {
        // Arrange
        UserEntity user = UserFactory.Create();

        // Act
        Action act = () => user.UpdateEmail(invalidEmail!);

        // Assert
        act.Should().Throw<Exception>();
    }

    #endregion

    #region UpdatePassword Tests

    [Fact]
    public void UpdatePassword_WithValidHash_ShouldUpdatePassword()
    {
        // Arrange
        UserEntity user = UserFactory.Create();
        string newPasswordHash = "new_hashed_password_value";

        // Act
        user.UpdatePassword(newPasswordHash);

        // Assert
        user.PasswordHash.Should().Be(newPasswordHash);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void UpdatePassword_WithInvalidHash_ShouldThrowException(string? invalidHash)
    {
        // Arrange
        UserEntity user = UserFactory.Create();

        // Act
        Action act = () => user.UpdatePassword(invalidHash!);

        // Assert
        act.Should().Throw<Exception>();
    }

    [Fact]
    public void UpdatePassword_WhenNonLocalUserWithoutEmail_ShouldThrowException()
    {
        // Arrange
        UserEntity user = UserFactory.CreateExternal(EnumAuthProvider.Google);

        // Remove email by using reflection to set it to null (simulating social login without email)
        typeof(UserEntity).GetProperty(nameof(UserEntity.Email))!.SetValue(user, null);

        // Act
        Action act = () => user.UpdatePassword("new_password_hash");

        // Assert
        act.Should().Throw<BadRequestException>().WithMessage("Cannot update password for a user without email.");
    }

    #endregion

    #region SetPasswordAndChangeToLocal Tests

    [Fact]
    public void SetPasswordAndChangeToLocal_WithValidPassword_ShouldSetPasswordAndChangeProvider()
    {
        // Arrange
        UserEntity user = UserFactory.CreateExternal(EnumAuthProvider.Google);
        string passwordHash = "valid_password_hash";

        // Act
        user.SetPasswordAndChangeToLocal(passwordHash);

        // Assert
        user.PasswordHash.Should().Be(passwordHash);
        user.AuthProvider.Should().Be(EnumAuthProvider.Local);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void SetPasswordAndChangeToLocal_WithInvalidPassword_ShouldThrowException(string? invalidPassword)
    {
        // Arrange
        UserEntity user = UserFactory.CreateExternal(EnumAuthProvider.Google);

        // Act
        Action act = () => user.SetPasswordAndChangeToLocal(invalidPassword!);

        // Assert
        act.Should().Throw<Exception>().WithMessage("*Password does not meet security requirements*");
    }

    [Fact]
    public void SetPasswordAndChangeToLocal_WhenUserHasNoEmail_ShouldThrowException()
    {
        // Arrange
        UserEntity user = UserFactory.CreateExternal(EnumAuthProvider.Facebook);

        // Remove email by using reflection to set it to null (simulating social login without email)
        typeof(UserEntity).GetProperty(nameof(UserEntity.Email))!.SetValue(user, null);

        // Act
        Action act = () => user.SetPasswordAndChangeToLocal("valid_password_hash");

        // Assert
        act.Should().Throw<BadRequestException>().WithMessage("An email address is required to set a password");
    }

    #endregion

    #region UpdateUserName Tests

    [Fact]
    public void UpdateUserName_WithValidUserName_ShouldUpdateUserName()
    {
        // Arrange
        UserEntity user = UserFactory.Create();
        string newUserName = "newusername";

        // Act
        user.UpdateUserName(newUserName);

        // Assert
        user.UserName.Should().Be(newUserName);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void UpdateUserName_WithInvalidUserName_ShouldThrowException(string? invalidUserName)
    {
        // Arrange
        UserEntity user = UserFactory.Create();

        // Act
        Action act = () => user.UpdateUserName(invalidUserName!);

        // Assert
        act.Should().Throw<Exception>();
    }

    #endregion

    #region MarkAsVerified Tests

    [Fact]
    public void MarkAsVerified_ShouldSetIsVerifiedToTrue()
    {
        // Arrange
        UserEntity user = UserFactory.Create();

        // Act
        user.MarkAsVerified();

        // Assert
        user.IsVerified.Should().BeTrue();
    }

    #endregion

    #region Activate/Deactivate Tests

    [Fact]
    public void Activate_ShouldSetIsActiveToTrue()
    {
        // Arrange
        UserEntity user = UserFactory.Create();

        // Act
        user.Activate();

        // Assert
        user.IsActive.Should().BeTrue();
    }

    [Fact]
    public void Deactivate_ShouldSetIsActiveToFalse()
    {
        // Arrange
        UserEntity user = UserFactory.CreateVerifiedActive();

        // Act
        user.Deactivate();

        // Assert
        user.IsActive.Should().BeFalse();
    }

    #endregion

    #region ValidateCanLogin Tests

    [Fact]
    public void ValidateCanLogin_WhenActiveAndVerified_ShouldNotThrow()
    {
        // Arrange
        UserEntity user = UserFactory.CreateVerifiedActive();

        // Act
        Action act = () => user.ValidateCanLogin();

        // Assert
        act.Should().NotThrow();
    }

    [Fact]
    public void ValidateCanLogin_WhenInactive_ShouldThrowException()
    {
        // Arrange
        UserEntity user = UserFactory.CreateInactive();
        typeof(UserEntity).GetProperty("IsVerified")!.SetValue(user, true);

        // Act
        Action act = () => user.ValidateCanLogin();

        // Assert
        act.Should().Throw<Exception>();
    }

    [Fact]
    public void ValidateCanLogin_WhenNotVerifiedAndLocalAuth_ShouldThrowException()
    {
        // Arrange
        UserEntity user = UserFactory.CreateUnverified(); // Not verified, Local auth

        // Act
        Action act = () => user.ValidateCanLogin();

        // Assert
        act.Should().Throw<Exception>();
    }

    #endregion

    #region Role Management Tests

    [Fact]
    public void HasRole_WhenUserHasRole_ShouldReturnTrue()
    {
        // Arrange
        Guid roleId = Guid.NewGuid();
        UserRoleEntity userRole = UserRoleFactory.CreateWithRoleId(roleId);
        UserEntity user = UserFactory.Create();
        user.AssignRole(userRole);

        // Act
        bool result = user.HasRole(roleId);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void HasRole_WhenUserDoesNotHaveRole_ShouldReturnFalse()
    {
        // Arrange
        UserEntity user = UserFactory.Create();

        // Act
        bool result = user.HasRole(Guid.NewGuid());

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void AssignRole_WhenRoleNotAssigned_ShouldAddRole()
    {
        // Arrange
        UserEntity user = UserFactory.Create();
        Guid roleId = Guid.NewGuid();
        UserRoleEntity userRole = UserRoleFactory.Create(user.Id, roleId);

        // Act
        user.AssignRole(userRole);

        // Assert
        user.HasRole(roleId).Should().BeTrue();
        user.UserRoles.Should().ContainSingle();
    }

    [Fact]
    public void AssignRole_WhenRoleAlreadyAssigned_ShouldThrowException()
    {
        // Arrange
        Guid roleId = Guid.NewGuid();
        UserRoleEntity userRole = UserRoleFactory.CreateWithRoleId(roleId);
        UserEntity user = UserFactory.Create();
        user.AssignRole(userRole);

        UserRoleEntity duplicateRole = UserRoleFactory.CreateWithRoleId(roleId);

        // Act
        Action act = () => user.AssignRole(duplicateRole);

        // Assert
        act.Should().Throw<Exception>();
    }

    [Fact]
    public void RemoveRole_WhenRoleAssigned_ShouldRemoveAndReturnTrue()
    {
        // Arrange
        Guid roleId = Guid.NewGuid();
        UserRoleEntity userRole = UserRoleFactory.CreateWithRoleId(roleId);
        UserEntity user = UserFactory.Create();
        user.AssignRole(userRole);

        // Act
        bool result = user.RemoveRole(roleId);

        // Assert
        result.Should().BeTrue();
        user.HasRole(roleId).Should().BeFalse();
    }

    [Fact]
    public void RemoveRole_WhenRoleNotAssigned_ShouldReturnFalse()
    {
        // Arrange
        UserEntity user = UserFactory.Create();

        // Act
        bool result = user.RemoveRole(Guid.NewGuid());

        // Assert
        result.Should().BeFalse();
    }

    #endregion

    #region UpdateAvatar Tests

    [Fact]
    public void UpdateAvatar_ShouldUpdateAvatarProperties()
    {
        // Arrange
        UserEntity user = UserFactory.Create();
        Guid avatarFileId = Guid.NewGuid();

        // Act
        user.UpdateAvatar(avatarFileId, EnumAvatarSource.Manual);

        // Assert
        user.AvatarFileId.Should().Be(avatarFileId);
        user.AvatarSource.Should().Be(EnumAvatarSource.Manual);
    }

    [Fact]
    public void UpdateAvatar_WithNull_ShouldClearAvatar()
    {
        // Arrange
        UserEntity user = UserFactory.Create();
        user.UpdateAvatar(Guid.NewGuid(), EnumAvatarSource.Manual);

        // Act
        user.UpdateAvatar(null, EnumAvatarSource.None);

        // Assert
        user.AvatarFileId.Should().BeNull();
        user.AvatarSource.Should().Be(EnumAvatarSource.None);
    }

    #endregion

    #region UpdatePhoneNumber Tests

    [Fact]
    public void UpdatePhoneNumber_ShouldUpdateAllPhoneProperties()
    {
        // Arrange
        UserEntity user = UserFactory.Create();

        // Act
        user.UpdatePhoneNumber(
            TestConstants.User.ValidCountry,
            "US",
            "+1",
            TestConstants.User.ValidPhone,
            "***-***-7890"
        );

        // Assert
        user.CountryName.Should().Be(TestConstants.User.ValidCountry);
        user.CountryIsoCode.Should().Be("US");
        user.CountryDialCode.Should().Be("+1");
        user.FullPhoneNumber.Should().Be(TestConstants.User.ValidPhone);
        user.PartialPhoneNumber.Should().Be("***-***-7890");
    }

    [Fact]
    public void UpdatePhoneNumber_WithNulls_ShouldClearPhoneProperties()
    {
        // Arrange
        UserEntity user = UserFactory.Create();
        user.UpdatePhoneNumber("USA", "US", "+1", "+1234567890", "***-***-7890");

        // Act
        user.UpdatePhoneNumber(null, null, null, null, null);

        // Assert
        user.CountryName.Should().BeNull();
        user.CountryIsoCode.Should().BeNull();
        user.CountryDialCode.Should().BeNull();
        user.FullPhoneNumber.Should().BeNull();
        user.PartialPhoneNumber.Should().BeNull();
    }

    #endregion
}
