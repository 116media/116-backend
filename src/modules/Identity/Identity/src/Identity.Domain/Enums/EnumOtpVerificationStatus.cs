namespace _116.Identity.Domain.Enums;

/// <summary>
/// Status of verifying a presented code against an outstanding OTP.
/// </summary>
public enum EnumOtpVerificationStatus
{
    /// <summary>
    /// The code matches an outstanding, unexpired OTP with attempts remaining.
    /// </summary>
    Valid,

    /// <summary>
    /// The OTP's lifetime has passed; the code is not judged.
    /// </summary>
    Expired,

    /// <summary>
    /// The attempt cap is spent, either before this attempt or by it.
    /// </summary>
    AttemptsExhausted,

    /// <summary>
    /// The code does not match; the failed attempt has been counted.
    /// </summary>
    Mismatch,
}
