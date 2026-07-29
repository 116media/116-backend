using _116.Identity.Application.Auth.Services;
using _116.Identity.Application.Auth.UseCases.Public.Commands.ResetPassword.Contracts;
using _116.Identity.Application.Shared.Errors;
using _116.Identity.Application.Shared.Persistence;
using _116.Identity.Application.Shared.Repositories;
using _116.Identity.Domain.Entities;
using _116.Identity.Domain.ValueObjects;
using _116.Mailer.Contracts.Application;

namespace _116.Identity.Application.Auth.UseCases.Public.Commands.ResetPassword;

/// <summary>
/// Factory implementation for handling user password reset logic.
/// </summary>
/// <param name="authRepository">Repository for user data access operations.</param>
/// <param name="passwordService">Service for password hashing operations.</param>
/// <param name="unitOfWork">Unit of Work for managing database transactions.</param>
/// <param name="userErrors">User domain error factory for generating domain exceptions.</param>
public class PublicResetPasswordAuthFactory(
    IAuthRepository authRepository,
    IPasswordService passwordService,
    IIdentityUnitOfWork unitOfWork,
    UserErrors userErrors,
    IMailer mailer
) : IPublicResetPasswordAuthFactory
{
    /// <summary>
    /// Gets and validates user by email for password reset.
    /// </summary>
    public async Task<PublicResetPasswordAuthData> GetUserForResetAsync(
        string email,
        CancellationToken cancellationToken
    )
    {
        var emailValue = new Email(value: email);
        UserEntity? user = await authRepository.GetUserWithRolesByEmailOrThrow(
            email: emailValue,
            cancellationToken: cancellationToken
        );

        authRepository.IsUserAccountActive(user!);
        authRepository.IsUserAccountVerified(user!);

        return new PublicResetPasswordAuthData(User: user!);
    }

    /// <summary>
    /// Resets a user's password with a new password.
    /// </summary>
    public async Task<UserEntity> ResetPasswordAsync(
        UserEntity user,
        string newPassword,
        CancellationToken cancellationToken
    )
    {
        if (passwordService.Verify(password: newPassword, hash: user.PasswordHash))
        {
            throw userErrors.NewPasswordSameAsOld();
        }

        string hashedPassword = passwordService.Hash(password: newPassword);
        user.UpdatePassword(newPasswordHash: hashedPassword, errors: userErrors);

        await unitOfWork.CommitAsync(cancellationToken: cancellationToken);

        if (user.Email is not null)
        {
            await mailer.EnqueueAsync(
                template: EnumEmailTemplate.PasswordResetCompleted,
                to: new EmailRecipient(Address: user.Email, DisplayName: user.UserName),
                tokens: new Dictionary<string, string>
                {
                    ["userName"] = user.UserName,
                    ["resetTime"] = DateTime.UtcNow.ToString("u"),
                },
                culture: EmailCulture.Current(),
                cancellationToken: cancellationToken
            );
        }

        return user;
    }
}
