using Microsoft.AspNetCore.Authorization;

namespace _116.Identity.Application.Shared.Authorizations.Requirements;

/// <summary>
/// Authorization requirement that validates user account status against claim requirements.
/// </summary>
/// <remarks>
/// This requirement is used to enforce account status checks such as verification,
/// active status, or login status. It works with the authorization handler to validate
/// specific claim types and values in the user's JWT token.
/// </remarks>
public class AccountStatusRequirement(string claimType, string claimValue) : IAuthorizationRequirement
{
    /// <summary>
    /// Gets the claim type that must be present in the user's token.
    /// </summary>
    /// <remarks>
    /// Specifies which claim to look for in the JWT token
    /// (e.g., "is_verified", "is_active", "is_logged_in").
    /// </remarks>
    public string ClaimType { get; } = claimType ?? throw new ArgumentNullException(nameof(claimType));
    /// <summary>
    /// Gets the required value for the claim.
    /// </summary>
    /// <remarks>
    /// Specifies the expected value of the claim for authorization to succeed (typically "true").
    /// </remarks>
    public string ClaimValue { get; } = claimValue ?? throw new ArgumentNullException(nameof(claimValue));
}
