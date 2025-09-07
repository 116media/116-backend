using System.ComponentModel.DataAnnotations;
using _116.BuildingBlocks.Constants;
using _116.Shared.Domain;
using _116.User.Application.Shared.Errors;
using _116.User.Domain.Enums;

namespace _116.User.Domain.Entities;

/// <summary>
/// Represents a user in the authentication system, containing identity, credential,
/// and profile-related information.
/// </summary>
/// <remarks>
/// This entity serves as an aggregate root with a unique identifier, managing
/// authentication and profile data such as email, password hash, roles, and login state.
/// </remarks>
public class UserEntity : Aggregate<Guid>
{
    /// <summary>
    /// Email address of the user.
    /// May be <c>null</c> for external authentication providers (e.g., Facebook).
    /// </summary>
    [MaxLength(UserConstants.MaxEmailLength)]
    public string? Email { get; private set; }

    /// <summary>
    /// Unique username chosen by the user (max {UserConstants.MaxUserNameLength} characters).
    /// </summary>
    [MaxLength(UserConstants.MaxUserNameLength)]
    public string UserName { get; private set; } = null!;

    /// <summary>
    /// Hashed password used for authentication.
    /// May be <c>null</c> for external authentication providers (e.g., Google).
    /// </summary>
    public string? PasswordHash { get; private set; }

    /// <summary>
    /// Identifier of the user's avatar file, if uploaded.
    /// </summary>
    public Guid? AvatarFileId { get; private set; }

    /// <summary>
    /// Navigation property:
    /// Collection of roles assigned to the user.
    /// </summary>
    public ICollection<UserRoleEntity> UserRoles { get; private set; } = new List<UserRoleEntity>();

    /// <summary>
    /// Authentication provider used by the user (e.g., Local, Google, Facebook).
    /// </summary>
    public AuthProvider AuthProvider { get; private set; }

    /// <summary>
    /// Indicates whether the user's email/account has been verified.
    /// </summary>
    public bool IsVerified { get; private set; } = UserConstants.DefaultIsVerified;

    /// <summary>
    /// Indicates whether the user account is currently active.
    /// </summary>
    public bool IsActive { get; private set; } = UserConstants.DefaultIsActive;

    /// <summary>
    /// Indicates whether the user is currently logged in.
    /// </summary>
    public bool IsLoggedIn { get; private set; } = UserConstants.DefaultIsLoggedIn;

    /// <summary>
    /// Date and time of the user's last login, in UTC.
    /// </summary>
    public DateTime? LastLoginAt { get; private set; }

    /// <summary>
    /// Full country name associated with the user.
    /// </summary>
    [MaxLength(UserConstants.MaxCountryNameLength)]
    public string? CountryName { get; private set; }

    /// <summary>
    /// URL link to the flag of the user's country.
    /// </summary>
    [MaxLength(UserConstants.MaxCountryFlagUrlLength)]
    public string? CountryFlagUrl { get; private set; }

    /// <summary>
    /// ISO country code (e.g., "US", "RW").
    /// </summary>
    [MaxLength(UserConstants.MaxCountryIsoCodeLength)]
    public string? CountryIsoCode { get; private set; }

    /// <summary>
    /// Country dialing code (e.g., "+1", "+250").
    /// </summary>
    [MaxLength(UserConstants.MaxCountryDialCodeLength)]
    public string? CountryDialCode { get; private set; }

    /// <summary>
    /// Partial (masked) phone number for privacy display.
    /// </summary>
    [MaxLength(UserConstants.MaxPartialPhoneNumberLength)]
    public string? PartialPhoneNumber { get; private set; }

    /// <summary>
    /// Full phone number including country code.
    /// </summary>
    [MaxLength(UserConstants.MaxFullPhoneNumberLength)]
    public string? FullPhoneNumber { get; private set; }

    /// <summary>
    /// Creates a new user entity with local authentication provider.
    /// </summary>
    /// <param name="id">The unique identifier of the user.</param>
    /// <param name="email">The user email address. Cannot be null for local authentication.</param>
    /// <param name="userName">The user's unique username (max {UserConstants.MaxUserNameLength} characters).</param>
    /// <param name="passwordHash">The hashed password for authentication.</param>
    /// <returns>A new <see cref="UserEntity"/> instance.</returns>
    /// <exception cref="ArgumentException">
    /// Thrown when email is null or empty for local auth, or username exceeds {UserConstants.MaxUserNameLength} characters.
    /// </exception>
    /// <exception cref="ArgumentNullException">Thrown when required parameters are null.</exception>
    /// <remarks>
    /// This method creates a user with the local authentication provider. The user starts as active but unverified.
    /// Email verification should be handled separately after creation.
    /// </remarks>
    public static UserEntity Create(Guid id, string email, string userName, string passwordHash)
    {
        Exception? error = (email, userName, passwordHash) switch
        {
            var (e, _, _) when string.IsNullOrWhiteSpace(e) => UserErrors.InvalidEmailFormat(e),

            var (_, u, _) when string.IsNullOrWhiteSpace(u) || u.Length > UserConstants.MaxUserNameLength
                => UserErrors.InvalidUsernameFormat(u),

            var (_, _, p) when string.IsNullOrWhiteSpace(p) => UserErrors.InvalidPasswordFormat(),

            _ => null
        };

        if (error is not null) throw error;

        return new UserEntity
        {
            Id = id,
            Email = email.ToLowerInvariant(),
            UserName = userName,
            PasswordHash = passwordHash,
            AuthProvider = AuthProvider.Local
        };
    }

