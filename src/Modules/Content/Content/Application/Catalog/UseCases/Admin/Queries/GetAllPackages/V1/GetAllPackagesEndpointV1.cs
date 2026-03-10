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

namespace _116.Content.Application.Catalog.UseCases.Admin.Queries.GetAllPackages.V1;

/// <summary>
/// Response model for listing all packages.
/// </summary>
/// <param name="Packages">Paginated result containing package DTOs and pagination metadata.</param>
public record GetAllPackagesResponse(PaginatedResult<PackageDto> Packages);

/// <summary>
/// Defines the admin get all packages endpoint.
/// Returns a paginated list of content packages with their slot compositions.
/// </summary>
public class GetAllPackagesEndpointV1 : ICarterModule
{
    /// <summary>
    /// Configures the package retrieval route within the API pipeline.
    /// Maps the <c>GET /api/v1/admin/packages</c> endpoint to handle package retrieval requests.
    /// </summary>
    /// <param name="app">The route builder used to register API endpoints.</param>
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        RouteGroupBuilder group = app.MapApiVersionGroup(1)
            .MapGroup($"{ContentConstants.Admin}/{CatalogRouteConstants.Packages}")
            .WithTags($"{ContentConstants.Admin}::{CatalogRouteConstants.Packages}");

        group
            .MapGet(
                "/",
                async (IDispatcher dispatcher, int pageIndex = 0, int pageSize = 10, bool? isActive = null) =>
                {
                    var paginatedRequest = new PaginatedRequest(pageIndex, pageSize);
                    var query = new GetAllPackagesQuery(PaginatedRequest: paginatedRequest, IsActive: isActive);

                    GetAllPackagesResult result = await dispatcher.Send(request: query);

                    var response = new GetAllPackagesResponse(Packages: result.Packages);
                    return Results.Ok(response);
                }
            )
            .WithName(endpointName: GetAllPackagesMetaField.GetAllPackages.Name)
            .WithSummary(summary: GetAllPackagesMetaField.GetAllPackages.Summary)
            .WithDescription(description: GetAllPackagesMetaField.GetAllPackages.Description)
            .WithAuthorization(AccountStatusPolicies.RequireActiveUser)
            .WithAuthorization(UserRolePolicies.RequireAdminOrSuperAdmin)
            .RequireRateLimiting(policyName: RateLimitPolicies.ContentBrowsing)
            .Produces<GetAllPackagesResponse>(statusCode: StatusCodes.Status200OK)
            .ProducesProblem(statusCode: StatusCodes.Status401Unauthorized)
            .ProducesProblem(statusCode: StatusCodes.Status403Forbidden)
            .ProducesProblem(statusCode: StatusCodes.Status429TooManyRequests);
    }
}
