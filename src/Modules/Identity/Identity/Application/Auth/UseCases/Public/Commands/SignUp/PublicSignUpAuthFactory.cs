using _116.Identity.Application.Auth.Repositories;
using _116.Identity.Application.Auth.Services;
using _116.Identity.Application.Auth.UseCases.Public.Commands.SignUp.Contracts;
using _116.Identity.Application.Shared.Persistence;
using _116.Identity.Application.Shared.Repositories;
using _116.Identity.Domain.Entities;
using _116.Identity.Domain.Enums;
using _116.Identity.Domain.ValueObjects;

namespace _116.Identity.Application.Auth.UseCases.Public.Commands.SignUp;

/// <summary>
/// Factory implementation for handling user registration logic in the signup flow.
/// </summary>
/// <param name="authRepository">Repository for user data access operations.</param>
/// <param name="otpRepository">Repository for OTP data access operations.</param>
/// <param name="passwordService">Service for hashing passwords.</param>
/// <param name="otpService">Service for generating OTP codes.</param>
/// <param name="unitOfWork">Unit of Work for managing database transactions.</param>
public class PublicSignUpAuthFactory(
    IAuthRepository authRepository,
    IOtpRepository otpRepository,
    IPasswordService passwordService,
    IOtpService otpService,
    IIdentityUnitOfWork unitOfWork
) : IPublicSignUpAuthFactory
{
    /// <summary>
    /// Registers a new user with email, username, and password.
    /// </summary>
    public async Task<PublicSignUpAuthData> RegisterAsync(
        string email,
        string userName,
        string password,
        CancellationToken cancellationToken
    )
    {
        await authRepository.ValidateUniqueCredentialsAsync(
            userName: userName,
            email: new Email(value: email),
            cancellationToken: cancellationToken
        );

        string hashedPassword = passwordService.Hash(password: password);

        var newUser = UserEntity.Create(
            Guid.NewGuid(),
            userName: userName,
            passwordHash: hashedPassword,
            email: new Email(value: email)
        );

        await authRepository.AddAsync(user: newUser, cancellationToken: cancellationToken);
        await authRepository.AssignVisitorRoleAsync(userId: newUser.Id, cancellationToken: cancellationToken);

        OtpEntity verificationOtp = otpService.CreateOtp(userId: newUser.Id, purpose: EnumOtpPurpose.EmailVerification);

        await otpRepository.AddAsync(otp: verificationOtp, cancellationToken: cancellationToken);
        await unitOfWork.CommitAsync(cancellationToken: cancellationToken);

        // Get the newly created user with roles to generate token
        UserEntity? userWithRoles = await authRepository.GetUserWithRolesAndPermissionsByCredentialsOrThrow(
            new Email(value: email),
            cancellationToken: cancellationToken
        );

        List<RolePermissionEntity> userPermissions = userWithRoles!
            .UserRoles.SelectMany(ur => ur.Role.RolePermissions)
            .ToList();

        return new PublicSignUpAuthData(User: userWithRoles, UserPermissions: userPermissions);
    }
}
