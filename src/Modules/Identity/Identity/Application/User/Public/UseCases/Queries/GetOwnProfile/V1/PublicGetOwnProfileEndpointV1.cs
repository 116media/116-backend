using System.Security.Claims;

using _116.Identity.Application.Shared.Authorizations.Policies;
using _116.Identity.Application.Shared.Constants;
using _116.Identity.Application.Shared.DTOs;
using _116.Identity.Application.Shared.Repositories;
using _116.Identity.Domain.Constants;
using _116.Shared.Application.Extensions;
using _116.Shared.Contracts.Application.CQRS;

using Carter;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace _116.Identity.Application.User.Public.UseCases.Queries.GetOwnProfile.V1;

/// <summary>
/// Response model for user profile.
/// </summary>
/// <param name="User">The complete user profile information including roles and permissions.</param>
public record PublicGetOwnProfileResponse(
    UserResponseDto User
);
/// <summary>
/// Defines the user profile endpoint for authenticated public users.
/// Handles retrieval of complete user profile information.
/// </summary>
public class PublicGetOwnProfileEndpointV1 : ICarterModule
{
    /// <summary>
    /// Configures the user profile route within the API pipeline.
    /// Maps the <c>/api/v1/public/user/profile</c> endpoint to handle profile retrieval requests.
    /// </summary>
    /// <param name="app">The route builder used to register API endpoints.</param>
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        RouteGroupBuilder group = app
            .MapApiVersionGroup(version: 1)
            .MapGroup($"{IdentityConstants.Public}/{AuthRouteConstants.Profile}")
            .WithTags($"{IdentityConstants.Public}::{AuthRouteConstants.Profile}");
        group.MapGet("/", async (
                ClaimsPrincipal user,
                IUserRepository userRepository,
                IDispatcher dispatcher
            ) =>
            {
                // Extract user ID from JWT token claims
                Guid userId = userRepository.GetUserIdFromClaims(user);
                // Send the query to get user profile
                var query = new PublicGetOwnProfileQuery(userId);
                PublicGetOwnProfileResult result = await dispatcher.Send(query);
                // Adapt the result to the response type
                var response = new PublicGetOwnProfileResponse(
                    result.User
                );
                return Results.Ok(response);
            })
            .WithName(PublicGetOwnProfileMetaField.GetOwnProfile.Name)
            .WithSummary(PublicGetOwnProfileMetaField.GetOwnProfile.Summary)
            .WithDescription(PublicGetOwnProfileMetaField.GetOwnProfile.Description)
            .RequireAuthorization(UserRolePolicies.RequireVisitorOnly)
            .RequireAuthorization(AccountStatusPolicies.RequireLoggedInUser)
            .Produces<PublicGetOwnProfileResponse>()
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .ProducesProblem(StatusCodes.Status404NotFound);
    }
}
