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

namespace _116.Content.Application.Catalog.UseCases.Admin.Queries.GetAllCustomers.V1;

/// <summary>
/// Response model for listing all customers.
/// </summary>
/// <param name="Customers">Paginated result containing customer DTOs and pagination metadata.</param>
public record AdminGetAllCustomersResponse(PaginatedResult<CustomerDto> Customers);

/// <summary>
/// Defines the admin get all customers endpoint.
/// Returns a paginated list of B2B customers ordered by most recently created first.
/// </summary>
public class AdminGetAllCustomersEndpointV1 : ICarterModule
{
    /// <summary>
    /// Configures the customer retrieval route within the API pipeline.
    /// Maps the <c>GET /api/v1/admin/customers</c> endpoint to handle customer retrieval requests.
    /// </summary>
    /// <param name="app">The route builder used to register API endpoints.</param>
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        RouteGroupBuilder group = app.MapApiVersionGroup(1)
            .MapGroup($"{ContentConstants.Admin}/{CatalogRouteConstants.Customers}")
            .WithTags($"{ContentConstants.Admin}::{CatalogRouteConstants.Customers}");

        group
            .MapGet(
                "/",
                async (IDispatcher dispatcher, int pageIndex = 0, int pageSize = 10) =>
                {
                    var paginatedRequest = new PaginatedRequest(pageIndex, pageSize);
                    var query = new AdminGetAllCustomersQuery(PaginatedRequest: paginatedRequest);

                    AdminGetAllCustomersResult result = await dispatcher.Send(request: query);

                    var response = new AdminGetAllCustomersResponse(Customers: result.Customers);
                    return Results.Ok(response);
                }
            )
            .WithName(endpointName: AdminGetAllCustomersMetaField.AdminGetAllCustomers.Name)
            .WithSummary(summary: AdminGetAllCustomersMetaField.AdminGetAllCustomers.Summary)
            .WithDescription(description: AdminGetAllCustomersMetaField.AdminGetAllCustomers.Description)
            .WithAuthorization(AccountStatusPolicies.RequireActiveUser)
            .WithAuthorization(UserRolePolicies.RequireAdminOrSuperAdmin)
            .RequireRateLimiting(policyName: RateLimitPolicies.ContentBrowsing)
            .Produces<AdminGetAllCustomersResponse>(statusCode: StatusCodes.Status200OK)
            .ProducesProblem(statusCode: StatusCodes.Status401Unauthorized)
            .ProducesProblem(statusCode: StatusCodes.Status403Forbidden)
            .ProducesProblem(statusCode: StatusCodes.Status429TooManyRequests);
    }
}
