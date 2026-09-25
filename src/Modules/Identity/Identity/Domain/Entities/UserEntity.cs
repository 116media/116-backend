using System.ComponentModel.DataAnnotations;
using _116.BuildingBlocks.Constants;
using _116.Identity.Domain.Enums;
using _116.Identity.Domain.Events;
using _116.Identity.Domain.Exceptions;
using _116.Identity.Domain.StateMachines;
using _116.Identity.Domain.ValueObjects;
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
    public Email? Email { get; private set; }

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
    /// The provider's stable subject id (Google <c>sub</c>, Facebook user id). Null for local accounts,
    /// and for legacy external accounts that predate subject-id tracking until their first verified
    /// login links it.
    /// </summary>
    [MaxLength(length: UserConstants.MaxProviderSubjectIdLength)]
    public string? ProviderSubjectId { get; private set; }

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
    /// <summary>
    /// The two-letter locale this user's mail and notifications render in, independent of the
    /// culture of whoever triggered them.
    /// </summary>
    public string PreferredLocale { get; private set; } = UserConstants.DefaultLocale;

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
        Exception? error = (userName, passwordHash) switch
        {
            var (u, _) when string.IsNullOrWhiteSpace(value: u) || u.Length > UserConstants.MaxUserNameLength =>
                new IdentityRuleException(IdentityRuleCodes.InvalidUsernameFormat, u),
            var (_, p) when string.IsNullOrWhiteSpace(value: p) => new IdentityRuleException(
                IdentityRuleCodes.InvalidPasswordFormat
            ),
            _ => null,
        };
        if (error is not null)
        {
            throw error;
        }

        return new UserEntity
        {
            Id = id,
            Email = new Email(value: email),
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
        string providerSubjectId,
        string? email = null
    )
    {
        if (string.IsNullOrWhiteSpace(value: userName))
        {
            throw new IdentityRuleException(IdentityRuleCodes.InvalidUsernameFormat, userName);
        }

        return new UserEntity
        {
            Id = id,
            Email = email is null ? null : new Email(value: email),
            UserName = userName,
            AuthProvider = authProvider,
            ProviderSubjectId = providerSubjectId,
            IsVerified = UserConstants.ExternalAuthIsVerified,
        };
    }

    /// <summary>
    /// Associates a provider subject id with an external account that predates subject-id tracking.
    /// Refuses to rebind an account already tied to a different subject — that is a mismatched token.
    /// </summary>
    /// <param name="providerSubjectId">The verified provider subject id to link.</param>
    public void LinkProviderSubject(string providerSubjectId)
    {
        if (!string.IsNullOrWhiteSpace(value: ProviderSubjectId) && ProviderSubjectId != providerSubjectId)
        {
            throw new IdentityRuleException(IdentityRuleCodes.ProviderMismatch);
        }

        ProviderSubjectId = providerSubjectId;
    }

    // Authentication Methods
    /// <summary>
    /// Updates the user's email. Resets verification status since they need to verify the new email.
    /// Raises <see cref="UserEmailChangedEvent" /> carrying both addresses.
    /// </summary>
    /// <param name="newEmail">The new email address.</param>
    public void UpdateEmail(string newEmail)
    {
        string? oldEmail = Email?.Value;
        Email = new Email(value: newEmail);
        IsVerified = UserConstants.EmailUpdatedVerificationStatus;

        AddDomainEvent(new UserEmailChangedEvent(UserId: Id, OldEmail: oldEmail, NewEmail: Email.Value));
    }

    /// <summary>
    /// Installs a password hash without declaring a password-change fact. This is the
    /// initialization path used to give an account a known credential outside a user-facing flow,
    /// such as seeding; user-facing flows call <see cref="UpdatePassword" /> so the change fans
    /// out to its notification reactions.
    /// </summary>
    /// <param name="newPasswordHash">The new hashed password.</param>
    public void InitializePasswordHash(string newPasswordHash)
    {
        if (string.IsNullOrWhiteSpace(value: newPasswordHash))
        {
            throw new IdentityRuleException(IdentityRuleCodes.InvalidPasswordFormat);
        }

        if (AuthProvider != EnumAuthProvider.Local && Email is null)
        {
            throw new IdentityRuleException(IdentityRuleCodes.EmailRequiredToSetPassword);
        }

        PasswordHash = newPasswordHash;
    }

    /// <summary>
    /// Updates the password hash for existing local auth users.
    /// Raises <see cref="UserPasswordChangedEvent" /> with the flow that replaced the password.
    /// </summary>
    /// <param name="newPasswordHash">The new hashed password.</param>
    /// <param name="origin">The flow that replaced the password.</param>
    public void UpdatePassword(string newPasswordHash, EnumPasswordChangeOrigin origin)
    {
        InitializePasswordHash(newPasswordHash: newPasswordHash);

        AddDomainEvent(new UserPasswordChangedEvent(UserId: Id, Origin: origin));
    }

    /// <summary>
    /// Sets a password for social login users, converting them to local auth.
    /// Useful when a Google/Facebook user wants to also login with password.
    /// Raises <see cref="UserPasswordChangedEvent" /> with the set-local origin.
    /// </summary>
    /// <param name="passwordHash">The hashed password to set.</param>
    public void SetPasswordAndChangeToLocal(string passwordHash)
    {
        if (string.IsNullOrWhiteSpace(value: passwordHash))
        {
            throw new IdentityRuleException(IdentityRuleCodes.InvalidPasswordFormat);
        }

        if (Email is null)
        {
            throw new IdentityRuleException(IdentityRuleCodes.EmailRequiredToSetPassword);
        }

        PasswordHash = passwordHash;
        AuthProvider = EnumAuthProvider.Local;

        AddDomainEvent(new UserPasswordChangedEvent(UserId: Id, Origin: EnumPasswordChangeOrigin.SetLocal));
    }

    /// <summary>
    /// Updates the username. Must still be unique across all users.
    /// </summary>
    public void UpdateUserName(string newUserName)
    {
        if (string.IsNullOrWhiteSpace(value: newUserName) || newUserName.Length > UserConstants.MaxUserNameLength)
        {
            throw new IdentityRuleException(IdentityRuleCodes.InvalidUsernameFormat, newUserName);
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
    /// Marks the email as verified when the presented code proves the address. Only an
    /// email-verification purpose may flip <see cref="IsVerified" />; a reset or recovery code
    /// must not silently confirm an unproven address.
    /// </summary>
    /// <param name="purpose">The purpose of the code that was just verified.</param>
    /// <returns><c>true</c> if the account transitioned to verified; <c>false</c> otherwise.</returns>
    public bool MarkVerifiedByOtp(EnumOtpPurpose purpose)
    {
        if (purpose != EnumOtpPurpose.EmailVerification || IsVerified)
        {
            return false;
        }

        MarkAsVerified();
        return true;
    }

    /// <summary>
    /// Records that every session on this account was terminated at once. The session rows are
    /// revoked by the caller in the same transaction; the account-level fact is raised here
    /// because the session family has no single aggregate to raise one event from (D14).
    /// </summary>
    /// <param name="byAdmin">Whether an administrator drove the termination.</param>
    public void RecordMassSignOut(bool byAdmin)
    {
        AddDomainEvent(new UserSignedOutAllDevicesEvent(UserId: Id, ByAdmin: byAdmin));
    }

    /// <summary>
    /// Activates the account so the user can log in again, raising
    /// <see cref="UserActivatedEvent" />. Idempotent: an active account reports <c>false</c>
    /// and raises nothing.
    /// </summary>
    /// <returns><c>true</c> if the account transitioned; <c>false</c> if already active.</returns>
    public bool Activate()
    {
        if (IsActive)
        {
            return false;
        }

        IsActive = UserConstants.ActivatedStatus;

        AddDomainEvent(new UserActivatedEvent(UserId: Id));
        return true;
    }

    /// <summary>
    /// Deactivates the account so the user can no longer log in, raising
    /// <see cref="UserDeactivatedEvent" />; consumers revoke the account's live sessions.
    /// Idempotent: an inactive account reports <c>false</c> and raises nothing.
    /// </summary>
    /// <returns><c>true</c> if the account transitioned; <c>false</c> if already inactive.</returns>
    public bool Deactivate()
    {
        if (!IsActive)
        {
            return false;
        }

        IsActive = UserConstants.DeactivatedStatus;

        AddDomainEvent(new UserDeactivatedEvent(UserId: Id));
        return true;
    }

    /// <summary>
    /// Checks if this user can log in. Throws an exception if not.
    /// Call this before creating a new session.
    /// </summary>
    public void ValidateCanLogin()
    {
        if (!IsActive)
        {
            throw new IdentityRuleException(IdentityRuleCodes.AccountInactive, Email?.Value ?? string.Empty);
        }

        if (AuthProvider == EnumAuthProvider.Local && !IsVerified)
        {
            throw new IdentityRuleException(IdentityRuleCodes.AccountNotVerified, Email?.Value ?? string.Empty);
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
    /// Grants a role to this user and raises <see cref="UserRoleGrantedEvent" /> carrying the
    /// role name. Idempotent: a role already granted reports <c>false</c> and raises nothing.
    /// </summary>
    /// <param name="roleId">The ID of the role to grant.</param>
    /// <param name="roleName">The granted role's name, carried by the event.</param>
    /// <returns><c>true</c> if the role was granted; <c>false</c> if already granted.</returns>
    public bool GrantRole(Guid roleId, string roleName)
    {
        if (!GrantInitialRole(roleId: roleId))
        {
            return false;
        }

        AddDomainEvent(new UserRoleGrantedEvent(UserId: Id, RoleId: roleId, RoleName: roleName));
        return true;
    }

    /// <summary>
    /// Grants a role as part of creating the account, raising no event: the visitor grant on
    /// signup is a same-transaction invariant, not a fact worth notifying the new user about.
    /// Idempotent: a role already granted reports <c>false</c>.
    /// </summary>
    /// <param name="roleId">The ID of the role to grant.</param>
    /// <returns><c>true</c> if the role was granted; <c>false</c> if already granted.</returns>
    public bool GrantInitialRole(Guid roleId)
    {
        if (HasRole(roleId: roleId))
        {
            return false;
        }

        UserRoles.Add(item: UserRoleEntity.Create(userId: Id, roleId: roleId));
        return true;
    }

    /// <summary>
    /// Revokes a role from this user and raises <see cref="UserRoleRevokedEvent" /> carrying the
    /// role name. Idempotent: a role not granted reports <c>false</c> and raises nothing.
    /// </summary>
    /// <param name="roleId">The ID of the role to revoke.</param>
    /// <param name="roleName">The revoked role's name, carried by the event.</param>
    /// <returns><c>true</c> if the role was revoked; <c>false</c> if it was not granted.</returns>
    public bool RevokeRole(Guid roleId, string roleName)
    {
        UserRoleEntity? userRole = UserRoles.FirstOrDefault(ur => ur.RoleId == roleId);
        if (userRole is null)
        {
            return false;
        }

        UserRoles.Remove(item: userRole);

        AddDomainEvent(new UserRoleRevokedEvent(UserId: Id, RoleId: roleId, RoleName: roleName));
        return true;
    }

    /// <summary>
    /// Checks if this user has a specific role.
    /// </summary>
    public bool HasRole(Guid roleId)
    {
        return UserRoles.Any(ur => ur.RoleId == roleId);
    }

    /// <summary>
    /// Sets the locale this user's messages render in. No-ops when unchanged.
    /// </summary>
    /// <param name="locale">The two-letter locale code.</param>
    /// <returns>True when the stored locale changed.</returns>
    public bool SetPreferredLocale(string locale)
    {
        if (string.IsNullOrWhiteSpace(locale) || PreferredLocale == locale)
        {
            return false;
        }

        PreferredLocale = locale;

        return true;
    }
}
