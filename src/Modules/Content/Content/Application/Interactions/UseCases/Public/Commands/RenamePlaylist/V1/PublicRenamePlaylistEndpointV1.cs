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

namespace _116.Content.Application.Interactions.UseCases.Public.Commands.RenamePlaylist.V1;

/// <summary>
/// Request body for renaming a playlist.
/// </summary>
/// <param name="Name">The new display name for the playlist.</param>
public record PublicRenamePlaylistRequest(string Name);

/// <summary>
/// Response model for a successful PublicRenamePlaylist operation.
/// </summary>
/// <param name="IsSuccess">Indicates if the operation was successful.</param>
public record PublicRenamePlaylistResponse(bool IsSuccess);

/// <summary>
/// Defines the rename playlist endpoint.
/// </summary>
public class PublicRenamePlaylistEndpointV1 : ICarterModule
{
    /// <inheritdoc />
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        RouteGroupBuilder group = app.MapApiVersionGroup(1)
            .MapGroup($"{ContentConstants.Public}/{InteractionsRouteConstants.Playlists}")
            .WithTags($"{ContentConstants.Public}::{InteractionsRouteConstants.Playlists}");

        group
            .MapPut(
                "/{id}",
                async (
                    string id,
                    PublicRenamePlaylistRequest request,
                    ClaimsPrincipal user,
                    IClaimsProvider claimsProvider,
                    IDispatcher dispatcher
                ) =>
                {
                    Guid playlistId = Guid.Parse(id);
                    Guid userId = claimsProvider.GetUserIdFromClaims(user: user);

                    var command = new PublicRenamePlaylistCommand(Id: playlistId, UserId: userId, Name: request.Name);
                    PublicRenamePlaylistResult result = await dispatcher.Send(request: command);

                    var response = new PublicRenamePlaylistResponse(IsSuccess: result.IsSuccess);
                    return Results.Ok(response);
                }
            )
            .WithName(endpointName: PublicRenamePlaylistMetaField.PublicRenamePlaylist.Name)
            .WithSummary(summary: PublicRenamePlaylistMetaField.PublicRenamePlaylist.Summary)
            .WithDescription(description: PublicRenamePlaylistMetaField.PublicRenamePlaylist.Description)
            .WithAuthorization(UserRolePolicies.RequireVisitorOnly)
            .RequireRateLimiting(policyName: RateLimitPolicies.ContentBrowsing)
            .Produces<PublicRenamePlaylistResponse>(statusCode: StatusCodes.Status200OK)
            .ProducesProblem(statusCode: StatusCodes.Status400BadRequest)
            .ProducesProblem(statusCode: StatusCodes.Status401Unauthorized)
            .ProducesProblem(statusCode: StatusCodes.Status404NotFound)
            .ProducesProblem(statusCode: StatusCodes.Status429TooManyRequests);
    }
}
