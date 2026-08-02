using System.Security.Claims;
using _116.BuildingBlocks.Constants.RateLimit;
using _116.Content.Application.Editorial.Constants;
using _116.Content.Application.Shared.DTOs;
using _116.Content.Domain.Constants;
using _116.Identity.Contracts.Application;
using _116.Shared.Application.Extensions;
using _116.Shared.Contracts.Application.CQRS;
using Carter;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace _116.Content.Application.Editorial.UseCases.Public.Queries.GetSimilarLyrics.V1;

/// <summary>
/// Response model for retrieving similar lyrics pages.
/// </summary>
/// <param name="Lyrics">The matched similar lyrics pages, or an empty list.</param>
public record PublicGetSimilarLyricsResponse(IReadOnlyList<LyricsSummaryDto> Lyrics);

/// <summary>
/// Defines the public get similar lyrics endpoint.
/// Returns lyrics pages similar to the given lyrics page.
/// </summary>
public class PublicGetSimilarLyricsEndpointV1 : ICarterModule
{
    /// <summary>
    /// Configures the similar lyrics retrieval route within the API pipeline.
    /// Maps the <c>GET /api/v1/public/lyrics/{id}/similar</c> endpoint.
    /// </summary>
    /// <param name="app">The route builder used to register API endpoints.</param>
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        RouteGroupBuilder group = app.MapApiVersionGroup(1)
            .MapGroup($"{ContentConstants.Public}/{EditorialRouteConstants.Lyrics}")
            .WithTags($"{ContentConstants.Public}::{EditorialRouteConstants.Lyrics}");

        group
            .MapGet(
                $"/{{id}}/{EditorialRouteConstants.Similar}",
                async (string id, ClaimsPrincipal user, IClaimsProvider claimsProvider, IDispatcher dispatcher) =>
                {
                    Guid lyricsId = Guid.Parse(id);
                    Guid? userId = null;

                    if (user.Identity?.IsAuthenticated == true)
                    {
                        userId = claimsProvider.GetUserIdFromClaims(user: user);
                    }

                    var query = new PublicGetSimilarLyricsQuery(LyricsId: lyricsId, CurrentUserId: userId);

                    PublicGetSimilarLyricsResult result = await dispatcher.Send(request: query);

                    var response = new PublicGetSimilarLyricsResponse(Lyrics: result.Lyrics);
                    return Results.Ok(response);
                }
            )
            .WithName(endpointName: PublicGetSimilarLyricsMetaField.GetSimilarLyrics.Name)
            .WithSummary(summary: PublicGetSimilarLyricsMetaField.GetSimilarLyrics.Summary)
            .WithDescription(description: PublicGetSimilarLyricsMetaField.GetSimilarLyrics.Description)
            .AllowAnonymous()
            .RequireRateLimiting(policyName: RateLimitPolicies.ContentBrowsing)
            .Produces<PublicGetSimilarLyricsResponse>(statusCode: StatusCodes.Status200OK)
            .ProducesProblem(statusCode: StatusCodes.Status404NotFound)
            .ProducesProblem(statusCode: StatusCodes.Status429TooManyRequests);
    }
}
