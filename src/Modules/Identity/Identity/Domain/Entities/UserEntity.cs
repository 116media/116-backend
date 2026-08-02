using System.ComponentModel.DataAnnotations;
using _116.BuildingBlocks.Constants;
using _116.Identity.Application.Shared.Errors;
using _116.Identity.Domain.Enums;
using _116.Identity.Domain.Events;
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
    public static UserEntity Create(Guid id, string email, string userName, string passwordHash, UserErrors errors)
    {
        Exception? error = (email, userName, passwordHash) switch
        {
            var (e, _, _) when string.IsNullOrWhiteSpace(value: e) => errors.InvalidEmailFormat(email: e),
            var (_, u, _) when string.IsNullOrWhiteSpace(value: u) || u.Length > UserConstants.MaxUserNameLength =>
                errors.InvalidUsernameFormat(username: u),
            var (_, _, p) when string.IsNullOrWhiteSpace(value: p) => errors.InvalidPasswordFormat(),
            _ => null,
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
            AuthProvider = EnumAuthProvider.Local,
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
        UserErrors errors,
        string? email = null
    )
    {
        if (string.IsNullOrWhiteSpace(value: userName))
        {
            throw errors.InvalidUsernameFormat(username: userName);
        }

        return new UserEntity
        {
            Id = id,
            Email = email?.ToLowerInvariant(),
            UserName = userName,
            AuthProvider = authProvider,
            IsVerified = UserConstants.ExternalAuthIsVerified,
        };
    }

    // Authentication Methods
    /// <summary>
    /// Updates the user's email. Resets verification status since they need to verify the new email.
    /// Raises <see cref="UserEmailChangedEvent" /> carrying both addresses.
    /// </summary>
    /// <param name="newEmail">The new email address.</param>
    /// <param name="errors">User domain error factory for generating domain exceptions.</param>
    public void UpdateEmail(string newEmail, UserErrors errors)
    {
        if (string.IsNullOrWhiteSpace(value: newEmail))
        {
            throw errors.InvalidEmailFormat(email: newEmail);
        }

        string? oldEmail = Email;
        Email = newEmail.ToLowerInvariant();
        IsVerified = UserConstants.EmailUpdatedVerificationStatus;

        AddDomainEvent(new UserEmailChangedEvent(UserId: Id, OldEmail: oldEmail, NewEmail: Email));
    }

    /// <summary>
    /// Installs a password hash without declaring a password-change fact. This is the
    /// initialization path used to give an account a known credential outside a user-facing flow,
    /// such as seeding; user-facing flows call <see cref="UpdatePassword" /> so the change fans
    /// out to its notification reactions.
    /// </summary>
    /// <param name="newPasswordHash">The new hashed password.</param>
    /// <param name="errors">User domain error factory for generating domain exceptions.</param>
    public void InitializePasswordHash(string newPasswordHash, UserErrors errors)
    {
        if (string.IsNullOrWhiteSpace(value: newPasswordHash))
        {
            throw errors.InvalidPasswordFormat();
        }

        if (AuthProvider != EnumAuthProvider.Local && string.IsNullOrEmpty(value: Email))
        {
            throw errors.EmailRequiredToSetPassword();
        }

        PasswordHash = newPasswordHash;
    }

    /// <summary>
    /// Updates the password hash for existing local auth users.
    /// Raises <see cref="UserPasswordChangedEvent" /> with the flow that replaced the password.
    /// </summary>
    /// <param name="newPasswordHash">The new hashed password.</param>
    /// <param name="errors">User domain error factory for generating domain exceptions.</param>
    /// <param name="origin">The flow that replaced the password.</param>
    public void UpdatePassword(string newPasswordHash, UserErrors errors, EnumPasswordChangeOrigin origin)
    {
        InitializePasswordHash(newPasswordHash: newPasswordHash, errors: errors);

        AddDomainEvent(new UserPasswordChangedEvent(UserId: Id, Origin: origin));
    }

    /// <summary>
    /// Sets a password for social login users, converting them to local auth.
    /// Useful when a Google/Facebook user wants to also login with password.
    /// Raises <see cref="UserPasswordChangedEvent" /> with the set-local origin.
    /// </summary>
    /// <param name="passwordHash">The hashed password to set.</param>
    /// <param name="errors">User domain error factory for generating domain exceptions.</param>
    public void SetPasswordAndChangeToLocal(string passwordHash, UserErrors errors)
    {
        if (string.IsNullOrWhiteSpace(value: passwordHash))
        {
            throw errors.InvalidPasswordFormat();
        }

        if (string.IsNullOrEmpty(value: Email))
        {
            throw errors.EmailRequiredToSetPassword();
        }

        PasswordHash = passwordHash;
        AuthProvider = EnumAuthProvider.Local;

        AddDomainEvent(new UserPasswordChangedEvent(UserId: Id, Origin: EnumPasswordChangeOrigin.SetLocal));
    }

    /// <summary>
    /// Updates the username. Must still be unique across all users.
    /// </summary>
    public void UpdateUserName(string newUserName, UserErrors errors)
    {
        if (string.IsNullOrWhiteSpace(value: newUserName) || newUserName.Length > UserConstants.MaxUserNameLength)
        {
            throw errors.InvalidUsernameFormat(username: newUserName);
        }

        UserName = newUserName;
    }

    /// <summary>
    /// Marks the email as verified. Call this after successful email verification.
    /// Raises <see cref="UserVerifiedEvent" /> only on the actual transition, so verifying an
    /// already-verified account stays a silent no-op.
    /// </summary>
    public void MarkAsVerified()
    {
        if (IsVerified)
        {
            return;
        }

        IsVerified = UserConstants.ExternalAuthIsVerified;

        AddDomainEvent(new UserVerifiedEvent(UserId: Id));
    }

    /// <summary>
    /// Records that every session on this account was terminated at once. The session rows are
    /// revoked by the caller in the same transaction; this method only declares the account-level
    /// fact by raising <see cref="UserSignedOutAllDevicesEvent" />.
    /// </summary>
    /// <param name="byAdmin">Whether an administrator drove the termination.</param>
    public void RecordMassSignOut(bool byAdmin)
    {
        AddDomainEvent(new UserSignedOutAllDevicesEvent(UserId: Id, ByAdmin: byAdmin));
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
    public void ValidateCanLogin(UserErrors errors)
    {
        if (!IsActive)
        {
            throw errors.AccountInactive(Email!);
        }

        if (AuthProvider == EnumAuthProvider.Local && !IsVerified)
        {
            throw errors.AccountNotVerified(Email!);
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
    public void AssignRole(UserRoleEntity userRole, UserErrors errors)
    {
        ArgumentNullException.ThrowIfNull(argument: userRole);
        if (HasRole(roleId: userRole.RoleId))
        {
            throw errors.RoleAlreadyAssignedToUser();
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
