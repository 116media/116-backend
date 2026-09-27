namespace _116.Identity.Domain.StateMachines;

/// <summary>
/// Stable identifiers for the identity domain rules reported through
/// <see cref="Exceptions.IdentityRuleException" />, scoped <c>identity.&lt;entity&gt;.&lt;rule&gt;</c>.
/// </summary>
public static class IdentityRuleCodes
{
    /// <summary>
    /// An email address failed the domain format guard. Args: [0] the rejected address.
    /// </summary>
    public const string InvalidEmailFormat = "identity.user.invalid-email-format";

    /// <summary>
    /// A username failed the domain format guard. Args: [0] the rejected username.
    /// </summary>
    public const string InvalidUsernameFormat = "identity.user.invalid-username-format";

    /// <summary>
    /// A password failed the domain format guard. Args: none.
    /// </summary>
    public const string InvalidPasswordFormat = "identity.user.invalid-password-format";

    /// <summary>
    /// The operation is not valid for the account's auth provider. Args: none.
    /// </summary>
    public const string ProviderMismatch = "identity.user.provider-mismatch";

    /// <summary>
    /// A password cannot be set on an account without an email. Args: none.
    /// </summary>
    public const string EmailRequiredToSetPassword = "identity.user.email-required-to-set-password";

    /// <summary>
    /// The account is deactivated and cannot log in. Args: [0] account email.
    /// </summary>
    public const string AccountInactive = "identity.user.account-inactive";

    /// <summary>
    /// The account is not verified and cannot log in. Args: [0] account email.
    /// </summary>
    public const string AccountNotVerified = "identity.user.account-not-verified";

    /// <summary>
    /// The role is already assigned to the user. Args: none.
    /// </summary>
    public const string RoleAlreadyAssignedToUser = "identity.user.role-already-assigned";

    /// <summary>
    /// A required role name was blank. Args: none.
    /// </summary>
    public const string RoleNameRequired = "identity.role.name-required";

    /// <summary>
    /// A required role description was blank. Args: none.
    /// </summary>
    public const string RoleDescriptionRequired = "identity.role.description-required";

    /// <summary>
    /// A required permission resource was blank. Args: none.
    /// </summary>
    public const string PermissionResourceRequired = "identity.permission.resource-required";

    /// <summary>
    /// A required permission action was blank. Args: none.
    /// </summary>
    public const string PermissionActionRequired = "identity.permission.action-required";

    /// <summary>
    /// A required permission description was blank. Args: none.
    /// </summary>
    public const string PermissionDescriptionRequired = "identity.permission.description-required";

    /// <summary>
    /// An email value failed the value object's format guard. Args: [0] the rejected value.
    /// </summary>
    public const string InvalidEmail = "identity.email.invalid-format";

    /// <summary>
    /// An OTP purpose value is not a known purpose. Args: [0] the rejected value.
    /// </summary>
    public const string InvalidOtpPurpose = "identity.otp-purpose.invalid";

    /// <summary>
    /// An auth provider value is not a known provider. Args: [0] the rejected value.
    /// </summary>
    public const string InvalidAuthProvider = "identity.auth-provider.invalid";

    /// <summary>
    /// A session status value is not a known status. Args: [0] the rejected value.
    /// </summary>
    public const string InvalidSessionStatus = "identity.session-status.invalid";

    /// <summary>
    /// A client platform value is not a known platform. Args: [0] the rejected value.
    /// </summary>
    public const string InvalidClientPlatform = "identity.client.invalid-platform";

    /// <summary>
    /// An export format value is not a known format. Args: [0] the rejected value.
    /// </summary>
    public const string InvalidExportFormat = "identity.export-format.invalid";
}
