using _116.Shared.Contracts.Application.CQRS;

namespace _116.Identity.Application.Auth.UseCases.Admin.Commands.ChangePassword;

/// <summary>
/// Command for changing an admin user's password using their current password for verification.
/// </summary>
/// <param name="UserId">The unique identifier of the admin user changing their password.</param>
/// <param name="OldPassword">The admin user's current password for verification.</param>
/// <param name="NewPassword">The new password to set for the admin user.</param>
/// <remarks>
/// This command allows authenticated admin users to change their password by providing their current password.
/// The old password is verified before updating to the new password.
/// The new password must be different from the old password.
/// Admin users must be authenticated and active to change their password.
/// </remarks>
public record AdminChangePasswordCommand(Guid UserId, string OldPassword, string NewPassword)
    : ICommand<AdminChangePasswordResult>;

/// <summary>
/// Result of the <see cref="AdminChangePasswordCommand" /> containing change status.
/// </summary>
/// <param name="IsSuccess">Indicates whether the password change was successful.</param>
/// <remarks>
/// Simple result indicating successful password change operation.
/// Upon success, the admin user can log in with their new password.
/// </remarks>
public record AdminChangePasswordResult(bool IsSuccess);
