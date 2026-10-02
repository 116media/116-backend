using System.ComponentModel.DataAnnotations;
using _116.Identity.Domain.Constants;
using _116.Identity.Domain.Enums;
using _116.Identity.Domain.Exceptions;
using _116.Identity.Domain.StateMachines;
using _116.Identity.Domain.ValueObjects;
using _116.Shared.Domain;
using _116.Shared.Domain.Constants;

namespace _116.Identity.Domain.Entities;

/// <summary>
/// Represents a user account with authentication credentials and profile information.
/// This is the main entity for user management - handles both login credentials and profile data.
/// </summary>
public partial class UserEntity : Aggregate<Guid>
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
    public string PreferredLocale { get; private set; } = LocaleConstants.DefaultLocale;

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
}
