using _116.Identity.Application.Shared.Errors;
using _116.Identity.Domain.Entities;
using _116.Identity.Domain.Enums;
using _116.Identity.Domain.Events;
using _116.Shared.Application.Exceptions;
using _116.Tests.Fixtures.Builders.Entities.Identity;
using _116.Tests.Fixtures.Constants;
using _116.Tests.Fixtures.Factories.Identity;
using _116.Tests.Fixtures.Helpers;
using AwesomeAssertions;
using Xunit;

namespace _116.Unit.Tests.Modules.Identity.Domain.Entities;

/// <summary>
/// Unit tests for <see cref="UserEntity"/>.
/// </summary>
public class UserEntityTests
{
    private readonly UserErrors _userErrors = TestErrorsFactory.CreateUserErrors();

    #region Create Tests

    [Fact]
    public void Create_WithValidParameters_ShouldCreateUser()
    {
        // Arrange
        var id = Guid.NewGuid();
        string email = TestConstants.User.ValidEmail;
        string userName = TestConstants.User.ValidUserName;
        string passwordHash = TestConstants.User.DefaultPasswordHash;

        // Act
        var user = UserEntity.Create(id, email, userName, passwordHash, TestErrorsFactory.CreateUserErrors());

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
        var id = Guid.NewGuid();

        // Act
        Action act = () =>
            UserEntity.Create(
                id,
                invalidEmail!,
                TestConstants.User.ValidUserName,
                TestConstants.User.DefaultPasswordHash,
                TestErrorsFactory.CreateUserErrors()
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
        var id = Guid.NewGuid();

        // Act
        Action act = () =>
            UserEntity.Create(
                id,
                TestConstants.User.ValidEmail,
                invalidUserName!,
                TestConstants.User.DefaultPasswordHash,
                TestErrorsFactory.CreateUserErrors()
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
        var id = Guid.NewGuid();

        // Act
        Action act = () =>
            UserEntity.Create(
                id,
                TestConstants.User.ValidEmail,
                TestConstants.User.ValidUserName,
                invalidPasswordHash!,
                TestErrorsFactory.CreateUserErrors()
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
        var id = Guid.NewGuid();
        string userName = TestConstants.User.ValidUserName;
        string email = TestConstants.User.ValidEmail;

        // Act
        var user = UserEntity.CreateExternal(
            id,
            userName,
            EnumAuthProvider.Google,
            $"sub-{Guid.NewGuid():N}",
            TestErrorsFactory.CreateUserErrors(),
            email
        );

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
        var id = Guid.NewGuid();
        string userName = TestConstants.User.ValidUserName;

        // Act
        var user = UserEntity.CreateExternal(
            id,
            userName,
            EnumAuthProvider.Facebook,
            $"sub-{Guid.NewGuid():N}",
            TestErrorsFactory.CreateUserErrors()
        );

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
        var id = Guid.NewGuid();

        // Act
        Action act = () =>
            UserEntity.CreateExternal(
                id,
                invalidUserName!,
                EnumAuthProvider.Google,
                $"sub-{Guid.NewGuid():N}",
                TestErrorsFactory.CreateUserErrors()
            );

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
        user.UpdateEmail(newEmail, _userErrors);

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
        Action act = () => user.UpdateEmail(invalidEmail!, _userErrors);

        // Assert
        act.Should().Throw<Exception>();
    }

    #endregion

    #region InitializePasswordHash Tests

    [Fact]
    public void InitializePasswordHash_WithValidHash_ShouldUpdatePassword()
    {
        // Arrange
        UserEntity user = UserFactory.Create();
        string newPasswordHash = "new_hashed_password_value";

        // Act
        user.InitializePasswordHash(newPasswordHash, _userErrors);

        // Assert
        user.PasswordHash.Should().Be(newPasswordHash);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void InitializePasswordHash_WithInvalidHash_ShouldThrowException(string? invalidHash)
    {
        // Arrange
        UserEntity user = UserFactory.Create();

        // Act
        Action act = () => user.InitializePasswordHash(invalidHash!, _userErrors);

        // Assert
        act.Should().Throw<Exception>();
    }

    [Fact]
    public void InitializePasswordHash_WhenNonLocalUserWithoutEmail_ShouldThrowException()
    {
        // Arrange
        UserEntity user = UserFactory.CreateExternalWithoutEmail(EnumAuthProvider.Google);

        // Act
        Action act = () => user.InitializePasswordHash("new_password_hash", _userErrors);

        // Assert
        act.Should().Throw<BadRequestException>().WithMessage("An email address is required to set a password.");
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
        user.SetPasswordAndChangeToLocal(passwordHash, _userErrors);

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
        Action act = () => user.SetPasswordAndChangeToLocal(invalidPassword!, _userErrors);

        // Assert
        act.Should().Throw<Exception>().WithMessage("*Password does not meet security requirements.*");
    }

    [Fact]
    public void SetPasswordAndChangeToLocal_WhenUserHasNoEmail_ShouldThrowException()
    {
        // Arrange
        UserEntity user = UserFactory.CreateExternalWithoutEmail(EnumAuthProvider.Facebook);

        // Act
        Action act = () => user.SetPasswordAndChangeToLocal("valid_password_hash", _userErrors);

        // Assert
        act.Should().Throw<BadRequestException>().WithMessage("An email address is required to set a password.");
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
        user.UpdateUserName(newUserName, _userErrors);

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
        Action act = () => user.UpdateUserName(invalidUserName!, _userErrors);

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
        Action act = () => user.ValidateCanLogin(_userErrors);

        // Assert
        act.Should().NotThrow();
    }

    [Fact]
    public void ValidateCanLogin_WhenInactive_ShouldThrowException()
    {
        // Arrange
        UserEntity user = new UserBuilder().AsInactive().AsVerified().Build();

        // Act
        Action act = () => user.ValidateCanLogin(_userErrors);

        // Assert
        act.Should().Throw<Exception>();
    }

    [Fact]
    public void ValidateCanLogin_WhenNotVerifiedAndLocalAuth_ShouldThrowException()
    {
        // Arrange
        UserEntity user = UserFactory.CreateUnverified(); // Not verified, Local auth

        // Act
        Action act = () => user.ValidateCanLogin(_userErrors);

        // Assert
        act.Should().Throw<Exception>();
    }

    #endregion

    #region Role Management Tests

    [Fact]
    public void HasRole_WhenUserHasRole_ShouldReturnTrue()
    {
        // Arrange
        var roleId = Guid.NewGuid();
        UserRoleEntity userRole = UserRoleFactory.CreateWithRoleId(roleId);
        UserEntity user = UserFactory.Create();
        user.AssignRole(userRole, TestErrorsFactory.CreateUserErrors());

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
        var roleId = Guid.NewGuid();
        UserRoleEntity userRole = UserRoleFactory.Create(user.Id, roleId);

        // Act
        user.AssignRole(userRole, TestErrorsFactory.CreateUserErrors());

        // Assert
        user.HasRole(roleId).Should().BeTrue();
        user.UserRoles.Should().ContainSingle();
    }

    [Fact]
    public void AssignRole_WhenRoleAlreadyAssigned_ShouldThrowException()
    {
        // Arrange
        var roleId = Guid.NewGuid();
        UserRoleEntity userRole = UserRoleFactory.CreateWithRoleId(roleId);
        UserEntity user = UserFactory.Create();
        user.AssignRole(userRole, TestErrorsFactory.CreateUserErrors());

        UserRoleEntity duplicateRole = UserRoleFactory.CreateWithRoleId(roleId);

        // Act
        Action act = () => user.AssignRole(duplicateRole, TestErrorsFactory.CreateUserErrors());

        // Assert
        act.Should().Throw<Exception>();
    }

    #endregion

    #region UpdateAvatar Tests

    [Fact]
    public void UpdateAvatar_ShouldUpdateAvatarProperties()
    {
        // Arrange
        UserEntity user = UserFactory.Create();
        var avatarFileId = Guid.NewGuid();

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

    #region Domain Event Tests

    [Fact]
    public void MarkAsVerified_WhenNotVerified_ShouldRaiseUserVerifiedEvent()
    {
        // Arrange
        UserEntity user = UserFactory.Create();

        // Act
        user.MarkAsVerified();

        // Assert
        user.DomainEvents.Should().ContainSingle(e => e is UserVerifiedEvent);
        user.DomainEvents.OfType<UserVerifiedEvent>().Single().UserId.Should().Be(user.Id);
    }

    [Fact]
    public void MarkAsVerified_WhenAlreadyVerified_ShouldNotRaiseEvent()
    {
        // Arrange
        UserEntity user = UserFactory.Create();
        user.MarkAsVerified();
        user.ClearDomainEvents();

        // Act
        user.MarkAsVerified();

        // Assert
        user.IsVerified.Should().BeTrue();
        user.DomainEvents.Should().BeEmpty();
    }

    [Fact]
    public void UpdatePassword_WithOrigin_ShouldRaiseUserPasswordChangedEvent()
    {
        // Arrange
        UserEntity user = UserFactory.Create();

        // Act
        user.UpdatePassword("new_hashed_password_value", _userErrors, EnumPasswordChangeOrigin.Changed);

        // Assert
        UserPasswordChangedEvent raised = user.DomainEvents.OfType<UserPasswordChangedEvent>().Single();
        raised.UserId.Should().Be(user.Id);
        raised.Origin.Should().Be(EnumPasswordChangeOrigin.Changed);
    }

    [Fact]
    public void UpdatePassword_WithResetOrigin_ShouldRaiseEventCarryingTheResetOrigin()
    {
        // Arrange
        UserEntity user = UserFactory.Create();

        // Act
        user.UpdatePassword("new_hashed_password_value", _userErrors, EnumPasswordChangeOrigin.Reset);

        // Assert
        UserPasswordChangedEvent raised = user.DomainEvents.OfType<UserPasswordChangedEvent>().Single();
        raised.Origin.Should().Be(EnumPasswordChangeOrigin.Reset);
    }

    [Fact]
    public void InitializePasswordHash_ShouldNotRaiseEvent()
    {
        // Arrange
        UserEntity user = UserFactory.Create();

        // Act
        user.InitializePasswordHash("new_hashed_password_value", _userErrors);

        // Assert
        user.PasswordHash.Should().Be("new_hashed_password_value");
        user.DomainEvents.Should().BeEmpty();
    }

    [Fact]
    public void SetPasswordAndChangeToLocal_ShouldRaiseUserPasswordChangedEventWithSetLocalOrigin()
    {
        // Arrange
        UserEntity user = UserFactory.CreateExternal(EnumAuthProvider.Google);

        // Act
        user.SetPasswordAndChangeToLocal("hashed_password_value", _userErrors);

        // Assert
        UserPasswordChangedEvent raised = user.DomainEvents.OfType<UserPasswordChangedEvent>().Single();
        raised.UserId.Should().Be(user.Id);
        raised.Origin.Should().Be(EnumPasswordChangeOrigin.SetLocal);
    }

    [Fact]
    public void UpdateEmail_ShouldRaiseUserEmailChangedEventWithOldAndNewAddresses()
    {
        // Arrange
        UserEntity user = UserFactory.Create();
        string? oldEmail = user.Email;

        // Act
        user.UpdateEmail("changed@example.com", _userErrors);

        // Assert
        UserEmailChangedEvent raised = user.DomainEvents.OfType<UserEmailChangedEvent>().Single();
        raised.UserId.Should().Be(user.Id);
        raised.OldEmail.Should().Be(oldEmail);
        raised.NewEmail.Should().Be("changed@example.com");
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void RecordMassSignOut_ShouldRaiseUserSignedOutAllDevicesEvent(bool byAdmin)
    {
        // Arrange
        UserEntity user = UserFactory.Create();

        // Act
        user.RecordMassSignOut(byAdmin);

        // Assert
        UserSignedOutAllDevicesEvent raised = user.DomainEvents.OfType<UserSignedOutAllDevicesEvent>().Single();
        raised.UserId.Should().Be(user.Id);
        raised.ByAdmin.Should().Be(byAdmin);
    }

    #endregion
}
