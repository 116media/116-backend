namespace _116.Identity.Domain.Enums;

/// <summary>
/// Identifies which flow replaced a user's password hash. The origin selects
/// the security email sent for the change.
/// </summary>
public enum EnumPasswordChangeOrigin
{
    /// <summary>
    /// The user changed their password knowing the current one.
    /// </summary>
    Changed,

    /// <summary>
    /// The password was replaced through the OTP-driven reset flow.
    /// </summary>
    Reset,

    /// <summary>
    /// A social-login account gained a local password for the first time.
    /// </summary>
    SetLocal,
}
