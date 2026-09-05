using _116.BuildingBlocks.Constants.RateLimit;
using _116.Identity.Application.Shared.DTOs;
using _116.Shared.Contracts.Application.CQRS;

namespace _116.Identity.Application.Auth.UseCases.Admin.Commands.Login;

/// <summary>
/// Command for admin user authentication.
/// </summary>
/// <param name="Email">The admin's email address.</param>
/// <param name="Password">The admin's password in plain text format.</param>
/// <remarks>
/// This command is specifically for administrative users requiring elevated privileges.
/// The authentication process validates admin role requirements.
/// </remarks>
public record AdminLoginCommand(string Email, string Password) : ICommand<AdminLoginResult>, IAccountRateLimited
{
    /// <inheritdoc />
    public string RateLimitPolicy => RateLimitPolicies.Authentication;

    /// <inheritdoc />
    public string AccountKey => Email;
}

/// <summary>
/// Result of the <see cref="AdminLoginCommand" /> containing admin authentication details.
/// </summary>
/// <param name="Authentication">The authenticated user with admin user info and JWT token.</param>
/// <remarks>
/// Contains admin-specific authentication information including elevated permissions.
/// </remarks>
public record AdminLoginResult(AuthenticationDto Authentication);
