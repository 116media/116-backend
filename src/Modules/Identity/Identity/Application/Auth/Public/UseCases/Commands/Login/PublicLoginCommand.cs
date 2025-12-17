using _116.Identity.Domain.Results;
using _116.Shared.Contracts.Application.CQRS;

namespace _116.Identity.Application.Auth.Public.UseCases.Commands.Login;

/// <summary>
/// Command used to authenticate a public user.
/// </summary>
/// <param name="Credentials">
/// The unique identifier for the user. It can be an email address or a username.
/// </param>
/// <param name="Password">The user's password in plain text format.</param>
/// <remarks>
/// This command is tailored for public user login scenarios.
/// The system validates the provided credentials and returns an authentication result if successful.
/// </remarks>
public record PublicLoginCommand(
    string Credentials,
    string Password
) : ICommand<PublicLoginResult>;
/// <summary>
/// The result of executing a <see cref="PublicLoginCommand"/>.
/// </summary>
/// <param name="AuthenticationResult">The authentication result with user info and JWT token.</param>
/// <remarks>
/// Provides authentication information relevant to public users.
/// </remarks>
public record PublicLoginResult(AuthenticationResult AuthenticationResult);
