using System.Security.Claims;
using _116.BuildingBlocks.Constants.Authorization.Policies;
using _116.BuildingBlocks.Constants.RateLimit;
using _116.Content.Application.Interactions.Constants;
using _116.Content.Application.Shared.DTOs;
using _116.Content.Domain.Constants;
using _116.Identity.Contracts.Application;
using _116.Shared.Application.Extensions;
using _116.Shared.Contracts.Application.CQRS;
using Carter;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace _116.Content.Application.Interactions.UseCases.Public.Queries.GetMyPlaylists.V1;

/// <summary>
/// Defines the endpoint to get my playlists.
/// </summary>
public class PublicGetMyPlaylistsEndpointV1 : ICarterModule
{
    /// <inheritdoc />
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        RouteGroupBuilder group = app.MapApiVersionGroup(1)
            .MapGroup($"{ContentConstants.Public}/{InteractionsRouteConstants.Playlists}")
            .WithTags($"{ContentConstants.Public}::{InteractionsRouteConstants.Playlists}");

        group
            .MapGet(
                "/",
                async (ClaimsPrincipal user, IClaimsProvider claimsProvider, IDispatcher dispatcher) =>
                {
                    Guid userId = claimsProvider.GetUserIdFromClaims(user: user);
                    var query = new PublicGetMyPlaylistsQuery(UserId: userId);

                    PublicGetMyPlaylistsResult result = await dispatcher.Send(request: query);
                    return Results.Ok(result.Playlists);
                }
            )
            .WithName(endpointName: PublicGetMyPlaylistsMetaField.PublicGetMyPlaylists.Name)
            .WithSummary(summary: PublicGetMyPlaylistsMetaField.PublicGetMyPlaylists.Summary)
            .WithDescription(description: PublicGetMyPlaylistsMetaField.PublicGetMyPlaylists.Description)
            .WithAuthorization(UserRolePolicies.RequireVisitorOnly)
            .RequireRateLimiting(policyName: RateLimitPolicies.ContentBrowsing)
            .Produces<IReadOnlyList<PlaylistDto>>(statusCode: StatusCodes.Status200OK)
            .ProducesProblem(statusCode: StatusCodes.Status401Unauthorized)
            .ProducesProblem(statusCode: StatusCodes.Status429TooManyRequests);
    }
}
