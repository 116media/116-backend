using _116.Shared.Contracts.Application.CQRS;

namespace _116.Identity.Application.Auth.UseCases.Public.Commands.ChangePassword;

/// <summary>
/// Command for changing a user's password using their current password for verification.
/// </summary>
/// <param name="UserId">The unique identifier of the user changing their password.</param>
/// <param name="OldPassword">The user's current password for verification.</param>
/// <param name="NewPassword">The new password to set for the user.</param>
/// <remarks>
/// This command allows authenticated users to change their password by providing their current password.
/// The old password is verified before updating to the new password.
/// The new password must be different from the old password.
/// Users must be authenticated, active, and verified to change their password.
/// </remarks>
public record PublicChangePasswordCommand(
    Guid UserId,
    string OldPassword,
    string NewPassword
) : ICommand<PublicChangePasswordResult>;

/// <summary>
/// Result of the <see cref="PublicChangePasswordCommand" /> containing change status.
/// </summary>
/// <param name="IsSuccess">Indicates whether the password change was successful.</param>
/// <remarks>
/// Simple result indicating successful password change operation.
/// Upon success, the user can log in with their new password.
/// </remarks>
public record PublicChangePasswordResult(
    bool IsSuccess
);
