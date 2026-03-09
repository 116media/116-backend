using _116.BuildingBlocks.Constants.RateLimit;
using _116.Content.Application.Catalog.Constants;
using _116.Content.Application.Shared.DTOs;
using _116.Content.Domain.Constants;
using _116.Shared.Application.Extensions;
using _116.Shared.Contracts.Application.CQRS;
using Carter;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace _116.Content.Application.Catalog.UseCases.Public.Queries.GetActiveCategories.V1;

/// <summary>
/// Response model for listing public active categories.
/// </summary>
/// <param name="Categories">The list of active category DTOs.</param>
public record PublicGetActiveCategoriesResponse(IReadOnlyList<CategoryDto> Categories);

/// <summary>
/// Defines the public get categories' endpoint.
/// Returns all active categories for public browsing, optionally filtered by content type.
/// </summary>
public class PublicGetActiveCategoriesEndpointV1 : ICarterModule
{
    /// <summary>
    /// Configures the public category retrieval route within the API pipeline.
    /// Maps the <c>GET /api/v1/public/categories</c> endpoint to handle public category retrieval requests.
    /// </summary>
    /// <param name="app">The route builder used to register API endpoints.</param>
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        RouteGroupBuilder group = app.MapApiVersionGroup(1)
            .MapGroup($"{ContentConstants.Public}/{CatalogRouteConstants.Categories}")
            .WithTags($"{ContentConstants.Public}::{CatalogRouteConstants.Categories}");

        group
            .MapGet(
                "/",
                async (IDispatcher dispatcher, Guid? contentTypeId = null) =>
                {
                    var query = new PublicGetActiveCategoriesQuery(ContentTypeId: contentTypeId);

                    PublicGetActiveCategoriesResult result = await dispatcher.Send(request: query);

                    var response = new PublicGetActiveCategoriesResponse(Categories: result.Categories);
                    return Results.Ok(response);
                }
            )
            .WithName(endpointName: PublicGetActiveCategoriesMetaField.PublicGetActiveCategories.Name)
            .WithSummary(summary: PublicGetActiveCategoriesMetaField.PublicGetActiveCategories.Summary)
            .WithDescription(description: PublicGetActiveCategoriesMetaField.PublicGetActiveCategories.Description)
            .AllowAnonymous()
            .RequireRateLimiting(policyName: RateLimitPolicies.ContentBrowsing)
            .Produces<PublicGetActiveCategoriesResponse>(statusCode: StatusCodes.Status200OK)
            .ProducesProblem(statusCode: StatusCodes.Status429TooManyRequests);
    }
}
