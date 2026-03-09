using _116.BuildingBlocks.Constants.Authorization.Policies;
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

namespace _116.Content.Application.Catalog.UseCases.Admin.Queries.GetAllCategories.V1;

/// <summary>
/// Response model for listing all categories.
/// </summary>
/// <param name="Categories">Paginated result containing category DTOs and pagination metadata.</param>
public record AdminGetAllCategoriesResponse(PaginatedResult<CategoryDto> Categories);

/// <summary>
/// Defines the admin get all categories' endpoint.
/// Returns a paginated list of categories with optional filters.
/// </summary>
public class AdminGetAllCategoriesEndpointV1 : ICarterModule
{
    /// <summary>
    /// Configures the category retrieval route within the API pipeline.
    /// Maps the <c>GET /api/v1/admin/categories</c> endpoint to handle category retrieval requests.
    /// </summary>
    /// <param name="app">The route builder used to register API endpoints.</param>
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        RouteGroupBuilder group = app.MapApiVersionGroup(1)
            .MapGroup($"{ContentConstants.Admin}/{CatalogRouteConstants.Categories}")
            .WithTags($"{ContentConstants.Admin}::{CatalogRouteConstants.Categories}");

        group
            .MapGet(
                "/",
                async (
                    IDispatcher dispatcher,
                    int pageIndex = 0,
                    int pageSize = 10,
                    bool? isActive = null,
                    bool? isFree = null
                ) =>
                {
                    var paginatedRequest = new PaginatedRequest(pageIndex, pageSize);

                    var query = new AdminGetAllCategoriesQuery(
                        PaginatedRequest: paginatedRequest,
                        IsActive: isActive,
                        IsFree: isFree
                    );

                    AdminGetAllCategoriesResult result = await dispatcher.Send(request: query);

                    var response = new AdminGetAllCategoriesResponse(Categories: result.Categories);
                    return Results.Ok(response);
                }
            )
            .WithName(endpointName: AdminGetAllCategoriesMetaField.AdminGetAllCategories.Name)
            .WithSummary(summary: AdminGetAllCategoriesMetaField.AdminGetAllCategories.Summary)
            .WithDescription(description: AdminGetAllCategoriesMetaField.AdminGetAllCategories.Description)
            .WithAuthorization(AccountStatusPolicies.RequireActiveUser)
            .WithAuthorization(UserRolePolicies.RequireAdminOrSuperAdmin)
            .RequireRateLimiting(policyName: RateLimitPolicies.ContentBrowsing)
            .Produces<AdminGetAllCategoriesResponse>(statusCode: StatusCodes.Status200OK)
            .ProducesProblem(statusCode: StatusCodes.Status401Unauthorized)
            .ProducesProblem(statusCode: StatusCodes.Status403Forbidden)
            .ProducesProblem(statusCode: StatusCodes.Status429TooManyRequests);
    }
}
