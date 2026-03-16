using System.Security.Claims;
using _116.BuildingBlocks.Constants;
using _116.Identity.Application.Auth.Specifications;
using _116.Identity.Application.Roles.Specifications;
using _116.Identity.Application.Shared.Errors;
using _116.Identity.Application.Shared.Repositories;
using _116.Identity.Contracts.Application;
using _116.Identity.Domain.Entities;
using _116.Identity.Domain.Enums;
using _116.Identity.Domain.ValueObjects;
using _116.Identity.Infrastructure.Persistence;
using _116.Shared.Application.Exceptions;
using _116.Shared.Infrastructure.Extensions;
using Microsoft.EntityFrameworkCore;

namespace _116.Identity.Infrastructure.Repositories;

/// <summary>
/// Implementation of <see cref="IAuthRepository" /> using Entity Framework Core.
/// </summary>
public class AuthRepository(IdentityDbContext context) : IAuthRepository
{
    /// <inheritdoc />
    public async Task<UserEntity?> FindUserByIdOrThrow(Guid userId, CancellationToken cancellationToken = default)
    {
        return await context.Users.FindOrThrowAsync([userId], cancellationToken: cancellationToken);
    }

    /// <inheritdoc />
    public async Task<UserEntity?> GetUserWithRolesByEmailOrThrow(
        Email email,
        CancellationToken cancellationToken = default
    )
    {
        var specification = new UserByEmailSpecification(email: email.Value);
        return await context
            .Users.ApplySpecification(specification: specification)
            .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
            .FirstDefaultOrThrowAsync(
                keyName: "credentials",
                keyValue: email.Value,
                cancellationToken: cancellationToken
            );
    }

    /// <inheritdoc />
    public async Task<UserEntity?> GetUserWithRolesAndPermissionsByIdOrThrow(
        Guid userId,
        CancellationToken cancellationToken = default
    )
    {
        var specification = new UserByIdSpecification(userId: userId);
        return await context
            .Users.ApplySpecification(specification: specification)
            .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
                    .ThenInclude(r => r.RolePermissions)
                        .ThenInclude(rp => rp.Permission)
            .AsSplitQuery()
            .FirstDefaultOrThrowAsync(keyValue: userId, cancellationToken: cancellationToken);
    }

    /// <inheritdoc />
    public async Task<UserEntity?> GetUserWithSessionsByIdOrThrow(
        Guid userId,
        CancellationToken cancellationToken = default
    )
    {
        var specification = new UserByIdSpecification(userId: userId);
        return await context
            .Users.ApplySpecification(specification: specification)
            .Include(u => u.Sessions)
            .FirstDefaultOrThrowAsync(keyValue: userId, cancellationToken: cancellationToken);
    }

    /// <inheritdoc />
    public async Task<UserEntity?> GetUserWithRolesAndPermissionsByEmailOrThrow(
        Email email,
        CancellationToken cancellationToken = default
    )
    {
        var specification = new UserByEmailSpecification(email: email.Value);
        return await context
            .Users.ApplySpecification(specification: specification)
            .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
                    .ThenInclude(r => r.RolePermissions)
                        .ThenInclude(rp => rp.Permission)
            .AsSplitQuery()
            .FirstDefaultOrThrowAsync(keyName: "email", keyValue: email.Value, cancellationToken: cancellationToken);
    }

    /// <inheritdoc />
    public async Task<UserEntity?> GetUserWithRolesAndPermissionsByCredentialsOrThrow(
        string credentials,
        CancellationToken cancellationToken = default
    )
    {
        // Use specification to determine if credentials is an email or username
        var specification = new UserByCredentialsSpecification(credentials: credentials);
        // Get the user by email or username without any status checks
        return await context
            .Users.ApplySpecification(specification: specification)
            .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
                    .ThenInclude(r => r.RolePermissions)
                        .ThenInclude(rp => rp.Permission)
            .AsSplitQuery()
            .FirstDefaultOrThrowAsync(
                keyName: "credentials",
                keyValue: credentials,
                cancellationToken: cancellationToken
            );
    }

