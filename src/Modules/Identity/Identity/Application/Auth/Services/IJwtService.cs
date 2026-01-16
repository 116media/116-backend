using _116.Identity.Domain.Entities;
using _116.Identity.Domain.Enums;
using _116.Identity.Domain.Results;

namespace _116.Identity.Application.Auth.Services;

/// <summary>
/// Service interface for JWT token generation and management.
/// Provides methods to create JWT tokens with user authentication and authorization claims.
/// </summary>
public interface IJwtService
{
    /// <summary>
    /// Generates a JWT token with expiration information containing user identity,
    /// roles, permissions, and status information.
    /// </summary>
    /// <param name="userId">The unique identifier of the user.</param>
    /// <param name="sessionId">The unique identifier of the session.</param>
    /// <param name="email">The user's email address.</param>
    /// <param name="userName">The user's unique username.</param>
    /// <param name="userRoles">Collection of roles assigned to the user for authorization.</param>
    /// <param name="userPermissions">Collection of permissions granted to the user through roles.</param>
    /// <param name="isVerified">Indicates whether the user's email/account has been verified.</param>
    /// <param name="isActive">Indicates whether the user account is currently active.</param>
    /// <param name="authProvider">The authentication provider used by the user (Local, Google, Facebook, etc.).</param>
    /// <returns>A JWT generation result containing both the token and its expiration time.</returns>
    /// <remarks>
    /// This provides both the JWT token and its expiration time for creating complete authentication results.
    /// </remarks>
    JwtGenerationResult GenerateToken(
        Guid userId,
        Guid sessionId,
        string email,
        string userName,
        ICollection<UserRoleEntity> userRoles,
        ICollection<RolePermissionEntity> userPermissions,
        bool isVerified,
        bool isActive,
        EnumAuthProvider authProvider
    );
}
