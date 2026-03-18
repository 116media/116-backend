using System.Security.Claims;
using _116.BuildingBlocks.Constants.Authorization.Policies;
using _116.BuildingBlocks.Constants.RateLimit;
using _116.BuildingBlocks.Utils;
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

namespace _116.Content.Application.Interactions.UseCases.Public.Commands.CreatePlaylist.V1;

/// <summary>
/// Request body for creating a playlist.
/// </summary>
/// <param name="Name">The display name for the playlist.</param>
public record PublicCreatePlaylistRequest(string Name);

/// <summary>
/// Response model for a successful PublicCreatePlaylist operation.
/// </summary>
/// <param name="Playlist">The newly created playlist DTO.</param>
public record PublicCreatePlaylistResponse(PlaylistDto Playlist);

/// <summary>
/// Defines the create playlist endpoint.
/// </summary>
public class PublicCreatePlaylistEndpointV1 : ICarterModule
{
    /// <inheritdoc />
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        RouteGroupBuilder group = app.MapApiVersionGroup(1)
            .MapGroup($"{ContentConstants.Public}/{InteractionsRouteConstants.Playlists}")
            .WithTags($"{ContentConstants.Public}::{InteractionsRouteConstants.Playlists}");

        group
            .MapPost(
                "/",
                async (
                    PublicCreatePlaylistRequest request,
                    ClaimsPrincipal user,
                    IClaimsProvider claimsProvider,
                    IDispatcher dispatcher,
                    HttpContext httpContext
                ) =>
                {
                    Guid userId = claimsProvider.GetUserIdFromClaims(user: user);
                    var command = new PublicCreatePlaylistCommand(UserId: userId, Name: request.Name);
                    PublicCreatePlaylistResult result = await dispatcher.Send(request: command);

                    var response = new PublicCreatePlaylistResponse(Playlist: result.Playlist);

                    Guid playlistId = result.Playlist.Id;
                    string path = $"{ContentConstants.Public}/{InteractionsRouteConstants.Playlists}/{playlistId}";
                    string locationUrl = ApiVersionUrl.Build(context: httpContext, path: path);

                    return Results.Created(uri: locationUrl, value: response);
                }
            )
            .WithName(endpointName: PublicCreatePlaylistMetaField.PublicCreatePlaylist.Name)
            .WithSummary(summary: PublicCreatePlaylistMetaField.PublicCreatePlaylist.Summary)
            .WithDescription(description: PublicCreatePlaylistMetaField.PublicCreatePlaylist.Description)
            .WithAuthorization(UserRolePolicies.RequireVisitorOnly)
            .RequireRateLimiting(policyName: RateLimitPolicies.ContentBrowsing)
            .Produces<PublicCreatePlaylistResponse>(statusCode: StatusCodes.Status201Created)
            .ProducesProblem(statusCode: StatusCodes.Status400BadRequest)
            .ProducesProblem(statusCode: StatusCodes.Status401Unauthorized)
            .ProducesProblem(statusCode: StatusCodes.Status429TooManyRequests);
    }
}