    /// <inheritdoc />
    public async Task<bool> ExistsByEmailAsync(Email email, CancellationToken cancellationToken = default)
    {
        var specification = new UserByEmailSpecification(email: email.Value);

        return await context.Users.AnyBySpecificationAsync(
            specification: specification,
            cancellationToken: cancellationToken
        );
    }

    /// <inheritdoc />
    public async Task<bool> ExistsByUserNameAsync(string userName, CancellationToken cancellationToken = default)
    {
        var specification = new UserByUserNameSpecification(userName: userName);

        return await context.Users.AnyBySpecificationAsync(
            specification: specification,
            cancellationToken: cancellationToken
        );
    }

    /// <inheritdoc />
    public async Task<UserEntity?> GetUserByPhoneNumberAsync(
        string phoneNumber,
        CancellationToken cancellationToken = default
    )
    {
        var specification = new UserByPhoneNumberSpecification(phoneNumber: phoneNumber);

        return await context.Users.FirstOrDefaultBySpecificationAsync(
            specification: specification,
            cancellationToken: cancellationToken
        );
    }

    /// <inheritdoc />
    public async Task AddAsync(UserEntity user, CancellationToken cancellationToken = default)
    {
        await context.Users.AddAsync(entity: user, cancellationToken: cancellationToken);
    }

    /// <inheritdoc />
    public bool IsUserAccountActive(UserEntity user)
    {
        if (!user.IsActive)
        {
            throw UserErrors.AccountInactive(user.Email!);
        }

        return true;
    }

    /// <inheritdoc />
    public bool IsUserAccountVerified(UserEntity user)
    {
        if (user is { AuthProvider: EnumAuthProvider.Local, IsVerified: false })
        {
            throw UserErrors.AccountNotVerified(user.Email!);
        }

        return true;
    }

    /// <inheritdoc />
    public async Task<bool> IsSessionValidAsync(Guid sessionId, CancellationToken cancellationToken = default)
    {
        SessionEntity? session = await context
            .Sessions.AsNoTracking()
            .FirstOrDefaultAsync(s => s.Id == sessionId, cancellationToken);

        if (session is null || !session.IsActive())
        {
            throw SessionErrors.InvalidRefreshToken();
        }

        return true;
    }

    /// <inheritdoc />
    public Guid GetSessionIdFromClaims(ClaimsPrincipal user)
    {
        string? sessionIdClaim = user.FindFirst(type: JwtClaimsConstants.SessionId)?.Value;
        if (string.IsNullOrEmpty(value: sessionIdClaim) || !Guid.TryParse(input: sessionIdClaim, out Guid sessionId))
        {
            throw UserErrors.InvalidUserAuthentication();
        }

        return sessionId;
    }

    /// <inheritdoc />
    public bool IsUserAdmin(UserEntity user)
    {
        var adminRoleSpec = new UserHasAdminRoleSpecification();
        if (!adminRoleSpec.IsSatisfiedBy(entity: user))
        {
            throw UserErrors.InsufficientPermissions();
        }

        return true;
    }

    /// <inheritdoc />
    public async Task ValidateUniqueCredentialsAsync(
        Email email,
        string userName,
        CancellationToken cancellationToken = default
    )
    {
        // Check for existing email first using specification
        var emailSpec = new UserByEmailSpecification(email: email.Value);
        bool emailExists = await context.Users.AnyBySpecificationAsync(
            specification: emailSpec,
            cancellationToken: cancellationToken
        );

        if (emailExists)
        {
            throw UserErrors.EmailAlreadyExists(email: email.Value);
        }

        // Check for existing username second using specification
        var usernameSpec = new UserByUserNameSpecification(userName: userName);
        bool usernameExists = await context.Users.AnyBySpecificationAsync(
            specification: usernameSpec,
            cancellationToken: cancellationToken
        );

        if (usernameExists)
        {
            throw UserErrors.UsernameAlreadyExists(username: userName);
        }
    }

