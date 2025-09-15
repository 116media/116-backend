using _116.BuildingBlocks.Constants;
using _116.Shared.Contracts.Application.CQRS;
using _116.User.Application.Shared.Authorizations.Policies;
using _116.User.Application.Shared.Errors;
using _116.User.Domain.DTOs;
using Carter;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using System.Security.Claims;

namespace _116.User.Application.Public.UseCases.Queries.GetUserProfile;

/// <summary>
/// Response model for user profile.
/// </summary>
/// <param name="User">The complete user profile information including roles and permissions.</param>
public record PublicGetUserProfileResponse(
    UserResponseDto User
);

/// <summary>
/// Defines the user profile endpoint for authenticated public users.
/// Handles retrieval of complete user profile information.
/// </summary>
public class PublicGetUserProfileEndpoint : ICarterModule
{
    /// <summary>
    /// Configures the user profile route within the API pipeline.
    /// Maps the <c>/api/v1/public/user/profile</c> endpoint to handle profile retrieval requests.
    /// </summary>
    /// <param name="app">The route builder used to register API endpoints.</param>
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        RouteGroupBuilder group = app
            .MapGroup(RouteConstants.V1.Public.Profile)
            .WithTags("Public::authentication");

        group.MapGet("/", async (ClaimsPrincipal user, IDispatcher dispatcher) =>
            {
                // Extract user ID from JWT token claims
                string? userIdClaim = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out Guid userId))
                {
                    throw UserErrors.InvalidUserAuthentication();
                }

                // Send the query to get user profile
                var query = new PublicGetUserProfileQuery(userId);
                PublicGetUserProfileResult result = await dispatcher.Send(query);

                // Adapt the result to the response type
                var response = new PublicGetUserProfileResponse(
                    result.User
                );

                return Results.Ok(response);
            })
            .WithName(PublicGetUserProfileMetaField.GetUserProfile.Name)
            .WithSummary(PublicGetUserProfileMetaField.GetUserProfile.Summary)
            .WithDescription(PublicGetUserProfileMetaField.GetUserProfile.Description)
            .RequireAuthorization(UserRolePolicies.RequireVisitorOnly)
            .Produces<PublicGetUserProfileResponse>()
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .ProducesProblem(StatusCodes.Status404NotFound);
    }
}
