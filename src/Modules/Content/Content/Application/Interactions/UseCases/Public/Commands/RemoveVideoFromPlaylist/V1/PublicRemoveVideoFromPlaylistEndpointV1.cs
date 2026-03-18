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

namespace _116.Content.Application.Interactions.UseCases.Public.Commands.RemoveVideoFromPlaylist.V1;

/// <summary>
/// Response model for a successful PublicRemoveVideoFromPlaylist operation.
/// </summary>
/// <param name="IsSuccess">Indicates if the operation was successful.</param>
public record PublicRemoveVideoFromPlaylistResponse(bool IsSuccess);

/// <summary>
/// Defines the remove video from playlist endpoint.
/// </summary>
public class PublicRemoveVideoFromPlaylistEndpointV1 : ICarterModule
{
    /// <inheritdoc />
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        RouteGroupBuilder group = app.MapApiVersionGroup(1)
            .MapGroup($"{ContentConstants.Public}/{InteractionsRouteConstants.Playlists}")
            .WithTags($"{ContentConstants.Public}::{InteractionsRouteConstants.Playlists}");

        group
            .MapDelete(
                $"/{{id}}/{InteractionsRouteConstants.PlaylistVideos}/{{videoId}}",
                async (
                    string id,
                    string videoId,
                    ClaimsPrincipal user,
                    IClaimsProvider claimsProvider,
                    IDispatcher dispatcher
                ) =>
                {
                    Guid playlistId = Guid.Parse(id);
                    Guid parsedVideoId = Guid.Parse(videoId);
                    Guid userId = claimsProvider.GetUserIdFromClaims(user: user);
                    var command = new PublicRemoveVideoFromPlaylistCommand(
                        PlaylistId: playlistId,
                        VideoId: parsedVideoId,
                        UserId: userId
                    );
                    PublicRemoveVideoFromPlaylistResult result = await dispatcher.Send(request: command);

                    var response = new PublicRemoveVideoFromPlaylistResponse(IsSuccess: result.IsSuccess);
                    return Results.Ok(response);
                }
            )
            .WithName(endpointName: PublicRemoveVideoFromPlaylistMetaField.PublicRemoveVideoFromPlaylist.Name)
            .WithSummary(summary: PublicRemoveVideoFromPlaylistMetaField.PublicRemoveVideoFromPlaylist.Summary)
            .WithDescription(
                description: PublicRemoveVideoFromPlaylistMetaField.PublicRemoveVideoFromPlaylist.Description
            )
            .WithAuthorization(UserRolePolicies.RequireVisitorOnly)
            .RequireRateLimiting(policyName: RateLimitPolicies.ContentBrowsing)
            .Produces<PublicRemoveVideoFromPlaylistResponse>(statusCode: StatusCodes.Status200OK)
            .ProducesProblem(statusCode: StatusCodes.Status400BadRequest)
            .ProducesProblem(statusCode: StatusCodes.Status401Unauthorized)
            .ProducesProblem(statusCode: StatusCodes.Status404NotFound)
            .ProducesProblem(statusCode: StatusCodes.Status429TooManyRequests);
    }
}
