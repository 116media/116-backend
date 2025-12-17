using _116.Shared.Contracts.Application.CQRS;
using _116.Auth.Domain.DTOs;

namespace _116.Auth.Application.Admin.UseCases.Queries.GetOwnProfile;

/// <summary>
/// Query for retrieving the authenticated admin user's complete profile information.
/// </summary>
/// <param name="UserId">The unique identifier of the admin user to retrieve.</param>
/// <remarks>
/// This query retrieves the complete admin user profile including roles, permissions, and file information.
/// Only authenticated admin users can access their own profile information.
/// The user ID is extracted from the JWT token at the endpoint level.
/// </remarks>
public record AdminGetOwnProfileQuery(
    Guid UserId
) : IQuery<AdminGetOwnProfileResult>;

/// <summary>
/// Result of the <see cref="AdminGetOwnProfileQuery"/> containing complete admin user profile data.
/// </summary>
/// <param name="User">The complete admin user profile information including roles and permissions.</param>
/// <remarks>
/// Contains the full UserResponseDto with all admin user details, roles, permissions, and avatar information.
/// This information is used by the admin application to display user profile and manage permissions.
/// </remarks>
public record AdminGetOwnProfileResult(
    UserResponseDto User
);
