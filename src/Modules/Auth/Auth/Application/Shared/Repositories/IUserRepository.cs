using _116.Shared.Application.Exceptions;
using _116.Shared.Domain;
using _116.Auth.Domain.Entities;
using _116.Auth.Domain.ValueObjects;
using System.Security.Claims;

namespace _116.Auth.Application.Shared.Repositories;

/// <summary>
/// Repository interface for managing user entities and their related data.
/// Provides methods for user retrieval, validation, and persistence operations.
/// </summary>
public interface IUserRepository : IRepository<UserEntity>
{
    /// <summary>
    /// Retrieves a user with their associated roles by email address.
    /// </summary>
    /// <param name="email">The email address to search for.</param>
    /// <param name="cancellationToken">Token to observe for cancellation requests.</param>
    /// <returns>The user entity with roles loaded.</returns>
    /// <exception cref="NotFoundException">Thrown when no user is found with the specified email.</exception>
    /// <remarks>
    /// This method includes the user's roles in the query for authentication scenarios.
    /// Use this method when you need to validate user credentials and roles.
    /// </remarks>
    Task<UserEntity> GetUserWithRolesOrThrowAsync(Email email, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves a user by their unique identifier.
    /// </summary>
    /// <param name="userId">The unique identifier of the user.</param>
    /// <param name="cancellationToken">Token to observe for cancellation requests.</param>
    /// <returns>The user entity if found; otherwise, null.</returns>
    /// <remarks>
    /// This method performs a simple lookup by primary key without loading related entities.
    /// </remarks>
    Task<UserEntity?> GetUserByIdAsync(Guid userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves a user with their associated roles by unique identifier.
    /// </summary>
    /// <param name="userId">The unique identifier of the user.</param>
    /// <param name="cancellationToken">Token to observe for cancellation requests.</param>
    /// <returns>The user entity with roles loaded if found; otherwise, null.</returns>
    /// <remarks>
    /// This method includes the user's roles in the query for profile scenarios.
    /// Use this method when you need user information along with their roles and permissions.
    /// </remarks>
    Task<UserEntity?> GetUserWithRolesByIdAsync(Guid userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves a user with their associated roles and permissions by unique identifier.
    /// </summary>
    /// <param name="userId">The unique identifier of the user.</param>
    /// <param name="cancellationToken">Token to observe for cancellation requests.</param>
    /// <returns>The user entity with roles and permissions loaded if found; otherwise, null.</returns>
    /// <remarks>
    /// This method includes the complete entity graph: user → roles → permissions.
    /// Use this method when you need user information along with their roles and detailed permissions.
    /// </remarks>
    Task<UserEntity?> GetUserWithRolesAndPermissionsByIdAsync(Guid userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if a user exists with the specified email address.
    /// </summary>
    /// <param name="email">The email address to check for existence.</param>
    /// <param name="cancellationToken">Token to observe for cancellation requests.</param>
    /// <returns>True if a user exists with the email, otherwise, false.</returns>
    /// <remarks>
    /// This method is useful for email uniqueness validation during user registration.
    /// </remarks>
    Task<bool> ExistsByEmailAsync(Email email, CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if a user exists with the specified username.
    /// </summary>
    /// <param name="userName">The username to check for existence.</param>
    /// <param name="cancellationToken">Token to observe for cancellation requests.</param>
    /// <returns>True if a user exists with the username, otherwise, false.</returns>
    /// <remarks>
    /// This method is useful for username uniqueness validation during user registration.
    /// </remarks>
    Task<bool> ExistsByUserNameAsync(string userName, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves a user by their phone number.
    /// </summary>
    /// <param name="phoneNumber">The full phone number to search for.</param>
    /// <param name="cancellationToken">Token to observe for cancellation requests.</param>
    /// <returns>The user entity if found; otherwise, null.</returns>
    /// <remarks>
    /// This method performs a simple lookup by phone number without loading related entities.
    /// Useful for phone number uniqueness validation during profile updates.
    /// </remarks>
    Task<UserEntity?> GetUserByPhoneNumberAsync(string phoneNumber, CancellationToken cancellationToken = default);

    /// <summary>
    /// Validates that both email and username are unique for user registration.
    /// </summary>
    /// <param name="email">The email address to check for uniqueness.</param>
    /// <param name="userName">The username to check for uniqueness.</param>
    /// <param name="cancellationToken">Token to observe for cancellation requests.</param>
    /// <returns>A task representing the asynchronous validation operation.</returns>
    /// <exception cref="ConflictException">Thrown when email or username already exists.</exception>
    /// <remarks>
    /// This method performs both email and username uniqueness validation in a single database operation.
    /// It throws specific conflict exceptions for email or username conflicts.
    /// </remarks>
    Task ValidateUniqueCredentialsAsync(Email email, string userName, CancellationToken cancellationToken = default);

    /// <summary>
    /// Assigns the Visitor role to a new user during registration.
    /// </summary>
    /// <param name="userId">The unique identifier of the user.</param>
    /// <param name="cancellationToken">Token to observe for cancellation requests.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    /// <exception cref="NotFoundException">Thrown when the Visitor role is not found in the system.</exception>
    /// <remarks>
    /// This method automatically assigns the default Visitor role to new users.
    /// Should be called as part of the user registration process.
    /// </remarks>
    Task AssignVisitorRoleAsync(Guid userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds a new user entity to the repository.
    /// </summary>
    /// <param name="user">The user entity to add.</param>
    /// <param name="cancellationToken">Token to observe for cancellation requests.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    /// <remarks>
    /// This method only adds the user to the context. Call <see cref="SaveChangesAsync"/> to persist changes.
    /// </remarks>
    Task AddAsync(UserEntity user, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an existing user entity in the repository.
    /// </summary>
    /// <param name="user">The user entity to update.</param>
    /// <param name="cancellationToken">Token to observe for cancellation requests.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    /// <remarks>
    /// This method marks the user as modified and automatically saves changes to the database.
    /// </remarks>
    Task UpdateAsync(UserEntity user, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves an active admin user with roles and permissions by email address.
    /// Validates that the user exists, is active, and has administrative privileges.
    /// </summary>
    /// <param name="email">The email address to search for.</param>
    /// <param name="cancellationToken">Token to observe for cancellation requests.</param>
    /// <returns>The admin user entity with roles and permissions loaded.</returns>
    /// <exception cref="NotFoundException">Thrown when no user is found with the specified email.</exception>
    /// <exception cref="AuthorizationException">Thrown when the user account is inactive (HTTP 403 Forbidden).</exception>
    /// <exception cref="AuthenticationException">Thrown when the user lacks administrative privileges (HTTP 401 Unauthorized).</exception>
    /// <remarks>
    /// This method specifically validates admin privileges and account status with proper HTTP status mapping:
    /// - AuthorizationException: User exists but the account is inactive (403 Forbidden)
    /// - AuthenticationException: User lacks Admin or SuperAdmin role (401 Unauthorized)
    /// Use this for admin authentication scenarios to ensure proper error handling.
    /// </remarks>
    Task<UserEntity> GetActiveAdminUserWithRolesAndPermissionsAsync(Email email, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves a public user with roles and permissions by credentials (email or username).
    /// Only validates that the user exists - does not check verification or account status.
    /// </summary>
    /// <param name="credentials">The credentials to search for (email or username).</param>
    /// <param name="cancellationToken">Token to observe for cancellation requests.</param>
    /// <returns>The public user entity with roles and permissions loaded.</returns>
    /// <exception cref="NotFoundException">Thrown when no user is found with the specified credentials.</exception>
    /// <remarks>
    /// This method accepts either email address or username as credentials.
    /// It does not perform account status or verification checks - use this when you need to
    /// validate credentials first before checking account status.
    /// </remarks>
    Task<UserEntity> GetPublicUserWithRolesAndPermissionsAsync(string credentials, CancellationToken cancellationToken = default);

    /// <summary>
    /// Validates that a user account is active.
    /// </summary>
    /// <param name="user">The user entity to validate.</param>
    /// <returns>True if the account is active, otherwise throws an exception.</returns>
    /// <exception cref="AuthorizationException">Thrown when the user account is inactive (HTTP 403 Forbidden).</exception>
    /// <remarks>
    /// This method should be called after password verification to ensure account status
    /// is only revealed for valid credentials.
    /// </remarks>
    bool IsUserAccountActive(UserEntity user);

    /// <summary>
    /// Validates that a user account is verified for local authentication.
    /// </summary>
    /// <param name="user">The user entity to validate.</param>
    /// <returns>True, if the account is verified or not using local auth, otherwise throws an exception.</returns>
    /// <exception cref="AuthorizationException">Thrown when the local account is not verified (HTTP 403 Forbidden).</exception>
    /// <remarks>
    /// This method should be called after password verification to ensure verification status
    /// is only revealed for valid credentials. Only applies to local authentication provider.
    /// </remarks>
    bool IsUserAccountVerified(UserEntity user);

    /// <summary>
    /// Validates that a user is currently logged in.
    /// </summary>
    /// <param name="user">The user entity to validate.</param>
    /// <returns>True if the user is logged in, otherwise throws an exception.</returns>
    /// <exception cref="AuthorizationException">Thrown when the user is not logged in (HTTP 403 Forbidden).</exception>
    /// <remarks>
    /// This method should be called to ensure the user's session is still valid
    /// and they haven't been logged out.
    /// </remarks>
    bool IsUserLoggedIn(UserEntity user);

    /// <summary>
    /// Validates that a user has administrative privileges (Admin or SuperAdmin role).
    /// </summary>
    /// <param name="user">The user entity to validate.</param>
    /// <returns>True if the user has admin privileges, otherwise throws an exception.</returns>
    /// <exception cref="AuthenticationException">Thrown when the user lacks administrative privileges (HTTP 401 Unauthorized).</exception>
    /// <remarks>
    /// This method checks if the user has either Admin or SuperAdmin role.
    /// Should be called after authentication to validate admin access.
    /// </remarks>
    bool IsUserAdmin(UserEntity user);

    /// <summary>
    /// Retrieves an active public user with roles and permissions by credentials (email or username).
    /// Validates that the user exists and is active for public authentication.
    /// </summary>
    /// <param name="credentials">The credentials to search for (email or username).</param>
    /// <param name="cancellationToken">Token to observe for cancellation requests.</param>
    /// <returns>The public user entity with roles and permissions loaded.</returns>
    /// <exception cref="NotFoundException">Thrown when no user is found with the specified credentials.</exception>
    /// <exception cref="AuthorizationException">Thrown when the user account is inactive (HTTP 403 Forbidden).</exception>
    /// <exception cref="AuthorizationException">Thrown when the user account is not verified (HTTP 403 Forbidden).</exception>
    /// <remarks>
    /// This method accepts either email address or username as credentials for public user authentication.
    /// It validates account status with proper HTTP status mapping:
    /// - AuthorizationException: User exists, but the account is inactive or not verified (403 Forbidden)
    /// Use this for public user authentication scenarios to ensure proper error handling.
    /// </remarks>
    Task<UserEntity> GetActivePublicUserWithRolesAndPermissionsAsync(string credentials, CancellationToken cancellationToken = default);

    /// <summary>
    /// Extracts the user ID from JWT claims and validates authentication.
    /// </summary>
    /// <param name="user">The claims principal from the authenticated user.</param>
    /// <returns>The extracted user ID as a Guid.</returns>
    /// <exception cref="AuthenticationException">Thrown when user authentication is invalid or user ID cannot be parsed.</exception>
    /// <remarks>
    /// This method centralizes the logic for extracting user IDs from JWT tokens
    /// and provides consistent error handling across all endpoints.
    /// </remarks>
    Guid GetUserIdFromClaims(ClaimsPrincipal user);

    /// <summary>
    /// Persists all pending changes in the repository to the database.
    /// </summary>
    /// <param name="cancellationToken">Token to observe for cancellation requests.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    /// <remarks>
    /// This method commits all pending changes (adds, updates, deletes) to the database.
    /// Should be called after performing repository operations that need persistence.
    /// </remarks>
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
