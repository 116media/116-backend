namespace _116.BuildingBlocks.Constants;

/// <summary>
/// Contains constants related to user entity business rules and constraints.
/// </summary>
public static class UserConstants
{
    /// <summary>
    /// Minimum allowed length for the username.
    /// </summary>
    public const int MinUserNameLength = 3;

    /// <summary>
    /// Maximum allowed length for username.
    /// </summary>
    public const int MaxUserNameLength = 20;

    /// <summary>
    /// Minimum allowed length for the password.
    /// </summary>
    public const int MinPasswordLength = 6;

    /// <summary>
    /// Default verification status for new local authentication users.
    /// </summary>
    public const bool DefaultIsVerified = false;

    /// <summary>
    /// Default active status for new users.
    /// </summary>
    public const bool DefaultIsActive = true;

    /// <summary>
    /// Verification status for external authentication users (pre-verified).
    /// </summary>
    public const bool ExternalAuthIsVerified = true;

    /// <summary>
    /// Active status when the user account is activated.
    /// </summary>
    public const bool ActivatedStatus = true;

    /// <summary>
    /// Active status when the user account is deactivated.
    /// </summary>
    public const bool DeactivatedStatus = false;

    /// <summary>
    /// Verification status after email is updated (requires re-verification).
    /// </summary>
    public const bool EmailUpdatedVerificationStatus = false;

    // Database field length constraints

    /// <summary>
    /// Maximum allowed length for email addresses (RFC 5321 standard).
    /// </summary>
    public const int MaxEmailLength = 254;

    /// <summary>
    /// Maximum allowed length for a provider subject id (Google sub, Facebook user id).
    /// </summary>
    public const int MaxProviderSubjectIdLength = 255;

    /// <summary>
    /// Maximum allowed length for country names.
    /// </summary>
    public const int MaxCountryNameLength = 100;

    /// <summary>
    /// Maximum allowed length for country ISO codes.
    /// </summary>
    public const int MaxCountryIsoCodeLength = 3;

    /// <summary>
    /// Maximum allowed length for country dial codes.
    /// </summary>
    public const int MaxCountryDialCodeLength = 10;

    /// <summary>
    /// Maximum allowed length for partial phone numbers.
    /// </summary>
    public const int MaxPartialPhoneNumberLength = 50;

    /// <summary>
    /// Maximum allowed length for full phone numbers.
    /// </summary>
    public const int MaxFullPhoneNumberLength = 20;

    /// <summary>
    /// OTP expiration time in minutes.
    /// </summary>
    public const int OtpExpirationMinutes = 60;

    /// <summary>
    /// Maximum number of OTP verification attempts against a single code.
    /// </summary>
    public const int MaxOtpAttempts = 3;

    /// <summary>
    /// Maximum failed OTP attempts an account tolerates before its OTP flows lock. Unlike
    /// <see cref="MaxOtpAttempts" /> this counter survives a resend.
    /// </summary>
    public const int MaxAccountOtpAttempts = 5;

    /// <summary>
    /// How long OTP verification stays locked once the account attempt cap is reached.
    /// </summary>
    public const int OtpLockoutMinutes = 15;

    /// <summary>
    /// Maximum OTP codes an account may request per purpose inside the resend window.
    /// </summary>
    public const int MaxOtpResendsPerWindow = 3;

    /// <summary>
    /// The window over which OTP resends are counted.
    /// </summary>
    public const int OtpResendWindowMinutes = 15;

    /// <summary>
    /// Maximum consecutive failed logins an account tolerates before it locks.
    /// </summary>
    public const int MaxLoginAttempts = 5;

    /// <summary>
    /// How long login stays locked once the attempt cap is reached.
    /// </summary>
    public const int LoginLockoutMinutes = 15;

    /// <summary>
    /// Length of generated OTP codes.
    /// </summary>
    public const int OtpCodeLength = 6;

    /// <summary>
    /// Maximum allowed length for the stored OTP code hash.
    /// The current PBKDF2 format occupies 67 characters; the surplus leaves room for a future
    /// hash version, mirroring the allowance made for the user password hash column.
    /// </summary>
    public const int OtpCodeHashLength = 100;
}
