using _116.BuildingBlocks.Constants.RateLimit;
using _116.Content.Application.Editorial.Constants;
using _116.Content.Application.Shared.DTOs;
using _116.Content.Domain.Constants;
using _116.Shared.Application.Extensions;
using _116.Shared.Contracts.Application.CQRS;
using Carter;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace _116.Content.Application.Editorial.UseCases.Public.Queries.GetPopularArticles.V1;

/// <summary>
/// Response model for listing popular articles.
/// </summary>
/// <param name="Articles">The articles ordered by engagement score descending.</param>
public record PublicGetPopularArticlesResponse(IReadOnlyList<ArticleSummaryDto> Articles);

/// <summary>
/// Defines the public get popular articles endpoint.
/// Returns published articles ranked by a weighted engagement score, cached for 10 minutes.
/// </summary>
public class PublicGetPopularArticlesEndpointV1 : ICarterModule
{
    /// <summary>
    /// Configures the popular articles retrieval route within the API pipeline.
    /// Maps the <c>GET /api/v1/public/articles/popular</c> endpoint.
    /// </summary>
    /// <param name="app">The route builder used to register API endpoints.</param>
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        RouteGroupBuilder group = app.MapApiVersionGroup(1)
            .MapGroup($"{ContentConstants.Public}/{EditorialRouteConstants.Articles}/{EditorialRouteConstants.Popular}")
            .WithTags($"{ContentConstants.Public}::{EditorialRouteConstants.Articles}");

        group
            .MapGet(
                "/",
                async (
                    IDispatcher dispatcher,
                    int limit = PopularArticlesLimits.DefaultLimit,
                    Guid? categoryId = null,
                    Guid? excludeId = null
                ) =>
                {
                    int safeLimit = Math.Clamp(
                        value: limit,
                        min: PopularArticlesLimits.MinLimit,
                        max: PopularArticlesLimits.MaxLimit
                    );

                    var query = new PublicGetPopularArticlesQuery(
                        Limit: safeLimit,
                        CategoryId: categoryId,
                        ExcludeId: excludeId
                    );

                    PublicGetPopularArticlesResult result = await dispatcher.Send(request: query);

                    var response = new PublicGetPopularArticlesResponse(Articles: result.Articles);
                    return Results.Ok(response);
                }
            )
            .WithName(endpointName: PublicGetPopularArticlesMetaField.GetPopularArticles.Name)
            .WithSummary(summary: PublicGetPopularArticlesMetaField.GetPopularArticles.Summary)
            .WithDescription(description: PublicGetPopularArticlesMetaField.GetPopularArticles.Description)
            .AllowAnonymous()
            .RequireRateLimiting(policyName: RateLimitPolicies.ContentBrowsing)
            .Produces<PublicGetPopularArticlesResponse>(statusCode: StatusCodes.Status200OK)
            .ProducesProblem(statusCode: StatusCodes.Status429TooManyRequests);
    }
}
