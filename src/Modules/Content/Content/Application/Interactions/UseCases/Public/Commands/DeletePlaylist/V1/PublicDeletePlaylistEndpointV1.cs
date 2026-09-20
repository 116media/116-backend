using System.Security.Claims;
using _116.BuildingBlocks.Constants.Authorization.Policies;
using _116.BuildingBlocks.Constants.RateLimit;
using _116.Content.Application.Interactions.Constants;
using _116.Content.Domain.Constants;
using _116.Identity.Contracts.Application.Services;
using _116.Shared.Application.Extensions;
using _116.Shared.Contracts.Application.CQRS;
using Carter;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace _116.Content.Application.Interactions.UseCases.Public.Commands.DeletePlaylist.V1;

/// <summary>
/// Defines the delete playlist endpoint.
/// </summary>
public class PublicDeletePlaylistEndpointV1 : ICarterModule
{
    /// <inheritdoc />
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        RouteGroupBuilder group = app.MapApiVersionGroup(1)
            .MapGroup($"{ContentConstants.Public}/{InteractionsRouteConstants.Playlists}")
            .WithTags($"{ContentConstants.Public}::{InteractionsRouteConstants.Playlists}");

        group
            .MapDelete(
                "/{id}",
                async (
                    string id,
                    ClaimsPrincipal user,
                    IClaimsProvider claimsProvider,
                    IDispatcher dispatcher,
                    CancellationToken cancellationToken
                ) =>
                {
                    Guid playlistId = Guid.Parse(id);
                    Guid userId = claimsProvider.GetUserIdFromClaims(user: user);

                    var command = new PublicDeletePlaylistCommand(Id: playlistId, UserId: userId);
                    await dispatcher.Send(request: command, cancellationToken: cancellationToken);
                    return Results.NoContent();
                }
            )
            .WithName(endpointName: PublicDeletePlaylistMetaField.DeletePlaylist.Name)
            .WithSummary(summary: PublicDeletePlaylistMetaField.DeletePlaylist.Summary)
            .WithDescription(description: PublicDeletePlaylistMetaField.DeletePlaylist.Description)
            .WithAuthorization(UserRolePolicies.RequireVisitorOnly)
            .RequireRateLimiting(policyName: RateLimitPolicies.ContentContribution)
            .Produces(statusCode: StatusCodes.Status204NoContent)
            .ProducesProblem(statusCode: StatusCodes.Status400BadRequest)
            .ProducesProblem(statusCode: StatusCodes.Status401Unauthorized)
            .ProducesProblem(statusCode: StatusCodes.Status404NotFound)
            .ProducesProblem(statusCode: StatusCodes.Status429TooManyRequests);
    }
}
