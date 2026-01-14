using System.Security.Claims;
using _116.Identity.Domain.Entities;
using _116.Identity.Domain.ValueObjects;
using _116.Shared.Application.Exceptions;
using _116.Shared.Domain;

namespace _116.Identity.Application.Shared.Repositories;

/// <summary>
/// Repository interface for authentication-related operations.
/// Provides methods for user authentication, authorization, session validation, and credential management.
/// This repository handles auth flows like login, signup, password management, and OTP verification.
/// For admin user CRUD operations, use I UserRepository instead.
/// </summary>
public interface IAuthRepository : IRepository<UserEntity>
{
    /// <summary>
    /// Retrieves a user by their unique identifier.
    /// </summary>
    /// <param name="userId">The unique identifier of the user.</param>
    /// <param name="cancellationToken">Token to observe for cancellation requests.</param>
    /// <returns>The user entity if found; otherwise, null.</returns>
    /// <exception cref="NotFoundException">
    /// Thrown when an entity with the specified primary key values is not found.
    /// </exception>
    /// <remarks>
    /// This method performs a simple lookup by primary key without loading related entities.
    /// </remarks>
    Task<UserEntity?> FindUserByIdOrThrow(Guid userId, CancellationToken cancellationToken = default);

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
    Task<UserEntity?> GetUserWithRolesByEmailOrThrow(Email email, CancellationToken cancellationToken = default);

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
    Task<UserEntity?> GetUserWithRolesAndPermissionsByIdOrThrow(
        Guid userId,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Retrieves a user with their active sessions by unique identifier.
    /// </summary>
    /// <param name="userId">The unique identifier of the user.</param>
    /// <param name="cancellationToken">Token to observe for cancellation requests.</param>
    /// <returns>The user entity with active sessions loaded.</returns>
    /// <exception cref="NotFoundException">Thrown when no user is found with the specified ID.</exception>
    /// <remarks>
    /// This method only loads sessions that are currently active (not expired and not deleted).
    /// Optimized to avoid loading unnecessary session data by filtering at the database level.
    /// Use this method when you need to validate if a user has any active login sessions.
    /// </remarks>
    Task<UserEntity?> GetUserWithSessionsByIdOrThrow(Guid userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves a user with their associated roles and permissions by email address.
    /// </summary>
    /// <param name="email">The email address to search for.</param>
    /// <param name="cancellationToken">Token to observe for cancellation requests.</param>
    /// <returns>The user entity with roles and permissions loaded.</returns>
    /// <exception cref="NotFoundException">Thrown when no user is found with the specified email.</exception>
    /// <remarks>
    /// This method includes the complete entity graph: user → roles → permissions.
    /// Uses AsSplitQuery for optimized loading of related entities.
    /// Use this method when you need user information along with their roles and detailed permissions by email.
    /// </remarks>
    Task<UserEntity?> GetUserWithRolesAndPermissionsByEmailOrThrow(
        Email email,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Retrieves a user with their associated roles and permissions by credentials (email or username).
    /// </summary>
    /// <param name="credentials">The credentials to search for (email or username).</param>
    /// <param name="cancellationToken">Token to observe for cancellation requests.</param>
    /// <returns>The user entity with roles and permissions loaded.</returns>
    /// <exception cref="NotFoundException">Thrown when no user is found with the specified credentials.</exception>
    /// <remarks>
    /// This method includes the complete entity graph: user → roles → permissions.
    /// Uses AsSplitQuery for optimized loading of related entities.
    /// Accepts either email address or username as credentials.
    /// Do not perform account status or verification checks - use this when you need to
    /// validate credentials first before checking account status separately.
    /// </remarks>
    Task<UserEntity?> GetUserWithRolesAndPermissionsByCredentialsOrThrow(
        string credentials,
        CancellationToken cancellationToken = default
    );

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
    /// This method only adds the user to the context. Call UnitOfWork.CommitAsync() to persist changes.
    /// </remarks>
    Task AddAsync(UserEntity user, CancellationToken cancellationToken = default);

    /// <summary>
    /// Validates that a user account is active.
    /// </summary>
    /// <param name="user">The user entity to validate.</param>
    /// <returns>True, if the account is active, otherwise throws an exception.</returns>
    /// <exception cref="AuthorizationException">
    /// Thrown when the user account is inactive (HTTP 403 Forbidden).
    /// </exception>
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
    /// <exception cref="AuthorizationException">
    /// Thrown when the local account is not verified (HTTP 403 Forbidden).
    /// </exception>
    /// <remarks>
    /// This method should be called after password verification to ensure verification status
    /// is only revealed for valid credentials. Only applies to local authentication provider.
    /// </remarks>
    bool IsUserAccountVerified(UserEntity user);

    /// <summary>
    /// Validates that the specified session is currently active.
    /// </summary>
    /// <param name="sessionId">The unique identifier of the session to validate.</param>
    /// <param name="cancellationToken">Token to observe for cancellation requests.</param>
    /// <returns>True, if the session is active, otherwise throws an exception.</returns>
    /// <exception cref="AuthorizationException">
    /// Thrown when the session is not active (HTTP 403 Forbidden).
    /// </exception>
    /// <remarks>
    /// This method validates a specific session by ID rather than checking if the user has any active session.
    /// Use this method with the session ID extracted from JWT claims for precise session validation.
    /// </remarks>
    Task<bool> IsSessionValidAsync(Guid sessionId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Extracts the session ID from JWT claims and validates it exists.
    /// </summary>
    /// <param name="user">The claims principal from the authenticated user.</param>
    /// <returns>The extracted session ID as a Guid.</returns>
    /// <exception cref="AuthenticationException">
    /// Thrown when session ID claim is missing or invalid.
    /// </exception>
    /// <remarks>
    /// This method centralizes the logic for extracting session IDs from JWT tokens
    /// and provides consistent error handling across all endpoints.
    /// </remarks>
    Guid GetSessionIdFromClaims(ClaimsPrincipal user);

    /// <summary>
    /// Validates that a user has administrative privileges (Admin or SuperAdmin role).
    /// </summary>
    /// <param name="user">The user entity to validate.</param>
    /// <returns>True, if the user has admin privileges, otherwise throws an exception.</returns>
    /// <exception cref="AuthenticationException">
    /// Thrown when the user lacks administrative privileges (HTTP 401 Unauthorized).
    /// </exception>
    /// <remarks>
    /// This method checks if the user has either Admin or SuperAdmin role.
    /// Should be called after authentication to validate admin access.
    /// </remarks>
    bool IsUserAdmin(UserEntity user);

    /// <summary>
    /// Extracts the user ID from JWT claims and validates authentication.
    /// </summary>
    /// <param name="user">The claims principal from the authenticated user.</param>
    /// <returns>The extracted user ID as a Guid.</returns>
    /// <exception cref="AuthenticationException">
    /// Thrown when user authentication is invalid or user ID cannot be parsed.
    /// </exception>
    /// <remarks>
    /// This method centralizes the logic for extracting user IDs from JWT tokens
    /// and provides consistent error handling across all endpoints.
    /// </remarks>
    Guid GetUserIdFromClaims(ClaimsPrincipal user);

    /// <summary>
    /// Gets the existing external user or creates a new one for social authentication.
    /// </summary>
    /// <param name="email">User's email address.</param>
    /// <param name="userName">Username from social provider.</param>
    /// <param name="authProvider">Authentication provider.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>User entity with roles and permissions loaded.</returns>
    /// <exception cref="ConflictException">Thrown when a local account exists with the same email.</exception>
    /// <remarks>
    /// This method handles the complete workflow for social authentication:
    /// - Checks if a user with the email exists
    /// - Prevents social login if a local account exists
    /// - Updates username if provided and different from existing
    /// - Creates a new external user if none exists
    /// - Assigns the Visitor role to new users
    /// - Returns the user with roles and permissions loaded
    /// </remarks>
    Task<UserEntity?> GetOrCreateExternalUserAsync(
        string email,
        string? userName,
        AuthProvider authProvider,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Sets a password for an external authentication user and changes their auth provider to Local.
    /// </summary>
    /// <param name="user">The user entity to set password for.</param>
    /// <param name="hashedPassword">The hashed password to set.</param>
    /// <exception cref="BadRequestException">Thrown when user already has Local AuthProvider.</exception>
    /// <exception cref="BadRequestException">Thrown when user auth provider is not Google or Facebook.</exception>
    /// <exception cref="BadRequestException">Thrown when the user doesn't have an email address.</exception>
    /// <remarks>
    /// This method validates that:
    /// - User doesn't already have Local auth (password not already set)
    /// - User's auth provider is Google or Facebook
    /// - User has an email address configured
    /// Upon success, sets the password hash and changes auth provider to Local.
    /// </remarks>
    void SetPasswordForExternalUser(UserEntity user, string hashedPassword);
}
