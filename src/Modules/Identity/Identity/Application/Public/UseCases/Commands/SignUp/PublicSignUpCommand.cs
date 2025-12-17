using _116.Shared.Contracts.Application.CQRS;
using _116.Identity.Domain.Results;

namespace _116.Identity.Application.Public.UseCases.Commands.SignUp;

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
public record PublicSignUpCommand(
    string Email,
    string UserName,
    string Password
) : ICommand<PublicSignUpResult>;

/// <summary>
/// Result of the <see cref="PublicSignUpCommand"/> containing registration details.
/// </summary>
/// <param name="AuthenticationResult">The authentication result with user info and JWT token.</param>
/// <param name="VerificationRequired">Indicates if email verification is required before full access.</param>
/// <remarks>
/// Contains registration information and indicates next steps for the user.
/// If verification is required, the user should check their email for a verification link.
/// </remarks>
public record PublicSignUpResult(
    AuthenticationResult AuthenticationResult,
    bool VerificationRequired
);
