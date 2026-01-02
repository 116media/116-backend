namespace _116.Identity.Application.Shared.Authorizations.Configuration;

/// <summary>
/// Represents the complete authorization configuration for the Auth module.
/// </summary>
/// <remarks>
/// This record encapsulates all authorization policy configurations in a structured format,
/// promoting type safety and immutability in the configuration system.
/// </remarks>
public sealed record AuthorizationConfiguration
{
    /// <summary>
    /// Gets or sets the account status policy configurations.
    /// </summary>
    /// <remarks>
    /// Contains mappings between policy names and their claim type/value requirements.
    /// </remarks>
    public required Dictionary<string, (string ClaimType, string ClaimValue)> AccountStatusPolicies { get; init; }

    /// <summary>
    /// Gets or sets the user role policy configurations.
    /// </summary>
    /// <remarks>
    /// Contains mappings between policy names and their allowed role arrays.
    /// </remarks>
    public required Dictionary<string, string[]> UserRolePolicies { get; init; }
}
