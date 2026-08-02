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

namespace _116.Content.Application.Editorial.UseCases.Public.Queries.GetLyricsBySlug.V1;

/// <summary>
/// Response model for retrieving lyrics by slug.
/// </summary>
/// <param name="Lyrics">The matched lyrics information.</param>
/// <param name="VideoSlug">
/// The slug of the linked video, or null if this lyrics page is standalone or the linked
/// video no longer exists.
/// </param>
/// <param name="ArtistSlug">
/// The slug of the linked artist profile, or null if this lyrics page has no linked
/// artist profile or the linked profile no longer exists.
/// </param>
/// <param name="AlbumTracks">
/// Other published tracks from the same album, excluding this one. Empty when the lyrics page
/// has no linked album (a standalone single).
/// </param>
/// <param name="StreamingLinks">
/// The resolved streaming platform deep links for this release — always populated for both an
/// album track and a standalone single, either curated or generated.
/// </param>
public record PublicGetLyricsBySlugResponse(
    LyricsDetailDto Lyrics,
    string? VideoSlug,
    string? ArtistSlug,
    IReadOnlyList<AlbumTrackDto> AlbumTracks,
    IReadOnlyList<StreamingLinkDto> StreamingLinks
);

/// <summary>
/// Defines the public get lyrics by slug endpoint.
/// Returns a lyrics page matching the given URL-safe slug.
/// </summary>
public class PublicGetLyricsBySlugEndpointV1 : ICarterModule
{
    /// <summary>
    /// Configures the lyrics by slug retrieval route within the API pipeline.
    /// Maps the <c>GET /api/v1/public/lyrics/{slug}</c> endpoint.
    /// </summary>
    /// <param name="app">The route builder used to register API endpoints.</param>
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        RouteGroupBuilder group = app.MapApiVersionGroup(1)
            .MapGroup($"{ContentConstants.Public}/{EditorialRouteConstants.Lyrics}")
            .WithTags($"{ContentConstants.Public}::{EditorialRouteConstants.Lyrics}");

        group
            .MapGet(
                "/{slug}",
                async (string slug, ClaimsPrincipal user, IClaimsProvider claimsProvider, IDispatcher dispatcher) =>
                {
                    Guid? userId = null;

                    if (user.Identity?.IsAuthenticated == true)
                    {
                        userId = claimsProvider.GetUserIdFromClaims(user: user);
                    }

                    var query = new PublicGetLyricsBySlugQuery(Slug: slug, CurrentUserId: userId);

                    PublicGetLyricsBySlugResult result = await dispatcher.Send(request: query);

                    var response = new PublicGetLyricsBySlugResponse(
                        Lyrics: result.Lyrics,
                        VideoSlug: result.VideoSlug,
                        ArtistSlug: result.ArtistSlug,
                        AlbumTracks: result.AlbumTracks,
                        StreamingLinks: result.StreamingLinks
                    );
                    return Results.Ok(response);
                }
            )
            .WithName(endpointName: PublicGetLyricsBySlugMetaField.GetLyricsBySlug.Name)
            .WithSummary(summary: PublicGetLyricsBySlugMetaField.GetLyricsBySlug.Summary)
            .WithDescription(description: PublicGetLyricsBySlugMetaField.GetLyricsBySlug.Description)
            .AllowAnonymous()
            .RequireRateLimiting(policyName: RateLimitPolicies.ContentBrowsing)
            .Produces<PublicGetLyricsBySlugResponse>(statusCode: StatusCodes.Status200OK)
            .ProducesProblem(statusCode: StatusCodes.Status404NotFound)
            .ProducesProblem(statusCode: StatusCodes.Status429TooManyRequests);
    }
}
