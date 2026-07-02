using System.Security.Claims;
using _116.BuildingBlocks.Constants.RateLimit;
using _116.Content.Application.Editorial.Constants;
using _116.Content.Application.Shared.DTOs;
using _116.Content.Domain.Constants;
using _116.Identity.Contracts.Application;
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
public record PublicGetPublishedArticlesResponse(PaginatedResult<ArticleSummaryDto> Articles);

/// <summary>
/// Defines the public get published articles endpoint.
/// Returns a paginated list of published articles with optional filters.
/// </summary>
public class PublicGetPublishedArticlesEndpointV1 : ICarterModule
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
                    ClaimsPrincipal user,
                    IClaimsProvider claimsProvider,
                    IDispatcher dispatcher,
                    int pageIndex = 0,
                    int pageSize = 10,
                    string? search = null,
                    Guid? categoryId = null,
                    string? tagSlug = null
                ) =>
                {
                    Guid? userId = null;

                    if (user.Identity?.IsAuthenticated == true)
                    {
                        userId = claimsProvider.GetUserIdFromClaims(user: user);
                    }

                    var paginatedRequest = new PaginatedRequest(pageIndex, pageSize);

                    var query = new PublicGetPublishedArticlesQuery(
                        PaginatedRequest: paginatedRequest,
                        Search: search,
                        CategoryId: categoryId,
                        TagSlug: tagSlug,
                        CurrentUserId: userId
                    );

                    PublicGetPublishedArticlesResult result = await dispatcher.Send(request: query);

                    var response = new PublicGetPublishedArticlesResponse(Articles: result.Articles);
                    return Results.Ok(response);
                }
            )
            .WithName(endpointName: PublicGetPublishedArticlesMetaField.GetPublishedArticles.Name)
            .WithSummary(summary: PublicGetPublishedArticlesMetaField.GetPublishedArticles.Summary)
            .WithDescription(description: PublicGetPublishedArticlesMetaField.GetPublishedArticles.Description)
            .AllowAnonymous()
            .RequireRateLimiting(policyName: RateLimitPolicies.ContentBrowsing)
            .Produces<PublicGetPublishedArticlesResponse>(statusCode: StatusCodes.Status200OK)
            .ProducesProblem(statusCode: StatusCodes.Status429TooManyRequests);
    }
}
