using _116.Shared.Application.Exceptions;
using _116.Shared.Application.Persistence;
using _116.Shared.Contracts.Application.CQRS;
using _116.Auth.Application.Shared.Mappers;
using _116.Auth.Application.Shared.Repositories;
using _116.Auth.Application.Shared.Services;
using _116.Auth.Domain.Entities;
using _116.Auth.Domain.Results;
using _116.Auth.Domain.ValueObjects;
using OtpPurpose = _116.Auth.Domain.Enums.OtpPurpose;

namespace _116.Auth.Application.Public.UseCases.Commands.SignUp;

/// <summary>
/// Handles the <see cref="PublicSignUpCommand"/> to register new public users.
/// </summary>
/// <param name="userRepository">Repository for user data access operations.</param>
/// <param name="roleRepository">Repository for role and permission data operations.</param>
/// <param name="otpRepository">Repository for OTP data access operations.</param>
/// <param name="passwordService">Service for hashing passwords.</param>
/// <param name="otpService">Service for generating OTP codes.</param>
/// <param name="jwtService">Service for generating JWT tokens with user claims.</param>
/// <param name="unitOfWork">Unit of Work for managing database transactions.</param>
public class PublicSignUpHandler(
    IUserRepository userRepository,
    IRoleRepository roleRepository,
    IOtpRepository otpRepository,
    IPasswordService passwordService,
    IOtpService otpService,
    IJwtService jwtService,
    IUnitOfWork unitOfWork
) : ICommandHandler<PublicSignUpCommand, PublicSignUpResult>
{
    /// <summary>
    /// Handles the public sign-up command by creating a new user account.
    /// </summary>
    /// <param name="command">The public sign-up command containing user registration data.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>A <see cref="PublicSignUpResult"/> containing user authentication information.</returns>
    /// <exception cref="ConflictException">Thrown when email or username already exists.</exception>
    public async Task<PublicSignUpResult> Handle(PublicSignUpCommand command, CancellationToken cancellationToken)
    {
        // Normalize email using value object
        var email = new Email(command.Email);

        // Validate unique credentials (email and username) in single repository call
        await userRepository.ValidateUniqueCredentialsAsync(email, command.UserName, cancellationToken);

        // Hash the password
        string hashedPassword = passwordService.Hash(command.Password);

        // Create new user entity
        var newUser = UserEntity.Create(
            id: Guid.NewGuid(),
            email: email.Value,
            userName: command.UserName,
            passwordHash: hashedPassword
        );

        // Add user to repository
        await userRepository.AddAsync(newUser, cancellationToken);

        // Assign the visitor role to the new user
        await userRepository.AssignVisitorRoleAsync(newUser.Id, cancellationToken);

        // Generate OTP for email verification
        OtpEntity verificationOtp = otpService.CreateOtp(newUser.Id, OtpPurpose.EmailVerification);
        await otpRepository.AddAsync(verificationOtp, cancellationToken);

        // Save all changes atomically in a single transaction
        // (user creation, visitor role assignment, and OTP generation) succeed or fail together
        await unitOfWork.CommitAsync(cancellationToken);

        // Get the newly created user with roles to generate token
        UserEntity? userWithRoles = await userRepository.GetUserWithRolesAndPermissionsByCredentialsOrThrow(
            email.Value,
            cancellationToken
        );

        // Extract user permissions from roles (already loaded by repository)
        List<RolePermissionEntity> userPermissions = userWithRoles!.UserRoles
            .SelectMany(ur => ur.Role.RolePermissions)
            .ToList();

        // Generate JWT token with user claims
        JwtGenerationResult token = jwtService.GenerateToken(
            userId: userWithRoles.Id,
            email: email.Value,
            userName: userWithRoles.UserName,
            userRoles: userWithRoles.UserRoles,
            userPermissions: userPermissions,
            isVerified: userWithRoles.IsVerified,
            isActive: userWithRoles.IsActive,
            isLoggedIn: userWithRoles.IsLoggedIn,
            authProvider: userWithRoles.AuthProvider
        );

        // Extract roles and permissions using repository
        var (roles, permissions) = roleRepository.GetUserRolesAndPermissions(userWithRoles.UserRoles);

        // Map to userDTO
        var userDto = userWithRoles.ToUserResponseDto(roles, permissions);
        var authResult = new AuthenticationResult(userDto, token.Token, token.ExpiresAt);

        // Return result with verification required flag (new users need email verification)
        return new PublicSignUpResult(authResult, VerificationRequired: true);
    }
}
