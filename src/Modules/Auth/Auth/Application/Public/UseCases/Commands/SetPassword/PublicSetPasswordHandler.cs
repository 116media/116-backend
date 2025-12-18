using _116.Shared.Application.Exceptions;
using _116.Auth.Application.Shared.Persistence;
using _116.Shared.Contracts.Application.CQRS;
using _116.Auth.Application.Shared.Repositories;
using _116.Auth.Application.Shared.Services;
using _116.Auth.Domain.Entities;

namespace _116.Auth.Application.Public.UseCases.Commands.SetPassword;

/// <summary>
/// Handles the <see cref="PublicSetPasswordCommand"/> to set a password for external auth users (Google/Facebook).
/// </summary>
/// <param name="userRepository">Repository for user data access operations.</param>
/// <param name="passwordService">Service for password hashing operations.</param>
/// <param name="unitOfWork">Unit of Work for managing database transactions.</param>
public class PublicSetPasswordHandler(
    IUserRepository userRepository,
    IPasswordService passwordService,
    IAuthUnitOfWork unitOfWork
) : ICommandHandler<PublicSetPasswordCommand, PublicSetPasswordResult>
{
    /// <summary>
    /// Handles the password set command for external auth users.
    /// </summary>
    /// <param name="command">The password set command containing user ID and new password.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>A <see cref="PublicSetPasswordResult"/> containing set status.</returns>
    /// <exception cref="NotFoundException">Thrown when no user is found with the specified ID.</exception>
    /// <exception cref="BadRequestException">Thrown when the account is not active.</exception>
    /// <exception cref="BadRequestException">Thrown when the user doesn't have an email address.</exception>
    /// <exception cref="BadRequestException">Thrown when user's auth provider is already Local.</exception>
    public async Task<PublicSetPasswordResult> Handle(
        PublicSetPasswordCommand command,
        CancellationToken cancellationToken
    )
    {
        UserEntity? user = await userRepository.FindUserByIdOrThrow(command.UserId, cancellationToken);

        // Validate user account status - accounts must be active
        userRepository.IsUserAccountActive(user!);

        // Hash the new password
        string hashedPassword = passwordService.Hash(command.Password);
        userRepository.SetPasswordForExternalUser(user!, hashedPassword);

        await unitOfWork.CommitAsync(cancellationToken);

        return new PublicSetPasswordResult(IsSuccess: true);
    }
}
