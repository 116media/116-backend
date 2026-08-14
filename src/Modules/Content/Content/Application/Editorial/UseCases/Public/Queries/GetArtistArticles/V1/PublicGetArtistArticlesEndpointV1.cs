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

namespace _116.Content.Application.Editorial.UseCases.Public.Queries.GetArtistArticles.V1;

/// <summary>
/// Response model for retrieving articles tagged to an artist.
/// </summary>
/// <param name="Articles">The published articles tagged to the artist, newest first.</param>
public record PublicGetArtistArticlesResponse(PaginatedResult<ArticleSummaryDto> Articles);

/// <summary>
/// Defines the public get artist articles endpoint.
/// Returns published articles tagged to an artist, addressed by slug.
/// </summary>
public class PublicGetArtistArticlesEndpointV1 : ICarterModule
{
    /// <summary>
    /// Configures the artist articles retrieval route within the API pipeline.
    /// Maps the <c>GET /api/v1/public/artists/{slug}/articles</c> endpoint.
    /// </summary>
    /// <param name="app">The route builder used to register API endpoints.</param>
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        RouteGroupBuilder group = app.MapApiVersionGroup(1)
            .MapGroup($"{ContentConstants.Public}/{EditorialRouteConstants.Artists}")
            .WithTags($"{ContentConstants.Public}::{EditorialRouteConstants.Artists}");

        group
            .MapGet(
                $"/{{slug}}/{EditorialRouteConstants.Articles}",
                async (string slug, IDispatcher dispatcher, int pageIndex = 0, int pageSize = 12) =>
                {
                    var query = new PublicGetArtistArticlesQuery(
                        Slug: slug,
                        Page: new PaginatedRequest(pageIndex, pageSize)
                    );

                    PublicGetArtistArticlesResult result = await dispatcher.Send(request: query);

                    var response = new PublicGetArtistArticlesResponse(Articles: result.Articles);
                    return Results.Ok(response);
                }
            )
            .WithName(endpointName: PublicGetArtistArticlesMetaField.GetArtistArticles.Name)
            .WithSummary(summary: PublicGetArtistArticlesMetaField.GetArtistArticles.Summary)
            .WithDescription(description: PublicGetArtistArticlesMetaField.GetArtistArticles.Description)
            .AllowAnonymous()
            .RequireRateLimiting(policyName: RateLimitPolicies.ContentBrowsing)
            .Produces<PublicGetArtistArticlesResponse>(statusCode: StatusCodes.Status200OK)
            .ProducesProblem(statusCode: StatusCodes.Status404NotFound)
            .ProducesProblem(statusCode: StatusCodes.Status429TooManyRequests);
    }
}
