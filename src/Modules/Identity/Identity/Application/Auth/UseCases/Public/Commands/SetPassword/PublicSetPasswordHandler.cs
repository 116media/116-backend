using _116.Identity.Application.Auth.Services;
using _116.Identity.Application.Shared.Persistence;
using _116.Identity.Application.Shared.Repositories;
using _116.Identity.Domain.Entities;
using _116.Shared.Application.Exceptions;
using _116.Shared.Contracts.Application.CQRS;

namespace _116.Identity.Application.Auth.UseCases.Public.Commands.SetPassword;

/// <summary>
/// Handles the <see cref="PublicSetPasswordCommand" /> to set a password for external auth users (Google/Facebook).
/// </summary>
/// <param name="authRepository">Repository for user data access operations.</param>
/// <param name="passwordService">Service for password hashing operations.</param>
/// <param name="unitOfWork">Unit of Work for managing database transactions.</param>
public class PublicSetPasswordHandler(
    IAuthRepository authRepository,
    IPasswordService passwordService,
    IIdentityUnitOfWork unitOfWork
) : ICommandHandler<PublicSetPasswordCommand, PublicSetPasswordResult>
{
    /// <summary>
    /// Handles the password set command for external auth users.
    /// </summary>
    /// <param name="command">The password set command containing user ID and new password.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>A <see cref="PublicSetPasswordResult" /> containing set status.</returns>
    /// <exception cref="NotFoundException">Thrown when no user is found with the specified ID.</exception>
    /// <exception cref="BadRequestException">Thrown when the account is not active.</exception>
    /// <exception cref="BadRequestException">Thrown when the user doesn't have an email address.</exception>
    /// <exception cref="BadRequestException">Thrown when user's auth provider is already Local.</exception>
    public async Task<PublicSetPasswordResult> Handle(
        PublicSetPasswordCommand command,
        CancellationToken cancellationToken
    )
    {
        UserEntity? user =
            await authRepository.FindUserByIdOrThrow(userId: command.UserId, cancellationToken: cancellationToken);
        // Validate user account status - accounts must be active
        authRepository.IsUserAccountActive(user!);
        // Hash the new password
        string hashedPassword = passwordService.Hash(password: command.Password);
        authRepository.SetPasswordForExternalUser(user!, hashedPassword: hashedPassword);
        await unitOfWork.CommitAsync(cancellationToken: cancellationToken);
        return new PublicSetPasswordResult(true);
    }
}
