using _116.Shared.Infrastructure.Extensions;
using _116.User.Application.Shared.Errors;
using _116.User.Application.Shared.Repositories;
using _116.User.Domain.Entities;
using _116.User.Domain.Enums;
using _116.User.Domain.ValueObjects;
using _116.User.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace _116.User.Infrastructure.Repositories;

/// <summary>
/// Implementation of <see cref="IUserRepository"/> using Entity Framework Core.
/// </summary>
public class UserRepository(UserDbContext context) : IUserRepository
{
    /// <inheritdoc />
    public async Task<UserEntity> GetUserWithRolesOrThrowAsync(Email email, CancellationToken cancellationToken = default)
    {
        return await context.Users
            .Where(u => u.Email == email.Value)
            .Include(u => u.UserRoles).ThenInclude(ur => ur.Role)
            .FirstDefaultOrThrowAsync(
                keyName: "credentials",
                keyValue: email.Value,
                cancellationToken: cancellationToken
            );
    }

    /// <inheritdoc />
    public async Task<UserEntity?> GetUserByIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await context.Users.FindOrThrowAsync([userId], cancellationToken);
    }

    /// <inheritdoc />
    public async Task<UserEntity?> GetUserWithRolesByIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await context.Users
            .Where(u => u.Id == userId)
            .Include(u => u.UserRoles).ThenInclude(ur => ur.Role)
            .FirstDefaultOrThrowAsync(
                keyValue: userId,
                cancellationToken: cancellationToken
            );
    }

    public async Task<UserEntity?> GetUserWithRolesAndPermissionsByIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await context.Users
            .Where(u => u.Id == userId)
            .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
                    .ThenInclude(r => r.RolePermissions)
                        .ThenInclude(rp => rp.Permission)
            .FirstDefaultOrThrowAsync(
                keyValue: userId,
                cancellationToken: cancellationToken
            );
    }

    /// <inheritdoc />
    public async Task<bool> ExistsByEmailAsync(Email email, CancellationToken cancellationToken = default)
    {
        return await context.Users.AnyAsync(u => u.Email == email.Value, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<bool> ExistsByUserNameAsync(string userName, CancellationToken cancellationToken = default)
    {
        return await context.Users.AnyAsync(u => u.UserName == userName, cancellationToken);
    }

    /// <inheritdoc />
    public async Task AddAsync(UserEntity user, CancellationToken cancellationToken = default)
    {
        await context.Users.AddAsync(user, cancellationToken);
    }

    /// <inheritdoc />
    public async Task UpdateAsync(UserEntity user, CancellationToken cancellationToken = default)
    {
        context.Users.Update(user);
        await context.SaveChangesAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<UserEntity> GetActiveAdminUserWithRolesAndPermissionsAsync(Email email, CancellationToken cancellationToken = default)
    {
        // First, get the user without filtering by IsActive to provide specific error messages
        UserEntity user = await context.Users
            .Where(u => u.Email == email.Value)
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

        // Check if the account is active
        IsUserAccountActive(user);

        // Validate admin privileges
        bool hasAdminRole = user.UserRoles
            .Any(ur => ur.Role.Name is nameof(CoreUserRole.Admin) or nameof(CoreUserRole.SuperAdmin));

        if (!hasAdminRole)
        {
            throw UserErrors.InvalidCredentials();
        }

        return user;
    }

    /// <inheritdoc />
    public async Task<UserEntity> GetPublicUserWithRolesAndPermissionsAsync(
        string credentials,
        CancellationToken cancellationToken = default
    )
    {
        // Check if credentials is an email (contains @ and .) or username
        bool isEmail = credentials.Contains('@') && credentials.Contains('.');

        // Get the user by email or username without any status checks
        return await context.Users
            .Where(u => isEmail ? u.Email == credentials : u.UserName == credentials)
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
        if (user is { AuthProvider: AuthProvider.Local, IsVerified: false })
        {
            throw UserErrors.AccountNotVerified(user.Email!);
        }
        return true;
    }

    /// <inheritdoc />
    public async Task<UserEntity> GetActivePublicUserWithRolesAndPermissionsAsync(string credentials, CancellationToken cancellationToken = default)
    {
        // Check if credentials is an email (contains @ and .) or username
        bool isEmail = credentials.Contains('@') && credentials.Contains('.');

        // Get the user by email or username without filtering by IsActive to provide specific error messages
        UserEntity user = await context.Users
            .Where(u => isEmail ? u.Email == credentials : u.UserName == credentials)
            .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
                    .ThenInclude(r => r.RolePermissions)
                        .ThenInclude(rp => rp.Permission)
            .FirstDefaultOrThrowAsync(
                keyName: "credentials",
                keyValue: credentials,
                cancellationToken: cancellationToken
            );

        // Check if the account is active
        IsUserAccountActive(user);

        // Check if the account is verified (for local auth)
        IsUserAccountVerified(user);

        return user;
    }

    /// <inheritdoc />
    public async Task ValidateUniqueCredentialsAsync(Email email, string userName, CancellationToken cancellationToken = default)
    {
        // Check for existing email first
        bool emailExists = await context.Users.AnyAsync(u => u.Email == email.Value, cancellationToken);
        if (emailExists)
        {
            throw UserErrors.EmailAlreadyExists(email.Value);
        }

        // Check for existing username second
        bool usernameExists = await context.Users.AnyAsync(u => u.UserName == userName, cancellationToken);
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

        // Find the Visitor role
        RoleEntity? visitorRole = await context.Roles
            .FirstOrDefaultAsync(r => r.Name == nameof(CoreUserRole.Visitor), cancellationToken);

        if (visitorRole == null)
        {
            throw UserErrors.RoleNotFoundByName(nameof(CoreUserRole.Visitor));
        }

        // Create user-role association using the static factory method
        var userRole = UserRoleEntity.Create(Guid.NewGuid(), userId, visitorRole.Id);

        // Use the domain method to assign the role
        user?.AssignRole(userRole);
    }

    /// <inheritdoc />
    public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        await context.SaveChangesAsync(cancellationToken);
    }
}
