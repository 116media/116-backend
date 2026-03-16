using System.Security.Claims;
using _116.BuildingBlocks.Constants.Authorization.Policies;
using _116.BuildingBlocks.Constants.RateLimit;
using _116.Identity.Application.Shared.DTOs;
using _116.Identity.Application.User.Constants;
using _116.Identity.Contracts.Application;
using _116.Identity.Domain.Constants;
using _116.Shared.Application.Extensions;
using _116.Shared.Contracts.Application.CQRS;
using Carter;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace _116.Identity.Application.User.UseCases.Public.Queries.GetOwnProfile.V1;

/// <summary>
/// Response model for user profile.
/// </summary>
/// <param name="User">The complete user profile information including roles and permissions.</param>
public record PublicGetOwnProfileResponse(UserResponseDto User);

/// <summary>
/// Defines the user profile endpoint for authenticated public users.
/// Handles retrieval of complete user profile information.
/// </summary>
public class PublicGetOwnProfileEndpointV1 : ICarterModule
{
    /// <summary>
    /// Configures the user profile route within the API pipeline.
    /// Maps the <c>/api/v1/public/me/profile</c> endpoint to handle profile retrieval requests.
    /// </summary>
    /// <param name="app">The route builder used to register API endpoints.</param>
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        RouteGroupBuilder group = app.MapApiVersionGroup(1)
            .MapGroup($"{IdentityConstants.Public}/{IdentityConstants.Me}")
            .WithTags($"{IdentityConstants.Public}::{IdentityConstants.Me}");

        group
            .MapGet(
                pattern: UserRouteConstants.Profile,
                async (ClaimsPrincipal user, IClaimsProvider authProvider, IDispatcher dispatcher) =>
                {
                    Guid userId = authProvider.GetUserIdFromClaims(user: user);

                    var query = new PublicGetOwnProfileQuery(UserId: userId);
                    PublicGetOwnProfileResult result = await dispatcher.Send(request: query);

                    var response = new PublicGetOwnProfileResponse(User: result.User);

                    return Results.Ok(value: response);
                }
            )
            .WithName(endpointName: PublicGetOwnProfileMetaField.GetOwnProfile.Name)
            .WithSummary(summary: PublicGetOwnProfileMetaField.GetOwnProfile.Summary)
            .WithDescription(description: PublicGetOwnProfileMetaField.GetOwnProfile.Description)
            .WithAuthorization(UserRolePolicies.RequireVisitorOnly)
            .RequireRateLimiting(policyName: RateLimitPolicies.UserProfile)
            .Produces<PublicGetOwnProfileResponse>()
            .ProducesProblem(statusCode: StatusCodes.Status401Unauthorized)
            .ProducesProblem(statusCode: StatusCodes.Status403Forbidden)
            .ProducesProblem(statusCode: StatusCodes.Status404NotFound)
            .ProducesProblem(statusCode: StatusCodes.Status429TooManyRequests);
    }
}