    /// <summary>
    /// Creates a new user entity with external authentication provider.
    /// </summary>
    /// <param name="id">The unique identifier of the user.</param>
    /// <param name="userName">The user's unique username (max {UserConstants.MaxUserNameLength} characters).</param>
    /// <param name="authProvider">The external authentication provider (Google, Facebook, etc.).</param>
    /// <param name="email">Optional email address from the external provider.</param>
    /// <returns>A new <see cref="UserEntity"/> instance.</returns>
    /// <exception cref="ArgumentException">
    /// Thrown when the username exceeds {UserConstants.MaxUserNameLength} characters or auth provider is Local.
    /// </exception>
    /// <exception cref="ArgumentNullException">Thrown when username is null.</exception>
    /// <remarks>
    /// This method creates a user with the external authentication provider. Email may be null
    /// if not provided by the external provider. External users start as verified and active.
    /// </remarks>
    public static UserEntity CreateExternal(Guid id, string userName, AuthProvider authProvider, string? email = null)
    {
        if (string.IsNullOrWhiteSpace(userName))
        {
            throw UserErrors.InvalidUsernameFormat(userName);
        }

        var user = new UserEntity
        {
            Id = id,
            Email = email?.ToLowerInvariant(),
            UserName = userName,
            AuthProvider = authProvider,
            IsVerified = UserConstants.ExternalAuthIsVerified,
        };

        return user;
    }

    /// <summary>
    /// Updates the user's email address.
    /// </summary>
    /// <param name="newEmail">The new email address.</param>
    /// <exception cref="ArgumentException">Thrown when email is null or empty.</exception>
    /// <remarks>
    /// When email is changed, the user's verification status is reset to false for local authentication.
    /// </remarks>
    public void UpdateEmail(string newEmail)
    {
        if (string.IsNullOrWhiteSpace(newEmail))
        {
            throw UserErrors.InvalidEmailFormat(newEmail);
        }

        Email = newEmail.ToLowerInvariant();
        IsVerified = UserConstants.EmailUpdatedVerificationStatus;
    }

    /// <summary>
    /// Updates the user's password hash.
    /// </summary>
    /// <param name="newPasswordHash">The new hashed password.</param>
    /// <exception cref="ArgumentNullException">Thrown when password hash is null or empty.</exception>
    public void UpdatePassword(string newPasswordHash)
    {
        if (string.IsNullOrWhiteSpace(newPasswordHash))
        {
            throw UserErrors.InvalidPasswordFormat();
        }

        if (AuthProvider != AuthProvider.Local && string.IsNullOrEmpty(Email))
        {
            throw UserErrors.BadRequest("Cannot update password for a user without email.");
        }

        PasswordHash = newPasswordHash;
    }

    /// <summary>
    /// Updates the user's username.
    /// </summary>
    /// <param name="newUserName">The new username.</param>
    /// <exception cref="ArgumentException">Thrown when username is null, empty, or exceeds maximum length.</exception>
    /// <remarks>
    /// Updates the username for existing users. Username must be unique and follow validation rules.
    /// </remarks>
    public void UpdateUserName(string newUserName)
    {
        if (string.IsNullOrWhiteSpace(newUserName) || newUserName.Length > UserConstants.MaxUserNameLength)
        {
            throw UserErrors.InvalidUsernameFormat(newUserName);
        }

        UserName = newUserName;
    }

    /// <summary>
    /// Marks the user's email as verified.
    /// </summary>
    /// <remarks>
    /// This method should be called after a successful email verification process.
    /// </remarks>
    public void MarkAsVerified() => IsVerified = UserConstants.ExternalAuthIsVerified;

    /// <summary>
    /// Activates the user account.
    /// </summary>
    /// <remarks>
    /// Activated users can log in and use the system.
    /// </remarks>
    public void Activate() => IsActive = UserConstants.ActivatedStatus;

