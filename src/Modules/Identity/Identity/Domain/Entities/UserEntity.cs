using System.ComponentModel.DataAnnotations;

using _116.BuildingBlocks.Constants;
using _116.Identity.Application.Shared.Errors;
using _116.Identity.Domain.Enums;
using _116.Shared.Domain;

namespace _116.Identity.Domain.Entities;

/// <summary>
/// Represents a user account with authentication credentials and profile information.
/// This is the main entity for user management - handles both login credentials and profile data.
/// </summary>
public class UserEntity : Aggregate<Guid>
{
    // Authentication & Identity
    /// <summary>
    /// User's email address. Required for local auth, optional for social providers.
    /// </summary>
    [MaxLength(length: UserConstants.MaxEmailLength)]
    public string? Email { get; private set; }

    /// <summary>
    /// Unique username for the user.
    /// </summary>
    [MaxLength(length: UserConstants.MaxUserNameLength)]
    public string UserName { get; private set; } = null!;

    /// <summary>
    /// Hashed password. Only set for local auth - null for social login users.
    /// </summary>
    public string? PasswordHash { get; private set; }

    /// <summary>
    /// How the user authenticated - Local (email/password), Google, Facebook, etc.
    /// </summary>
    public EnumAuthProvider AuthProvider { get; private set; }

    /// <summary>
    /// Whether the user has verified their email. Auto-true for social logins.
    /// </summary>
    public bool IsVerified { get; private set; } = UserConstants.DefaultIsVerified;

    /// <summary>
    /// Whether the account is active. Inactive users cannot log in.
    /// </summary>
    public bool IsActive { get; private set; } = UserConstants.DefaultIsActive;

    // Profile Data
    /// <summary>
    /// ID of the uploaded avatar file, if any.
    /// </summary>
    public Guid? AvatarFileId { get; private set; }

    /// <summary>
    /// Where the avatar came from - manually uploaded, from social provider, or none.
    /// </summary>
    public EnumAvatarSource AvatarSource { get; private set; } = EnumAvatarSource.None;

    /// <summary>
    /// Country name (e.g., "United States", "Rwanda").
    /// </summary>
    [MaxLength(length: UserConstants.MaxCountryNameLength)]
    public string? CountryName { get; private set; }

    /// <summary>
    /// ISO country code (e.g., "US", "RW").
    /// </summary>
    [MaxLength(length: UserConstants.MaxCountryIsoCodeLength)]
    public string? CountryIsoCode { get; private set; }

    /// <summary>
    /// Country dialing code (e.g., "+1", "+250").
    /// </summary>
    [MaxLength(length: UserConstants.MaxCountryDialCodeLength)]
    public string? CountryDialCode { get; private set; }

    /// <summary>
    /// Masked phone number for display (e.g., "***-***-1234").
    /// </summary>
    [MaxLength(length: UserConstants.MaxPartialPhoneNumberLength)]
    public string? PartialPhoneNumber { get; private set; }

    /// <summary>
    /// Full phone number with country code.
    /// </summary>
    [MaxLength(length: UserConstants.MaxFullPhoneNumberLength)]
    public string? FullPhoneNumber { get; private set; }

    // Navigation Properties
    /// <summary>
    /// Roles assigned to this user.
    /// </summary>
    public ICollection<UserRoleEntity> UserRoles { get; } = new List<UserRoleEntity>();

    /// <summary>
    /// Active login sessions for this user.
    /// </summary>
    public ICollection<SessionEntity> Sessions { get; private set; } = new List<SessionEntity>();

    // Factory Methods
    /// <summary>
    /// Creates a new user with local authentication (email + password).
    /// </summary>
    public static UserEntity Create(Guid id, string email, string userName, string passwordHash)
    {
        Exception? error = (email, userName, passwordHash) switch
        {
            var (e, _, _) when string.IsNullOrWhiteSpace(value: e) => UserErrors.InvalidEmailFormat(email: e),
            var (_, u, _) when string.IsNullOrWhiteSpace(value: u) || u.Length > UserConstants.MaxUserNameLength
                => UserErrors.InvalidUsernameFormat(username: u),
            var (_, _, p) when string.IsNullOrWhiteSpace(value: p) => UserErrors.InvalidPasswordFormat(),
            _ => null
        };
        if (error is not null)
        {
            throw error;
        }

        return new UserEntity
        {
            Id = id,
            Email = email.ToLowerInvariant(),
            UserName = userName,
            PasswordHash = passwordHash,
            AuthProvider = EnumAuthProvider.Local
        };
    }

    /// <summary>
    /// Creates a new user from external authentication (Google, Facebook, etc).
    /// Email is optional since some providers don't share it.
    /// </summary>
    public static UserEntity CreateExternal(
        Guid id,
        string userName,
        EnumAuthProvider authProvider,
        string? email = null
    )
    {
        if (string.IsNullOrWhiteSpace(value: userName))
        {
            throw UserErrors.InvalidUsernameFormat(username: userName);
        }

        return new UserEntity
        {
            Id = id,
            Email = email?.ToLowerInvariant(),
            UserName = userName,
            AuthProvider = authProvider,
            IsVerified = UserConstants.ExternalAuthIsVerified
        };
    }

