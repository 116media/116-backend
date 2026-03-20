using System.Security.Claims;
using _116.BuildingBlocks.Constants.Authorization.Policies;
using _116.BuildingBlocks.Constants.RateLimit;
using _116.Content.Application.Interactions.Constants;
using _116.Content.Domain.Constants;
using _116.Identity.Contracts.Application;
using _116.Shared.Application.Extensions;
using _116.Shared.Contracts.Application.CQRS;
using Carter;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace _116.Content.Application.Interactions.UseCases.Public.Commands.AddVideoToPlaylist.V1;

/// <summary>
/// Request body for adding a video to a playlist.
/// </summary>
/// <param name="VideoId">The unique identifier of the video to add.</param>
/// <param name="SortOrder">The display order for this video within the playlist.</param>
public record PublicAddVideoToPlaylistRequest(Guid VideoId, int SortOrder);

/// <summary>
/// Response model for a successful PublicAddVideoToPlaylist operation.
/// </summary>
/// <param name="IsSuccess">Indicates if the operation was successful.</param>
public record PublicAddVideoToPlaylistResponse(bool IsSuccess);

/// <summary>
/// Defines the add video to playlist endpoint.
/// </summary>
public class PublicAddVideoToPlaylistEndpointV1 : ICarterModule
{
    /// <inheritdoc />
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        RouteGroupBuilder group = app.MapApiVersionGroup(1)
            .MapGroup($"{ContentConstants.Public}/{InteractionsRouteConstants.Playlists}")
            .WithTags($"{ContentConstants.Public}::{InteractionsRouteConstants.Playlists}");

        group
            .MapPost(
                $"/{{id}}/{InteractionsRouteConstants.PlaylistVideos}",
                async (
                    string id,
                    PublicAddVideoToPlaylistRequest request,
                    ClaimsPrincipal user,
                    IClaimsProvider claimsProvider,
                    IDispatcher dispatcher
                ) =>
                {
                    Guid playlistId = Guid.Parse(id);
                    Guid userId = claimsProvider.GetUserIdFromClaims(user: user);

                    var command = new PublicAddVideoToPlaylistCommand(
                        PlaylistId: playlistId,
                        VideoId: request.VideoId,
                        UserId: userId,
                        SortOrder: request.SortOrder
                    );

                    PublicAddVideoToPlaylistResult result = await dispatcher.Send(request: command);

                    var response = new PublicAddVideoToPlaylistResponse(IsSuccess: result.IsSuccess);
                    return Results.Ok(response);
                }
            )
            .WithName(endpointName: PublicAddVideoToPlaylistMetaField.PublicAddVideoToPlaylist.Name)
            .WithSummary(summary: PublicAddVideoToPlaylistMetaField.PublicAddVideoToPlaylist.Summary)
            .WithDescription(description: PublicAddVideoToPlaylistMetaField.PublicAddVideoToPlaylist.Description)
            .WithAuthorization(UserRolePolicies.RequireVisitorOnly)
            .RequireRateLimiting(policyName: RateLimitPolicies.ContentBrowsing)
            .Produces<PublicAddVideoToPlaylistResponse>(statusCode: StatusCodes.Status200OK)
            .ProducesProblem(statusCode: StatusCodes.Status400BadRequest)
            .ProducesProblem(statusCode: StatusCodes.Status401Unauthorized)
            .ProducesProblem(statusCode: StatusCodes.Status404NotFound)
            .ProducesProblem(statusCode: StatusCodes.Status409Conflict)
            .ProducesProblem(statusCode: StatusCodes.Status429TooManyRequests);
    }
}
