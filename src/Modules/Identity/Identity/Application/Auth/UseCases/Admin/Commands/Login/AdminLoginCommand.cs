using _116.Identity.Domain.Results;
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
public record AdminLoginCommand(string Email, string Password) : ICommand<AdminLoginResult>;

/// <summary>
/// Result of the <see cref="AdminLoginCommand" /> containing admin authentication details.
/// </summary>
/// <param name="AuthenticationResult">The authentication result with admin user info and JWT token.</param>
/// <remarks>
/// Contains admin-specific authentication information including elevated permissions.
/// </remarks>
public record AdminLoginResult(AuthenticationResult AuthenticationResult);
