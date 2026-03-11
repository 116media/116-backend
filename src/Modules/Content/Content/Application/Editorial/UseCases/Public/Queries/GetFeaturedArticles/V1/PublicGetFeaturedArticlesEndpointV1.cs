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

namespace _116.Content.Application.Editorial.UseCases.Public.Queries.GetFeaturedArticles.V1;

/// <summary>
/// Response model for listing featured articles.
/// </summary>
/// <param name="Articles">The list of featured article summary DTOs.</param>
public record PublicGetFeaturedArticlesResponse(IReadOnlyList<ArticleSummaryDto> Articles);

/// <summary>
/// Defines the public get featured articles endpoint.
/// Returns the list of currently featured published articles.
/// </summary>
public class PublicGetFeaturedArticlesEndpointV1 : ICarterModule
{
    /// <summary>
    /// Configures the featured articles retrieval route within the API pipeline.
    /// Maps the <c>GET /api/v1/public/articles/featured</c> endpoint.
    /// </summary>
    /// <param name="app">The route builder used to register API endpoints.</param>
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        RouteGroupBuilder group = app.MapApiVersionGroup(1)
            .MapGroup($"{ContentConstants.Public}/{EditorialRouteConstants.Articles}")
            .WithTags($"{ContentConstants.Public}::{EditorialRouteConstants.Articles}");

        group
            .MapGet(
                $"/{EditorialRouteConstants.Featured}",
                async (IDispatcher dispatcher) =>
                {
                    var query = new PublicGetFeaturedArticlesQuery();
                    PublicGetFeaturedArticlesResult result = await dispatcher.Send(request: query);
                    return Results.Ok(new PublicGetFeaturedArticlesResponse(Articles: result.Articles));
                }
            )
            .WithName(endpointName: PublicGetFeaturedArticlesMetaField.PublicGetFeaturedArticles.Name)
            .WithSummary(summary: PublicGetFeaturedArticlesMetaField.PublicGetFeaturedArticles.Summary)
            .WithDescription(description: PublicGetFeaturedArticlesMetaField.PublicGetFeaturedArticles.Description)
            .AllowAnonymous()
            .RequireRateLimiting(policyName: RateLimitPolicies.ContentBrowsing)
            .Produces<PublicGetFeaturedArticlesResponse>(statusCode: StatusCodes.Status200OK)
            .ProducesProblem(statusCode: StatusCodes.Status429TooManyRequests);
    }
}
