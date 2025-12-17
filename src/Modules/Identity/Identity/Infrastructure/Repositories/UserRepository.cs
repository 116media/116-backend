using _116.Shared.Infrastructure.Extensions;
using _116.Identity.Application.Shared.Errors;
using _116.Identity.Application.Shared.Repositories;
using _116.Identity.Application.Shared.Specifications;
using _116.Identity.Domain.Entities;
using _116.Identity.Domain.Enums;
using _116.Identity.Domain.ValueObjects;
using _116.Identity.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using _116.Shared.Application.Exceptions;

namespace _116.Identity.Infrastructure.Repositories;

/// <summary>
/// Implementation of <see cref="IUserRepository"/> using Entity Framework Core.
/// </summary>
public class UserRepository(AuthDbContext context) : IUserRepository
{
    /// <inheritdoc />
    public async Task<UserEntity?> FindUserByIdOrThrow(Guid userId, CancellationToken cancellationToken = default)
    {
        return await context.Users.FindOrThrowAsync([userId], cancellationToken);
    }

    /// <inheritdoc />
    public async Task<UserEntity?> GetUserWithRolesByIdOrThrow(
        Guid userId,
        CancellationToken cancellationToken = default
    )
    {
        var specification = new UserByIdSpecification(userId);

        return await context.Users
            .ApplySpecification(specification)
            .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
            .FirstDefaultOrThrowAsync(
                keyValue: userId,
                cancellationToken: cancellationToken
            );
    }

    /// <inheritdoc />
    public async Task<UserEntity?> GetUserWithRolesByEmailOrThrow(
        Email email,
        CancellationToken cancellationToken = default
    )
    {
        var specification = new UserByEmailSpecification(email.Value);

        return await context.Users
            .ApplySpecification(specification)
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
        var specification = new UserByIdSpecification(userId);

        return await context.Users
            .ApplySpecification(specification)
            .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
                    .ThenInclude(r => r.RolePermissions)
                        .ThenInclude(rp => rp.Permission)
            .AsSplitQuery()
            .FirstDefaultOrThrowAsync(
                keyValue: userId,
                cancellationToken: cancellationToken
            );
    }

    /// <inheritdoc />
    public async Task<UserEntity?> GetUserWithRolesAndPermissionsByEmailOrThrow(
        Email email,
        CancellationToken cancellationToken = default
    )
    {
        var specification = new UserByEmailSpecification(email.Value);

        return await context.Users
            .ApplySpecification(specification)
            .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
                    .ThenInclude(r => r.RolePermissions)
                        .ThenInclude(rp => rp.Permission)
            .AsSplitQuery()
            .FirstDefaultOrThrowAsync(
                keyName: "email",
                keyValue: email.Value,
                cancellationToken: cancellationToken
            );
    }

