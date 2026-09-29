using System.Security.Claims;
using _116.Identity.Application.Shared.Authorizations.Requirements;
using Microsoft.AspNetCore.Authorization;

namespace _116.Identity.Application.Shared.Authorizations.Handlers;

/// <summary>
/// Authorization handler that validates user roles against policy requirements.
/// </summary>
/// <remarks>
/// Checks if any of the user's role claims matches any of the allowed roles in the
/// requirement using case-insensitive comparison, so authorization does not depend on
/// the order role claims were written into the token. Used automatically by the
/// ASP.NET Core authorization system.
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
        IEnumerable<string> userRoles = context
            .User.FindAll(type: ClaimTypes.Role)
            .Select(claim => claim.Value)
            .Where(role => !string.IsNullOrEmpty(value: role));

        bool isUserRoleMatching = userRoles.Any(role =>
            requirement.AllowedRoles.Contains(value: role, comparer: StringComparer.OrdinalIgnoreCase)
        );
        if (isUserRoleMatching)
        {
            context.Succeed(requirement: requirement);
        }

        return Task.CompletedTask;
    }
}
