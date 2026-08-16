using _116.Identity.Application.Shared.DTOs;
using _116.Shared.Contracts.Application.CQRS;

namespace _116.Identity.Application.Auth.UseCases.Public.Commands.SignUp;

/// <summary>
/// Command for public user registration with local authentication provider.
/// </summary>
/// <param name="Email">The user's email address for account verification.</param>
/// <param name="UserName">The desired username (alphanumeric with spaces and hyphens allowed).</param>
/// <param name="Password">The user's password in plain text format (will be hashed).</param>
/// <remarks>
/// This command is for local user registration where users provide their own credentials.
/// The system will hash the password and create an unverified account that requires email confirmation.
/// </remarks>
public record PublicSignUpCommand(string Email, string UserName, string Password) : ICommand<PublicSignUpResult>;

/// <summary>
/// Result of the <see cref="PublicSignUpCommand" /> containing registration details, deliberately
/// carrying no tokens.
/// </summary>
/// <param name="User">The created user information.</param>
/// <param name="VerificationRequired">Indicates that email verification must happen before login.</param>
public record PublicSignUpResult(UserResponseDto User, bool VerificationRequired);