    /// <summary>
    /// Deactivates the user account.
    /// </summary>
    /// <remarks>
    /// Deactivated users cannot log in or use the system.
    /// </remarks>
    public void Deactivate()
    {
        IsActive = UserConstants.DeactivatedStatus;
        // Force logout when deactivating
        IsLoggedIn = UserConstants.LoggedOutStatus;
    }

    /// <summary>
    /// Records a successful login for the user.
    /// </summary>
    /// <exception cref="InvalidOperationException">
    /// Thrown when the user is not active or not verified (for local auth).
    /// </exception>
    /// <remarks>
    /// Updates the login status and last login timestamp.
    /// </remarks>
    public void RecordLogin()
    {
        if (!IsActive)
        {
            throw UserErrors.AccountInactive(Email!);
        }

        if (AuthProvider == AuthProvider.Local && !IsVerified)
        {
            throw UserErrors.AccountNotVerified(Email!);
        }

        IsLoggedIn = UserConstants.LoggedInStatus;
        LastLoginAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Records a logout for the user.
    /// </summary>
    /// <remarks>
    /// Updates the login status to false.
    /// </remarks>
    public void RecordLogout() => IsLoggedIn = UserConstants.LoggedOutStatus;

    /// <summary>
    /// Updates the user's avatar file reference.
    /// </summary>
    /// <param name="avatarFileId">The identifier of the avatar file, or null to remove avatar.</param>
    /// <remarks>
    /// Associates the user with an uploaded avatar file from the file management system.
    /// </remarks>
    public void UpdateAvatar(Guid? avatarFileId) => AvatarFileId = avatarFileId;

    /// <summary>
    /// Updates the user's phone number information.
    /// </summary>
    /// <param name="countryName">Full country name.</param>
    /// <param name="countryFlagUrl">URL to the country flag image.</param>
    /// <param name="countryIsoCode">ISO country code (e.g., "US", "RW").</param>
    /// <param name="countryDialCode">Country dialing code (e.g., "+1", "+250").</param>
    /// <param name="fullPhoneNumber">Complete phone number including country code.</param>
    /// <param name="partialPhoneNumber">Masked phone number for privacy display.</param>
    /// <remarks>
    ///  Updates all country-related information in a single operation to maintain consistency.
    ///  Updates both full and partial phone numbers to maintain consistency for privacy and display purposes.
    /// </remarks>
    public void UpdatePhoneNumber(
        string? countryName,
        string? countryFlagUrl,
        string? countryIsoCode,
        string? countryDialCode,
        string? fullPhoneNumber,
        string? partialPhoneNumber
    )
    {
        CountryName = countryName;
        CountryFlagUrl = countryFlagUrl;
        CountryIsoCode = countryIsoCode;
        CountryDialCode = countryDialCode;
        FullPhoneNumber = fullPhoneNumber;
        PartialPhoneNumber = partialPhoneNumber;
    }

    /// <summary>
    /// Assigns a role to the user.
    /// </summary>
    /// <param name="userRole">The user-role association to add.</param>
    /// <exception cref="ArgumentNullException">Thrown when userRole is null.</exception>
    /// <exception cref="InvalidOperationException">Thrown when the role is already assigned.</exception>
    /// <remarks>
    /// Adds a new role assignment if not already present.
    /// </remarks>
    public void AssignRole(UserRoleEntity userRole)
    {
        ArgumentNullException.ThrowIfNull(userRole);

        if (HasRole(userRole.RoleId))
        {
            throw UserErrors.RoleAlreadyAssignedToUser();
        }

        UserRoles.Add(userRole);
    }

    /// <summary>
    /// Removes a role from the user.
    /// </summary>
    /// <param name="roleId">The identifier of the role to remove.</param>
    /// <returns>True if the role was removed, false if the role was not assigned.</returns>
    /// <remarks>
    /// Removes the role assignment if it exists.
    /// </remarks>
    public bool RemoveRole(Guid roleId)
    {
        UserRoleEntity? userRole = UserRoles.FirstOrDefault(ur => ur.RoleId == roleId);

        if (userRole == null)
        {
            return UserConstants.DeactivatedStatus;
        }

        UserRoles.Remove(userRole);
        return UserConstants.ActivatedStatus;
    }

    /// <summary>
    /// Checks if the user has a specific role assigned.
    /// </summary>
    /// <param name="roleId">The identifier of the role to check.</param>
    /// <returns>True if the user has the role, false otherwise.</returns>
    /// <remarks>
    /// Useful for authorization and role-based access control.
    /// </remarks>
    public bool HasRole(Guid roleId) => UserRoles.Any(ur => ur.RoleId == roleId);

}
