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

namespace _116.Content.Application.Editorial.UseCases.Public.Queries.GetLyricsByVideoId.V1;

/// <summary>
/// Response model for retrieving lyrics by video ID.
/// </summary>
/// <param name="Lyrics">The lyrics information linked to the video.</param>
public record PublicGetLyricsByVideoIdResponse(LyricsDetailDto Lyrics);

/// <summary>
/// Defines the public get lyrics by video ID endpoint.
/// Returns the lyrics page linked to the given video.
/// </summary>
public class PublicGetLyricsByVideoIdEndpointV1 : ICarterModule
{
    /// <summary>
    /// Configures the lyrics by video ID retrieval route within the API pipeline.
    /// Maps the <c>GET /api/v1/public/lyrics/video/{videoId}</c> endpoint.
    /// </summary>
    /// <param name="app">The route builder used to register API endpoints.</param>
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        RouteGroupBuilder group = app.MapApiVersionGroup(1)
            .MapGroup($"{ContentConstants.Public}/{EditorialRouteConstants.Lyrics}")
            .WithTags($"{ContentConstants.Public}::{EditorialRouteConstants.Lyrics}");

        group
            .MapGet(
                $"/{EditorialRouteConstants.Videos}/{{videoId}}",
                async (string videoId, ClaimsPrincipal user, IClaimsProvider claimsProvider, IDispatcher dispatcher) =>
                {
                    Guid? userId = null;

                    if (user.Identity?.IsAuthenticated == true)
                    {
                        userId = claimsProvider.GetUserIdFromClaims(user: user);
                    }

                    var query = new PublicGetLyricsByVideoIdQuery(VideoId: videoId, CurrentUserId: userId);

                    PublicGetLyricsByVideoIdResult result = await dispatcher.Send(request: query);

                    var response = new PublicGetLyricsByVideoIdResponse(Lyrics: result.Lyrics);
                    return Results.Ok(response);
                }
            )
            .WithName(endpointName: PublicGetLyricsByVideoIdMetaField.GetLyricsByVideoId.Name)
            .WithSummary(summary: PublicGetLyricsByVideoIdMetaField.GetLyricsByVideoId.Summary)
            .WithDescription(description: PublicGetLyricsByVideoIdMetaField.GetLyricsByVideoId.Description)
            .AllowAnonymous()
            .RequireRateLimiting(policyName: RateLimitPolicies.ContentBrowsing)
            .Produces<PublicGetLyricsByVideoIdResponse>(statusCode: StatusCodes.Status200OK)
            .ProducesProblem(statusCode: StatusCodes.Status404NotFound)
            .ProducesProblem(statusCode: StatusCodes.Status429TooManyRequests);
    }
}
