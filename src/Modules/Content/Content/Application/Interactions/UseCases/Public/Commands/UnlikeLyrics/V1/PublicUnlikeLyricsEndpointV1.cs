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

namespace _116.Content.Application.Interactions.UseCases.Public.Commands.UnlikeLyrics.V1;

/// <summary>
/// Defines the unlike lyrics endpoint.
/// </summary>
public class PublicUnlikeLyricsEndpointV1 : ICarterModule
{
    /// <inheritdoc />
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        RouteGroupBuilder group = app.MapApiVersionGroup(1)
            .MapGroup($"{ContentConstants.Public}/{InteractionsRouteConstants.Lyrics}")
            .WithTags($"{ContentConstants.Public}::{InteractionsRouteConstants.Lyrics}");

        group
            .MapDelete(
                $"/{{id}}/{InteractionsRouteConstants.Likes}",
                async (
                    string id,
                    ClaimsPrincipal user,
                    IClaimsProvider claimsProvider,
                    IDispatcher dispatcher,
                    CancellationToken cancellationToken
                ) =>
                {
                    Guid lyricsId = Guid.Parse(id);
                    Guid userId = claimsProvider.GetUserIdFromClaims(user: user);

                    var command = new PublicUnlikeLyricsCommand(LyricsId: lyricsId, UserId: userId);
                    await dispatcher.Send(request: command, cancellationToken: cancellationToken);
                    return Results.NoContent();
                }
            )
            .WithName(endpointName: PublicUnlikeLyricsMetaField.UnlikeLyrics.Name)
            .WithSummary(summary: PublicUnlikeLyricsMetaField.UnlikeLyrics.Summary)
            .WithDescription(description: PublicUnlikeLyricsMetaField.UnlikeLyrics.Description)
            .WithAuthorization(UserRolePolicies.RequireVisitorOnly)
            .RequireRateLimiting(policyName: RateLimitPolicies.ContentContribution)
            .Produces(statusCode: StatusCodes.Status204NoContent)
            .ProducesProblem(statusCode: StatusCodes.Status400BadRequest)
            .ProducesProblem(statusCode: StatusCodes.Status401Unauthorized)
            .ProducesProblem(statusCode: StatusCodes.Status404NotFound)
            .ProducesProblem(statusCode: StatusCodes.Status429TooManyRequests);
    }
}