    // Authentication Methods
    /// <summary>
    /// Updates the user's email. Resets verification status since they need to verify the new email.
    /// </summary>
    public void UpdateEmail(string newEmail)
    {
        if (string.IsNullOrWhiteSpace(value: newEmail))
        {
            throw UserErrors.InvalidEmailFormat(email: newEmail);
        }

        Email = newEmail.ToLowerInvariant();
        IsVerified = UserConstants.EmailUpdatedVerificationStatus;
    }

    /// <summary>
    /// Updates the password hash for existing local auth users.
    /// </summary>
    public void UpdatePassword(string newPasswordHash)
    {
        if (string.IsNullOrWhiteSpace(value: newPasswordHash))
        {
            throw UserErrors.InvalidPasswordFormat();
        }

        if (AuthProvider != EnumAuthProvider.Local && string.IsNullOrEmpty(value: Email))
        {
            throw UserErrors.BadRequest("Cannot update password for a user without email.");
        }

        PasswordHash = newPasswordHash;
    }

    /// <summary>
    /// Sets a password for social login users, converting them to local auth.
    /// Useful when a Google/Facebook user wants to also login with password.
    /// </summary>
    public void SetPasswordAndChangeToLocal(string passwordHash)
    {
        if (string.IsNullOrWhiteSpace(value: passwordHash))
        {
            throw UserErrors.InvalidPasswordFormat();
        }

        if (string.IsNullOrEmpty(value: Email))
        {
            throw UserErrors.EmailRequiredToSetPassword();
        }

        PasswordHash = passwordHash;
        AuthProvider = EnumAuthProvider.Local;
    }

    /// <summary>
    /// Updates the username. Must still be unique across all users.
    /// </summary>
    public void UpdateUserName(string newUserName)
    {
        if (string.IsNullOrWhiteSpace(value: newUserName) || newUserName.Length > UserConstants.MaxUserNameLength)
        {
            throw UserErrors.InvalidUsernameFormat(username: newUserName);
        }

        UserName = newUserName;
    }

    /// <summary>
    /// Marks the email as verified. Call this after successful email verification.
    /// </summary>
    public void MarkAsVerified()
    {
        IsVerified = UserConstants.ExternalAuthIsVerified;
    }

    /// <summary>
    /// Activates the account. User can now log in.
    /// </summary>
    public void Activate()
    {
        IsActive = UserConstants.ActivatedStatus;
    }

    /// <summary>
    /// Deactivates the account. User cannot log in anymore.
    /// You should also invalidate all their sessions separately.
    /// </summary>
    public void Deactivate()
    {
        IsActive = UserConstants.DeactivatedStatus;
    }

    /// <summary>
    /// Checks if this user can log in. Throws an exception if not.
    /// Call this before creating a new session.
    /// </summary>
    public void ValidateCanLogin()
    {
        if (!IsActive)
        {
            throw UserErrors.AccountInactive(Email!);
        }

        if (AuthProvider == EnumAuthProvider.Local && !IsVerified)
        {
            throw UserErrors.AccountNotVerified(Email!);
        }
    }

    // Profile Methods
    /// <summary>
    /// Updates or removes the user's avatar.
    /// </summary>
    public void UpdateAvatar(Guid? avatarFileId, EnumAvatarSource avatarSource)
    {
        AvatarFileId = avatarFileId;
        AvatarSource = avatarSource;
    }

    /// <summary>
    /// Updates the user's phone number and country information.
    /// Pass nulls to clear the phone number.
    /// </summary>
    public void UpdatePhoneNumber(
        string? countryName,
        string? countryIsoCode,
        string? countryDialCode,
        string? fullPhoneNumber,
        string? partialPhoneNumber
    )
    {
        CountryName = countryName;
        CountryIsoCode = countryIsoCode;
        CountryDialCode = countryDialCode;
        FullPhoneNumber = fullPhoneNumber;
        PartialPhoneNumber = partialPhoneNumber;
    }

    // Role Methods
    /// <summary>
    /// Assigns a role to this user. Throws if the role is already assigned.
    /// </summary>
    public void AssignRole(UserRoleEntity userRole)
    {
        ArgumentNullException.ThrowIfNull(argument: userRole);
        if (HasRole(roleId: userRole.RoleId))
        {
            throw UserErrors.RoleAlreadyAssignedToUser();
        }

        UserRoles.Add(item: userRole);
    }

    /// <summary>
    /// Removes a role from this user. Returns true if removed, false if wasn't assigned.
    /// </summary>
    public bool RemoveRole(Guid roleId)
    {
        UserRoleEntity? userRole = UserRoles.FirstOrDefault(ur => ur.RoleId == roleId);
        if (userRole == null)
        {
            return false;
        }

        UserRoles.Remove(item: userRole);
        return true;
    }

    /// <summary>
    /// Checks if this user has a specific role.
    /// </summary>
    public bool HasRole(Guid roleId)
    {
        return UserRoles.Any(ur => ur.RoleId == roleId);
    }
}
