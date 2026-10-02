using _116.Identity.Domain.Constants;
using _116.Identity.Domain.Enums;
using _116.Identity.Domain.Events;
using _116.Identity.Domain.Exceptions;
using _116.Identity.Domain.StateMachines;
using _116.Identity.Domain.ValueObjects;

namespace _116.Identity.Domain.Entities;

/// <summary>
/// Credentials behaviour of <see cref="UserEntity" />. Its state lives in <c>Entities/UserEntity.cs</c>.
/// </summary>
public sealed partial class UserEntity
{
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
}