    /// <inheritdoc />
    public async Task<UserEntity?> GetUserWithRolesAndPermissionsByCredentialsOrThrow(
        string credentials,
        CancellationToken cancellationToken = default
    )
    {
        // Use specification to determine if credentials is an email or username
        var specification = new UserByCredentialsSpecification(credentials);

        // Get the user by email or username without any status checks
        return await context.Users
            .ApplySpecification(specification)
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
        var specification = new UserByEmailSpecification(email.Value);
        return await context.Users.AnyBySpecificationAsync(specification, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<bool> ExistsByUserNameAsync(string userName, CancellationToken cancellationToken = default)
    {
        var specification = new UserByUserNameSpecification(userName);
        return await context.Users.AnyBySpecificationAsync(specification, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<UserEntity?> GetUserByPhoneNumberAsync(
        string phoneNumber,
        CancellationToken cancellationToken = default
    )
    {
        var specification = new UserByPhoneNumberSpecification(phoneNumber);
        return await context.Users.FirstOrDefaultBySpecificationAsync(specification, cancellationToken);
    }

    /// <inheritdoc />
    public async Task AddAsync(UserEntity user, CancellationToken cancellationToken = default)
    {
        await context.Users.AddAsync(user, cancellationToken);
    }

    /// <inheritdoc />
    public Task UpdateAsync(UserEntity user, CancellationToken cancellationToken = default)
    {
        context.Users.Update(user);
        return Task.CompletedTask;
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
    public bool IsUserLoggedIn(UserEntity user)
    {
        if (!user.IsLoggedIn)
        {
            throw UserErrors.UserNotLoggedIn(user.Email!);
        }
        return true;
    }

    /// <inheritdoc />
    public bool IsUserAdmin(UserEntity user)
    {
        var adminRoleSpec = new UserHasAdminRoleSpecification();
        if (!adminRoleSpec.IsSatisfiedBy(user))
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
        var emailSpec = new UserByEmailSpecification(email.Value);
        bool emailExists = await context.Users.AnyBySpecificationAsync(emailSpec, cancellationToken);
        if (emailExists)
        {
            throw UserErrors.EmailAlreadyExists(email.Value);
        }

        // Check for existing username second using specification
        var usernameSpec = new UserByUserNameSpecification(userName);
        bool usernameExists = await context.Users.AnyBySpecificationAsync(usernameSpec, cancellationToken);
        if (usernameExists)
        {
            throw UserErrors.UsernameAlreadyExists(userName);
        }
    }

    /// <inheritdoc />
    public async Task AssignVisitorRoleAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        // Find the user
        UserEntity? user = await context.Users.FindAsync([userId], cancellationToken);

        // Find the Visitor role using specification
        var roleSpec = new RoleByNameSpecification(nameof(EnumCoreUserRole.Visitor));
        RoleEntity? visitorRole = await context.Roles
            .FirstOrDefaultBySpecificationAsync(roleSpec, cancellationToken);

        if (visitorRole == null)
        {
            throw UserErrors.RoleNotFoundByName(nameof(EnumCoreUserRole.Visitor));
        }

        // Create user-role association using the static factory method
        var userRole = UserRoleEntity.Create(Guid.NewGuid(), userId, visitorRole.Id);

        // Use the domain method to assign the role
        user?.AssignRole(userRole);
    }

    /// <inheritdoc />
    public Guid GetUserIdFromClaims(ClaimsPrincipal user)
    {
        string? userIdClaim = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out Guid userId))
        {
            throw UserErrors.InvalidUserAuthentication();
        }
        return userId;
    }

    /// <inheritdoc />
    public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        await context.SaveChangesAsync(cancellationToken);
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
                email, cancellationToken
            );

            // Prevent social login if a local account exists
            if (user!.AuthProvider == EnumAuthProvider.Local)
            {
                throw UserErrors.EmailAlreadyExists(email);
            }

            // Update username if a new one is provided and it's different
            if (!string.IsNullOrWhiteSpace(userName) && user.UserName != userName)
            {
                // Check if another user already takes the new username
                bool usernameExists = await ExistsByUserNameAsync(userName, cancellationToken);
                if (usernameExists)
                {
                    throw UserErrors.UsernameAlreadyExists(userName);
                }

                user.UpdateUserName(userName);
            }
        }
        catch (NotFoundException)
        {
            // User doesn't exist, create a new one
            user = UserEntity.CreateExternal(
                id: Guid.NewGuid(),
                userName: userName!,
                authProvider: authProvider.Value,
                email: email
            );

            await AddAsync(user, cancellationToken);
            await AssignVisitorRoleAsync(user.Id, cancellationToken);
            await SaveChangesAsync(cancellationToken);

            // Reload user with roles and permissions after creation
            user = await GetUserWithRolesAndPermissionsByCredentialsOrThrow(
                email, cancellationToken
            );
        }

        return user;
    }

    /// <inheritdoc />
    public void SetPasswordForExternalUser(UserEntity user, string hashedPassword)
    {
        // Check if user has an email address
        if (string.IsNullOrEmpty(user.Email))
        {
            throw UserErrors.EmailRequiredToSetPassword();
        }

        // Check if user already has a password set
        if (user.AuthProvider == EnumAuthProvider.Local)
        {
            throw UserErrors.PasswordOnlyForExternalAuth();
        }

        user.SetPasswordAndChangeToLocal(hashedPassword);
    }
}
