using _116.Auth.Application.Shared.Authorizations.Handlers;
using Microsoft.AspNetCore.Authorization;

namespace _116.Auth.Application.Shared.Authorizations.Requirements;

/// <summary>
/// Authorization requirement that validates user roles against a list of allowed roles.
/// </summary>
/// <remarks>
/// This requirement is used by the authorization system to determine if a user's role
/// matches any of the specified allowed roles. It works in conjunction with
/// <see cref="UserRoleRequirementHandler"/> to perform the actual authorization logic.
/// The requirement supports multiple allowed roles, meaning users with any of the specified
/// roles will be authorized to access the protected resource.
/// </remarks>
public class UserRoleRequirement(params string[] allowedRoles) : IAuthorizationRequirement
{
    /// <summary>
    /// Gets the array of roles that are allowed to access the protected resource.
    /// </summary>
    /// <remarks>
    /// Contains the role names that will be compared against the user's role claim.
    /// The comparison is performed case-insensitively by the authorization handler.
    /// At least one role must be specified during construction.
    /// </remarks>
    public string[] AllowedRoles { get; } = allowedRoles ?? throw new ArgumentNullException(nameof(allowedRoles));
}
