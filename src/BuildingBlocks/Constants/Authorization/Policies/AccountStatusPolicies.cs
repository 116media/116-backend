namespace _116.BuildingBlocks.Constants.Authorization.Policies;

/// <summary>
/// Defines authorization policy names for accounts status-based access control.
/// </summary>
/// <remarks>
/// Contains string constants for authorization policies that restrict access based on user account status.
/// These policies are used with [Authorize (Policy = "PolicyName")] attributes to enforce account requirements.
/// </remarks>
public static class AccountStatusPolicies
{
    /// <summary>
    /// Policy that requires user account to be verified.
    /// </summary>
    /// <remarks>
    /// Use the policy for resources that requires email/phone verification,
    /// such as sensitive account operations or premium features.
    /// </remarks>
    public const string RequireVerifiedUser = "RequireVerifiedUser";

    /// <summary>
    /// Policy that requires user account to be active (not suspended/banned).
    /// </summary>
    /// <remarks>
    /// Use the policy for resources that should be blocked for deactivated accounts,
    /// such as posting or reacting to contents or accessing user features.
    /// </remarks>
    public const string RequireActiveUser = "RequireActiveUser";
}
