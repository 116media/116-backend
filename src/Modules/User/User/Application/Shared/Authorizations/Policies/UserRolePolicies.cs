namespace _116.User.Application.Shared.Authorizations.Policies;

/// <summary>
/// Defines authorization policy names for user role-based access control.
/// </summary>
/// <remarks>
/// Contains string constants for authorization policies that restrict access based on user roles.
/// These policies are registered in the UserModule and used with [Authorize (Policy = "PolicyName")] attributes.
/// </remarks>
public class UserRolePolicies
{
    /// <summary>
    /// Policy that requires SuperAdmin role access.
    /// </summary>
    /// <remarks>
    /// Use this policy for endpoints that should only be accessible to SuperAdmin users,
    /// such as system configuration, user role management, and critical administrative functions.
    /// </remarks>
    public const string RequireSuperAdminOnly = "RequireSuperAdminOnly";

    /// <summary>
    /// Policy that requires Admin role access (Admin or SuperAdmin).
    /// </summary>
    /// <remarks>
    /// Use this policy for endpoints that should be accessible to both Admin and SuperAdmin users,
    /// such as user management, content moderation, and general administrative functions.
    /// </remarks>
    public const string RequireAdminOnly = "RequireAdminOnly";

    /// <summary>
    /// Policy that requires Admin role access (Admin or SuperAdmin).
    /// </summary>
    /// <remarks>
    /// Use this policy for endpoints that should be accessible to both Admin and SuperAdmin users,
    /// such as user management, content moderation, and general administrative functions.
    /// </remarks>
    public const string RequireVisitorOnly = "RequireVisitorOnly";
}
