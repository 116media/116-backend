using _116.BuildingBlocks.Constants.RateLimit;
using _116.Content.Application.Editorial.Constants;
using _116.Content.Application.Shared.DTOs;
using _116.Content.Domain.Constants;
using _116.Shared.Application.Extensions;
using _116.Shared.Application.Pagination;
using _116.Shared.Contracts.Application.CQRS;
using Carter;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace _116.Content.Application.Editorial.UseCases.Public.Queries.GetArtistBySlug.V1;

/// <summary>
/// Response model for retrieving an artist's public profile page by slug.
/// </summary>
/// <param name="Artist">The matched artist profile information.</param>
/// <param name="Lyrics">The artist's paginated published lyrics pages.</param>
/// <param name="Videos">The artist's paginated published videos.</param>
public record PublicGetArtistBySlugResponse(
    ArtistDto Artist,
    PaginatedResult<LyricsSummaryDto> Lyrics,
    PaginatedResult<VideoSummaryDto> Videos
);

/// <summary>
/// Defines the public get artist by slug endpoint.
/// Returns an artist's public profile page matching the given URL-safe slug.
/// </summary>
public class PublicGetArtistBySlugEndpointV1 : ICarterModule
{
    /// <summary>
    /// Configures the artist by slug retrieval route within the API pipeline.
    /// Maps the <c>GET /api/v1/public/artists/{slug}</c> endpoint.
    /// </summary>
    /// <param name="app">The route builder used to register API endpoints.</param>
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        RouteGroupBuilder group = app.MapApiVersionGroup(1)
            .MapGroup($"{ContentConstants.Public}/{EditorialRouteConstants.Artists}")
            .WithTags($"{ContentConstants.Public}::{EditorialRouteConstants.Artists}");

        group
            .MapGet(
                "/{slug}",
                async (
                    string slug,
                    IDispatcher dispatcher,
                    int lyricsPageIndex = 0,
                    int lyricsPageSize = 10,
                    int videosPageIndex = 0,
                    int videosPageSize = 10
                ) =>
                {
                    var query = new PublicGetArtistBySlugQuery(
                        Slug: slug,
                        LyricsPage: new PaginatedRequest(lyricsPageIndex, lyricsPageSize),
                        VideosPage: new PaginatedRequest(videosPageIndex, videosPageSize)
                    );

                    PublicGetArtistBySlugResult result = await dispatcher.Send(request: query);

                    var response = new PublicGetArtistBySlugResponse(
                        Artist: result.Artist,
                        Lyrics: result.Lyrics,
                        Videos: result.Videos
                    );
                    return Results.Ok(response);
                }
            )
            .WithName(endpointName: PublicGetArtistBySlugMetaField.GetArtistBySlug.Name)
            .WithSummary(summary: PublicGetArtistBySlugMetaField.GetArtistBySlug.Summary)
            .WithDescription(description: PublicGetArtistBySlugMetaField.GetArtistBySlug.Description)
            .AllowAnonymous()
            .RequireRateLimiting(policyName: RateLimitPolicies.ContentBrowsing)
            .Produces<PublicGetArtistBySlugResponse>(statusCode: StatusCodes.Status200OK)
            .ProducesProblem(statusCode: StatusCodes.Status404NotFound)
            .ProducesProblem(statusCode: StatusCodes.Status429TooManyRequests);
    }
}
