using System.Security.Claims;
using _116.BuildingBlocks.Constants.Authorization.Policies;
using _116.BuildingBlocks.Constants.RateLimit;
using _116.Content.Application.Editorial.Constants;
using _116.Identity.Contracts.Application;
using _116.Shared.Application.Extensions;
using _116.Shared.Contracts.Application.CQRS;
using Carter;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace _116.Content.Application.Editorial.UseCases.Public.Commands.RequestArtistClaim.V1;

/// <summary>
/// Response model for a successful artist claim request.
/// </summary>
/// <param name="IsSuccess">Indicates if the request was recorded successfully.</param>
public record PublicRequestArtistClaimResponse(bool IsSuccess);

/// <summary>
/// Defines the public request artist claim endpoint.
/// Handles authenticated users requesting ownership of an artist profile.
/// </summary>
public class PublicRequestArtistClaimEndpointV1 : ICarterModule
{
    /// <summary>
    /// Configures the artist claim request route within the API pipeline.
    /// Maps the <c>POST /api/v1/artists/{id}/claim</c> endpoint.
    /// </summary>
    /// <param name="app">The route builder used to register API endpoints.</param>
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        RouteGroupBuilder group = app.MapApiVersionGroup(1)
            .MapGroup($"{EditorialRouteConstants.Artists}")
            .WithTags(EditorialRouteConstants.Artists);

        group
            .MapPost(
                $"/{{id}}/{EditorialRouteConstants.Claim}",
                async (Guid id, ClaimsPrincipal user, IClaimsProvider claimsProvider, IDispatcher dispatcher) =>
                {
                    Guid userId = claimsProvider.GetUserIdFromClaims(user: user);

                    var command = new PublicRequestArtistClaimCommand(ArtistId: id, UserId: userId);
                    PublicRequestArtistClaimResult result = await dispatcher.Send(request: command);

                    var response = new PublicRequestArtistClaimResponse(IsSuccess: result.IsSuccess);
                    return Results.Ok(response);
                }
            )
            .WithName(endpointName: PublicRequestArtistClaimMetaField.RequestArtistClaim.Name)
            .WithSummary(summary: PublicRequestArtistClaimMetaField.RequestArtistClaim.Summary)
            .WithDescription(description: PublicRequestArtistClaimMetaField.RequestArtistClaim.Description)
            .WithAuthorization(AccountStatusPolicies.RequireActiveUser)
            .RequireRateLimiting(policyName: RateLimitPolicies.ContentBrowsing)
            .Produces<PublicRequestArtistClaimResponse>(statusCode: StatusCodes.Status200OK)
            .ProducesProblem(statusCode: StatusCodes.Status401Unauthorized)
            .ProducesProblem(statusCode: StatusCodes.Status404NotFound)
            .ProducesProblem(statusCode: StatusCodes.Status429TooManyRequests);
    }
}
