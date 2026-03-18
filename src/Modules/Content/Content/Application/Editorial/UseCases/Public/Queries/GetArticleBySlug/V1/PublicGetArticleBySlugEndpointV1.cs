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

namespace _116.Content.Application.Editorial.UseCases.Public.Queries.GetArticleBySlug.V1;

/// <summary>
/// Response model for retrieving a published article by its slug.
/// </summary>
/// <param name="Article">The full article detail information.</param>
public record PublicGetArticleBySlugResponse(ArticleDetailDto Article);

/// <summary>
/// Defines the public get article by slug endpoint.
/// Returns the full details of a single-published article.
/// </summary>
public class PublicGetArticleBySlugEndpointV1 : ICarterModule
{
    /// <summary>
    /// Configures the article detail retrieval route within the API pipeline.
    /// Maps the <c>GET /api/v1/public/articles/{slug}</c> endpoint.
    /// </summary>
    /// <param name="app">The route builder used to register API endpoints.</param>
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        RouteGroupBuilder group = app.MapApiVersionGroup(1)
            .MapGroup($"{ContentConstants.Public}/{EditorialRouteConstants.Articles}")
            .WithTags($"{ContentConstants.Public}::{EditorialRouteConstants.Articles}");

        group
            .MapGet(
                "/{slug}",
                async (string slug, IDispatcher dispatcher) =>
                {
                    var query = new PublicGetArticleBySlugQuery(Slug: slug);
                    PublicGetArticleBySlugResult result = await dispatcher.Send(request: query);

                    var response = new PublicGetArticleBySlugResponse(Article: result.Article);
                    return Results.Ok(response);
                }
            )
            .WithName(endpointName: PublicGetArticleBySlugMetaField.PublicGetArticleBySlug.Name)
            .WithSummary(summary: PublicGetArticleBySlugMetaField.PublicGetArticleBySlug.Summary)
            .WithDescription(description: PublicGetArticleBySlugMetaField.PublicGetArticleBySlug.Description)
            .AllowAnonymous()
            .RequireRateLimiting(policyName: RateLimitPolicies.ContentBrowsing)
            .Produces<PublicGetArticleBySlugResponse>(statusCode: StatusCodes.Status200OK)
            .ProducesProblem(statusCode: StatusCodes.Status404NotFound)
            .ProducesProblem(statusCode: StatusCodes.Status429TooManyRequests);
    }
}
