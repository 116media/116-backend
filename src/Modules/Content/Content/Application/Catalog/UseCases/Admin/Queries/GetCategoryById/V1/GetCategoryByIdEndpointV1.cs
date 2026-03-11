using _116.BuildingBlocks.Constants.Authorization.Policies;
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

namespace _116.Content.Application.Catalog.UseCases.Admin.Queries.GetCategoryById.V1;

/// <summary>
/// Response model for retrieving a single category.
/// </summary>
/// <param name="Category">The category details including pricing tiers.</param>
public record GetCategoryByIdResponse(CategoryDto Category);

/// <summary>
/// Defines the admin get category by ID endpoint.
/// Returns full category details including its pricing configuration.
/// </summary>
public class GetCategoryByIdEndpointV1 : ICarterModule
{
    /// <summary>
    /// Configures the category by ID retrieval route within the API pipeline.
    /// Maps the <c>GET /api/v1/admin/categories/{id:guid}</c> endpoint to handle category retrieval requests.
    /// </summary>
    /// <param name="app">The route builder used to register API endpoints.</param>
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        RouteGroupBuilder group = app.MapApiVersionGroup(1)
            .MapGroup($"{ContentConstants.Admin}/{CatalogRouteConstants.Categories}")
            .WithTags($"{ContentConstants.Admin}::{CatalogRouteConstants.Categories}");

        group
            .MapGet(
                "/{id}",
                async (string id, IDispatcher dispatcher) =>
                {
                    var query = new GetCategoryByIdQuery(Id: id);

                    GetCategoryByIdResult result = await dispatcher.Send(request: query);

                    var response = new GetCategoryByIdResponse(Category: result.Category);
                    return Results.Ok(response);
                }
            )
            .WithName(endpointName: GetCategoryByIdMetaField.GetCategoryById.Name)
            .WithSummary(summary: GetCategoryByIdMetaField.GetCategoryById.Summary)
            .WithDescription(description: GetCategoryByIdMetaField.GetCategoryById.Description)
            .WithAuthorization(AccountStatusPolicies.RequireActiveUser)
            .WithAuthorization(UserRolePolicies.RequireAdminOrSuperAdmin)
            .RequireRateLimiting(policyName: RateLimitPolicies.ContentBrowsing)
            .Produces<GetCategoryByIdResponse>(statusCode: StatusCodes.Status200OK)
            .ProducesProblem(statusCode: StatusCodes.Status401Unauthorized)
            .ProducesProblem(statusCode: StatusCodes.Status403Forbidden)
            .ProducesProblem(statusCode: StatusCodes.Status404NotFound)
            .ProducesProblem(statusCode: StatusCodes.Status429TooManyRequests);
    }
}
