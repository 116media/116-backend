using _116.Shared.Contracts.Application.CQRS;

namespace _116.Auth.Application.Public.UseCases.Commands.SetPassword;

/// <summary>
/// Command for setting a password for a public user who authenticated via Google/Facebook.
/// </summary>
/// <param name="UserId">The unique identifier of the user setting their password.</param>
/// <param name="Password">The password to set for the user.</param>
/// <remarks>
/// This command allows authenticated users who signed up via external providers to set a password.
/// </remarks>
public record PublicSetPasswordCommand(
    Guid UserId,
    string Password
) : ICommand<PublicSetPasswordResult>;

/// <summary>
/// Result of the <see cref="PublicSetPasswordCommand"/> containing set status.
/// </summary>
/// <param name="IsSuccess">Indicates whether the password was set successfully.</param>
public record PublicSetPasswordResult(bool IsSuccess);