    /// <inheritdoc />
    public async Task AssignVisitorRoleAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        // Find the user
        UserEntity? user = await context.Users.FindAsync([userId], cancellationToken: cancellationToken);
        // Find the Visitor role using specification
        var roleSpec = new RoleByNameSpecification(nameof(EnumCoreUserRole.Visitor));
        RoleEntity? visitorRole = await context.Roles.FirstOrDefaultBySpecificationAsync(
            specification: roleSpec,
            cancellationToken: cancellationToken
        );

        if (visitorRole == null)
        {
            throw UserErrors.RoleNotFoundByName(nameof(EnumCoreUserRole.Visitor));
        }

        // Create user-role association using the static factory method
        var userRole = UserRoleEntity.Create(Guid.NewGuid(), userId: userId, roleId: visitorRole.Id);
        // Use the domain method to assign the role
        user?.AssignRole(userRole: userRole);
    }

    /// <inheritdoc cref="IClaimsProvider.GetUserIdFromClaims" />
    public Guid GetUserIdFromClaims(ClaimsPrincipal user)
    {
        string? userIdClaim = user.FindFirst(type: ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(value: userIdClaim) || !Guid.TryParse(input: userIdClaim, out Guid userId))
        {
            throw UserErrors.InvalidUserAuthentication();
        }

        return userId;
    }

    /// <inheritdoc />
    public async Task<UserEntity?> GetOrCreateExternalUserAsync(
        string email,
        string? userName,
        AuthProvider authProvider,
        CancellationToken cancellationToken = default
    )
    {
        UserEntity? user;
        try
        {
            // Try to load existing user including roles and permissions
            user = await GetUserWithRolesAndPermissionsByCredentialsOrThrow(
                credentials: email,
                cancellationToken: cancellationToken
            );
            // Prevent social login if a local account exists
            if (user!.AuthProvider == EnumAuthProvider.Local)
            {
                throw UserErrors.EmailAlreadyExists(email: email);
            }

            // Update username if a new one is provided and it's different
            if (!string.IsNullOrWhiteSpace(value: userName) && user.UserName != userName)
            {
                // Check if another user already takes the new username
                bool usernameExists = await ExistsByUserNameAsync(
                    userName: userName,
                    cancellationToken: cancellationToken
                );
                if (usernameExists)
                {
                    throw UserErrors.UsernameAlreadyExists(username: userName);
                }

                user.UpdateUserName(newUserName: userName);
            }
        }
        catch (NotFoundException)
        {
            // User doesn't exist, create a new one
            user = UserEntity.CreateExternal(Guid.NewGuid(), userName!, authProvider: authProvider.Value, email: email);

            await AddAsync(user: user, cancellationToken: cancellationToken);
            await AssignVisitorRoleAsync(userId: user.Id, cancellationToken: cancellationToken);
            await context.SaveChangesAsync(cancellationToken: cancellationToken);

            // Reload user with roles and permissions after creation
            user = await GetUserWithRolesAndPermissionsByCredentialsOrThrow(
                credentials: email,
                cancellationToken: cancellationToken
            );
        }

        return user;
    }

    /// <inheritdoc />
    public void SetPasswordForExternalUser(UserEntity user, string hashedPassword)
    {
        // Check if user has an email address
        if (string.IsNullOrEmpty(value: user.Email))
        {
            throw UserErrors.EmailRequiredToSetPassword();
        }

        // Check if user already has a password set
        if (user.AuthProvider == EnumAuthProvider.Local)
        {
            throw UserErrors.PasswordOnlyForExternalAuth();
        }

        user.SetPasswordAndChangeToLocal(passwordHash: hashedPassword);
    }
}
