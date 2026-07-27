using _116.BuildingBlocks.Constants.RateLimit;
using _116.Content.Application.Editorial.Constants;
using _116.Content.Application.Shared.DTOs;
using _116.Content.Domain.Constants;
using _116.Content.Domain.Enums;
using _116.Shared.Application.Extensions;
using _116.Shared.Application.Pagination;
using _116.Shared.Contracts.Application.CQRS;
using Carter;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace _116.Content.Application.Editorial.UseCases.Public.Queries.GetArtistReleases.V1;

/// <summary>
/// Response model for retrieving an artist's releases by slug.
/// </summary>
/// <param name="Releases">The artist's paginated releases of the requested type.</param>
public record PublicGetArtistReleasesResponse(PaginatedResult<AlbumDto> Releases);

/// <summary>
/// Defines the public get artist releases endpoint.
/// Returns a paginated page of an artist's releases filtered by release type.
/// </summary>
public class PublicGetArtistReleasesEndpointV1 : ICarterModule
{
    /// <summary>
    /// Configures the artist releases retrieval route within the API pipeline.
    /// Maps the <c>GET /api/v1/public/artists/{slug}/releases</c> endpoint.
    /// </summary>
    /// <param name="app">The route builder used to register API endpoints.</param>
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        RouteGroupBuilder group = app.MapApiVersionGroup(1)
            .MapGroup($"{ContentConstants.Public}/{EditorialRouteConstants.Artists}")
            .WithTags($"{ContentConstants.Public}::{EditorialRouteConstants.Artists}");

        group
            .MapGet(
                $"/{{slug}}/{EditorialRouteConstants.Releases}",
                async (
                    string slug,
                    IDispatcher dispatcher,
                    EnumReleaseType type = EnumReleaseType.Album,
                    int pageIndex = 0,
                    int pageSize = 12
                ) =>
                {
                    var query = new PublicGetArtistReleasesQuery(
                        Slug: slug,
                        ReleaseType: type,
                        Page: new PaginatedRequest(pageIndex, pageSize)
                    );

                    PublicGetArtistReleasesResult result = await dispatcher.Send(request: query);

                    var response = new PublicGetArtistReleasesResponse(Releases: result.Releases);
                    return Results.Ok(response);
                }
            )
            .WithName(endpointName: PublicGetArtistReleasesMetaField.GetArtistReleases.Name)
            .WithSummary(summary: PublicGetArtistReleasesMetaField.GetArtistReleases.Summary)
            .WithDescription(description: PublicGetArtistReleasesMetaField.GetArtistReleases.Description)
            .AllowAnonymous()
            .RequireRateLimiting(policyName: RateLimitPolicies.ContentBrowsing)
            .Produces<PublicGetArtistReleasesResponse>(statusCode: StatusCodes.Status200OK)
            .ProducesProblem(statusCode: StatusCodes.Status404NotFound)
            .ProducesProblem(statusCode: StatusCodes.Status429TooManyRequests);
    }
}
