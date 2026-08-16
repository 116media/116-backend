using System.Security.Claims;
using _116.BuildingBlocks.Constants;
using _116.Identity.Application.Auth.Specifications;
using _116.Identity.Application.Roles.Specifications;
using _116.Identity.Application.Session.Specifications;
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
public class AuthRepository(IdentityDbContext context, UserErrors userErrors, SessionErrors sessionErrors)
    : IAuthRepository
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
            throw userErrors.AccountInactive(user.Email!);
        }

        return true;
    }

    /// <inheritdoc />
    public bool IsUserAccountVerified(UserEntity user)
    {
        if (user is { AuthProvider: EnumAuthProvider.Local, IsVerified: false })
        {
            throw userErrors.AccountNotVerified(user.Email!);
        }

        return true;
    }

    /// <inheritdoc />
    public async Task<bool> IsSessionValidAsync(Guid sessionId, CancellationToken cancellationToken = default)
    {
        var specification = new SessionByIdSpecification(sessionId: sessionId);
        SessionEntity? session = await context
            .Sessions.AsNoTracking()
            .ApplySpecification(specification: specification)
            .FirstOrDefaultAsync(cancellationToken);

        if (session is null || !session.IsActive())
        {
            throw sessionErrors.InvalidRefreshToken();
        }

        return true;
    }

    /// <inheritdoc />
    public Guid GetSessionIdFromClaims(ClaimsPrincipal user)
    {
        string? sessionIdClaim = user.FindFirst(type: JwtClaimsConstants.SessionId)?.Value;
        if (string.IsNullOrEmpty(value: sessionIdClaim) || !Guid.TryParse(input: sessionIdClaim, out Guid sessionId))
        {
            throw userErrors.InvalidUserAuthentication();
        }

        return sessionId;
    }

    /// <inheritdoc />
    public bool IsUserAdmin(UserEntity user)
    {
        var adminRoleSpec = new UserHasAdminRoleSpecification();
        if (!adminRoleSpec.IsSatisfiedBy(entity: user))
        {
            throw userErrors.InsufficientPermissions();
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
            throw userErrors.EmailAlreadyExists(email: email.Value);
        }

        // Check for existing username second using specification
        var usernameSpec = new UserByUserNameSpecification(userName: userName);
        bool usernameExists = await context.Users.AnyBySpecificationAsync(
            specification: usernameSpec,
            cancellationToken: cancellationToken
        );

        if (usernameExists)
        {
            throw userErrors.UsernameAlreadyExists(username: userName);
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
            throw userErrors.RoleNotFoundByName(nameof(EnumCoreUserRole.Visitor));
        }

        // Create user-role association using the static factory method
        var userRole = UserRoleEntity.CreateBootstrap(Guid.NewGuid(), userId: userId, roleId: visitorRole.Id);

        // Use the domain method to assign the role
        user?.AssignRole(userRole: userRole, errors: userErrors);
    }

    /// <inheritdoc cref="IClaimsProvider.GetUserIdFromClaims" />
    public Guid GetUserIdFromClaims(ClaimsPrincipal user)
    {
        string? userIdClaim = user.FindFirst(type: ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(value: userIdClaim) || !Guid.TryParse(input: userIdClaim, out Guid userId))
        {
            throw userErrors.InvalidUserAuthentication();
        }

        return userId;
    }

    /// <inheritdoc />
    public async Task<UserEntity?> GetOrCreateExternalUserAsync(
        string email,
        string? userName,
        AuthProvider authProvider,
        string providerSubjectId,
        CancellationToken cancellationToken = default
    )
    {
        // Subject-id match — the authoritative key.
        UserEntity? user = await GetUserWithRolesAndPermissionsByProviderSubjectAsync(
            authProvider: authProvider.Value,
            providerSubjectId: providerSubjectId,
            cancellationToken: cancellationToken
        );
        if (user is not null)
        {
            return user;
        }

        try
        {
            // Email match — link or reject; never silently take over.
            user = await GetUserWithRolesAndPermissionsByCredentialsOrThrow(
                credentials: email,
                cancellationToken: cancellationToken
            );

            // Prevent social login if a local account owns the email.
            if (user!.AuthProvider == EnumAuthProvider.Local)
            {
                throw userErrors.EmailAlreadyExists(email: email);
            }

            // Existing external row: link if unlinked, reject if it belongs to another subject.
            user.LinkProviderSubject(providerSubjectId: providerSubjectId, errors: userErrors);

            // Update username if a new, still-available one is provided.
            if (!string.IsNullOrWhiteSpace(value: userName) && user.UserName != userName)
            {
                bool usernameExists = await ExistsByUserNameAsync(
                    userName: userName,
                    cancellationToken: cancellationToken
                );
                if (!usernameExists)
                {
                    user.UpdateUserName(newUserName: userName, errors: userErrors);
                }
            }

            await context.SaveChangesAsync(cancellationToken: cancellationToken);
            return user;
        }
        catch (NotFoundException)
        {
            // Case of a Brand-new account.
            user = UserEntity.CreateExternal(
                Guid.NewGuid(),
                userName!,
                authProvider: authProvider.Value,
                providerSubjectId: providerSubjectId,
                errors: userErrors,
                email: email
            );

            await AddAsync(user: user, cancellationToken: cancellationToken);
            await AssignVisitorRoleAsync(userId: user.Id, cancellationToken: cancellationToken);

            await context.UserTokenStates.AddAsync(
                entity: UserTokenStateEntity.Create(userId: user.Id),
                cancellationToken: cancellationToken
            );

            await context.SaveChangesAsync(cancellationToken: cancellationToken);

            // Reload with roles and permissions after creation.
            return await GetUserWithRolesAndPermissionsByProviderSubjectAsync(
                authProvider: authProvider.Value,
                providerSubjectId: providerSubjectId,
                cancellationToken: cancellationToken
            );
        }
    }

    /// <summary>
    /// Loads the external user identified by the provider and subject id, including roles and
    /// permissions, or null when no such account exists.
    /// </summary>
    /// <param name="authProvider">The provider the subject id belongs to.</param>
    /// <param name="providerSubjectId">The provider's stable subject id.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>The matching user with its role graph, or null.</returns>
    private async Task<UserEntity?> GetUserWithRolesAndPermissionsByProviderSubjectAsync(
        EnumAuthProvider authProvider,
        string providerSubjectId,
        CancellationToken cancellationToken = default
    )
    {
        return await context
            .Users.Where(u => u.AuthProvider == authProvider && u.ProviderSubjectId == providerSubjectId)
            .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
                    .ThenInclude(r => r.RolePermissions)
                        .ThenInclude(rp => rp.Permission)
            .AsSplitQuery()
            .FirstOrDefaultAsync(cancellationToken);
    }

    /// <inheritdoc />
    public void SetPasswordForExternalUser(UserEntity user, string hashedPassword)
    {
        // Check if user has an email address
        if (string.IsNullOrEmpty(value: user.Email))
        {
            throw userErrors.EmailRequiredToSetPassword();
        }

        // Check if user already has a password set
        if (user.AuthProvider == EnumAuthProvider.Local)
        {
            throw userErrors.PasswordOnlyForExternalAuth();
        }

        user.SetPasswordAndChangeToLocal(passwordHash: hashedPassword, errors: userErrors);
    }
}
