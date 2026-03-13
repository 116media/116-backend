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

namespace _116.Content.Application.Editorial.UseCases.Public.Queries.GetPublishedArticles.V1;

/// <summary>
/// Response model for listing published articles.
/// </summary>
/// <param name="Articles">Paginated result containing article summary DTOs and pagination metadata.</param>
public record GetPublishedArticlesResponse(PaginatedResult<ArticleSummaryDto> Articles);

/// <summary>
/// Defines the public get published articles endpoint.
/// Returns a paginated list of published articles with optional filters.
/// </summary>
public class GetPublishedArticlesEndpointV1 : ICarterModule
{
    /// <summary>
    /// Configures the published articles retrieval route within the API pipeline.
    /// Maps the <c>GET /api/v1/public/articles</c> endpoint to handle article listing requests.
    /// </summary>
    /// <param name="app">The route builder used to register API endpoints.</param>
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        RouteGroupBuilder group = app.MapApiVersionGroup(1)
            .MapGroup($"{ContentConstants.Public}/{EditorialRouteConstants.Articles}")
            .WithTags($"{ContentConstants.Public}::{EditorialRouteConstants.Articles}");

        group
            .MapGet(
                "/",
                async (
                    IDispatcher dispatcher,
                    int pageIndex = 0,
                    int pageSize = 10,
                    Guid? categoryId = null,
                    string? tagSlug = null
                ) =>
                {
                    var paginatedRequest = new PaginatedRequest(pageIndex, pageSize);

                    var query = new GetPublishedArticlesQuery(
                        PaginatedRequest: paginatedRequest,
                        CategoryId: categoryId,
                        TagSlug: tagSlug
                    );

                    GetPublishedArticlesResult result = await dispatcher.Send(request: query);

                    var response = new GetPublishedArticlesResponse(Articles: result.Articles);
                    return Results.Ok(response);
                }
            )
            .WithName(endpointName: GetPublishedArticlesMetaField.GetPublishedArticles.Name)
            .WithSummary(summary: GetPublishedArticlesMetaField.GetPublishedArticles.Summary)
            .WithDescription(description: GetPublishedArticlesMetaField.GetPublishedArticles.Description)
            .AllowAnonymous()
            .RequireRateLimiting(policyName: RateLimitPolicies.ContentBrowsing)
            .Produces<GetPublishedArticlesResponse>(statusCode: StatusCodes.Status200OK)
            .ProducesProblem(statusCode: StatusCodes.Status429TooManyRequests);
    }
}
