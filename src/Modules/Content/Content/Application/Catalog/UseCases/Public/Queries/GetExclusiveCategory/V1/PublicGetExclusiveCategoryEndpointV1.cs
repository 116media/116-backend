using _116.BuildingBlocks.Constants.RateLimit;
using _116.Content.Application.Catalog.Constants;
using _116.Content.Application.Shared.DTOs;
using _116.Content.Domain.Constants;
using _116.Shared.Application.Extensions;
using _116.Shared.Application.Pagination;
using _116.Shared.Contracts.Application.CQRS;
using Carter;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace _116.Content.Application.Catalog.UseCases.Public.Queries.GetExclusiveCategory.V1;

/// <summary>
/// Response model for the exclusive category with its videos.
/// </summary>
/// <param name="Category">The exclusive category DTO.</param>
/// <param name="Videos">Paginated list of published videos in the exclusive category.</param>
public record PublicGetExclusiveCategoryResponse(CategoryDto Category, PaginatedResult<VideoSummaryDto> Videos);

/// <summary>
/// Defines the public get exclusive category endpoint.
/// Returns the exclusive category and a paginated list of its published videos.
/// </summary>
public class PublicGetExclusiveCategoryEndpointV1 : ICarterModule
{
    /// <summary>
    /// Configures the exclusive category retrieval route within the API pipeline.
    /// Maps the <c>GET /api/v1/public/categories/exclusive</c> endpoint.
    /// </summary>
    /// <param name="app">The route builder used to register API endpoints.</param>
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        RouteGroupBuilder group = app.MapApiVersionGroup(1)
            .MapGroup($"{ContentConstants.Public}/{CatalogRouteConstants.Categories}")
            .WithTags($"{ContentConstants.Public}::{CatalogRouteConstants.Categories}");

        group
            .MapGet(
                "/exclusive",
                async (IDispatcher dispatcher, int pageIndex = 0, int pageSize = 10) =>
                {
                    var paginatedRequest = new PaginatedRequest(pageIndex, pageSize);

                    var query = new PublicGetExclusiveCategoryQuery(PaginatedRequest: paginatedRequest);

                    PublicGetExclusiveCategoryResult result = await dispatcher.Send(request: query);

                    var response = new PublicGetExclusiveCategoryResponse(
                        Category: result.Category,
                        Videos: result.Videos
                    );

                    return Results.Ok(response);
                }
            )
            .WithName(endpointName: PublicGetExclusiveCategoryMetaField.GetExclusiveCategory.Name)
            .WithSummary(summary: PublicGetExclusiveCategoryMetaField.GetExclusiveCategory.Summary)
            .WithDescription(description: PublicGetExclusiveCategoryMetaField.GetExclusiveCategory.Description)
            .AllowAnonymous()
            .RequireRateLimiting(policyName: RateLimitPolicies.ContentBrowsing)
            .Produces<PublicGetExclusiveCategoryResponse>(statusCode: StatusCodes.Status200OK)
            .ProducesProblem(statusCode: StatusCodes.Status404NotFound)
            .ProducesProblem(statusCode: StatusCodes.Status429TooManyRequests);
    }
}
