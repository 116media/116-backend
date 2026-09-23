namespace _116.Identity.Application.Shared.Messages;

/// <summary>
/// The message templates this module sends. Owning the names here keeps the module off a
/// shared cross-module enum.
/// </summary>
public static class IdentityMessageTemplates
{
    /// <summary>
    /// The AccountForceLoggedOut template name, resolved from this module's resources.
    /// </summary>
    public const string AccountForceLoggedOut = "AccountForceLoggedOut";

    /// <summary>
    /// The EmailChangedAlertOld template name, resolved from this module's resources.
    /// </summary>
    public const string EmailChangedAlertOld = "EmailChangedAlertOld";

    /// <summary>
    /// The EmailChangedConfirmNew template name, resolved from this module's resources.
    /// </summary>
    public const string EmailChangedConfirmNew = "EmailChangedConfirmNew";

    /// <summary>
    /// The EmailVerificationOtp template name, resolved from this module's resources.
    /// </summary>
    public const string EmailVerificationOtp = "EmailVerificationOtp";

    /// <summary>
    /// The LocalPasswordAdded template name, resolved from this module's resources.
    /// </summary>
    public const string LocalPasswordAdded = "LocalPasswordAdded";

    /// <summary>
    /// The PasswordChanged template name, resolved from this module's resources.
    /// </summary>
    public const string PasswordChanged = "PasswordChanged";

    /// <summary>
    /// The PasswordResetCompleted template name, resolved from this module's resources.
    /// </summary>
    public const string PasswordResetCompleted = "PasswordResetCompleted";

    /// <summary>
    /// The PasswordResetOtp template name, resolved from this module's resources.
    /// </summary>
    public const string PasswordResetOtp = "PasswordResetOtp";

    /// <summary>
    /// The RefreshTokenReplayAlert template name, resolved from this module's resources.
    /// </summary>
    public const string RefreshTokenReplayAlert = "RefreshTokenReplayAlert";

    /// <summary>
    /// The RoleChanged template name, resolved from this module's resources.
    /// </summary>
    public const string RoleChanged = "RoleChanged";

    /// <summary>
    /// The SignedOutAllDevices template name, resolved from this module's resources.
    /// </summary>
    public const string SignedOutAllDevices = "SignedOutAllDevices";

    /// <summary>
    /// The Welcome template name, resolved from this module's resources.
    /// </summary>
    public const string Welcome = "Welcome";
}
