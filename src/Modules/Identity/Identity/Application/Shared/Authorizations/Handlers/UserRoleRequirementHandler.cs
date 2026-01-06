using System.Security.Claims;
using _116.Identity.Application.Shared.Authorizations.Requirements;
using Microsoft.AspNetCore.Authorization;

namespace _116.Identity.Application.Shared.Authorizations.Handlers;

/// <summary>
/// Authorization handler that validates user roles against policy requirements.
/// </summary>
/// <remarks>
/// Checks if the user's role claim matches any of the allowed roles in the requirement
/// using case-insensitive comparison. Used automatically by ASP.NET Core authorization system.
/// </remarks>
public class UserRoleRequirementHandler : AuthorizationHandler<UserRoleRequirement>
{
    /// <summary>
    /// Evaluates the user role requirement against the current authorization context.
    /// </summary>
    /// <param name="context">The authorization context containing user claims</param>
    /// <param name="requirement">The role requirement specifying allowed roles</param>
    /// <returns>A completed task representing the authorization evaluation</returns>
    protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, UserRoleRequirement requirement)
    {
        // Extract user's role from JWT token claims
        string? userRole = context.User.FindFirst(type: ClaimTypes.Role)?.Value;

        // Authorize if the user role matches any allowed role (case-insensitive)
        bool isUserRoleMatching = requirement.AllowedRoles.Contains(
            value: userRole,
            comparer: StringComparer.OrdinalIgnoreCase
        );
        if (!string.IsNullOrEmpty(value: userRole) && isUserRoleMatching)
        {
            context.Succeed(requirement: requirement);
        }

        return Task.CompletedTask;
    }
}
