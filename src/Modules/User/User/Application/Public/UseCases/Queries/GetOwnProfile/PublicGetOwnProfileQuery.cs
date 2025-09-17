using _116.Shared.Contracts.Application.CQRS;
using _116.User.Domain.DTOs;

namespace _116.User.Application.Public.UseCases.Queries.GetOwnProfile;

/// <summary>
/// Query for retrieving the authenticated user's complete profile information.
/// </summary>
/// <param name="UserId">The unique identifier of the user to retrieve.</param>
/// <remarks>
/// This query retrieves the complete user profile including roles, permissions, and file information.
/// Only authenticated users can access their own profile information.
/// The user ID is extracted from the JWT token at the endpoint level.
/// </remarks>
public record PublicGetOwnProfileQuery(
    Guid UserId
) : IQuery<PublicGetOwnProfileResult>;

/// <summary>
/// Result of the <see cref="PublicGetOwnProfileQuery"/> containing complete user profile data.
/// </summary>
/// <param name="User">The complete user profile information including roles and permissions.</param>
/// <remarks>
/// Contains the full UserResponseDto with all user details, roles, permissions, and avatar information.
/// This information is used by the client application to display user profile and manage permissions.
/// </remarks>
public record PublicGetOwnProfileResult(
    UserResponseDto User
);
