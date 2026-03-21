using _116.BuildingBlocks.Constants.Authorization.Policies;
using _116.BuildingBlocks.Constants.RateLimit;
using _116.Content.Application.Commerce.Constants;
using _116.Content.Application.Shared.DTOs;
using _116.Content.Domain.Constants;
using _116.Shared.Application.Extensions;
using _116.Shared.Application.Pagination;
using _116.Shared.Contracts.Application.CQRS;
using Carter;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace _116.Content.Application.Commerce.UseCases.Admin.Queries.GetCustomerOrders.V1;

/// <summary>
/// Response model for listing a customer's orders.
/// </summary>
/// <param name="Orders">Paginated result containing order summary DTOs and pagination metadata.</param>
public record AdminGetCustomerOrdersResponse(PaginatedResult<ContentOrderSummaryDto> Orders);

/// <summary>
/// Defines the admin get customer orders endpoint.
/// </summary>
public class AdminGetCustomerOrdersEndpointV1 : ICarterModule
{
    /// <inheritdoc />
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        RouteGroupBuilder group = app.MapApiVersionGroup(1)
            .MapGroup($"{ContentConstants.Admin}/customers")
            .WithTags($"{ContentConstants.Admin}::customers");

        group
            .MapGet(
                $"/{{id:guid}}/{CommerceRouteConstants.Orders}",
                async (Guid id, IDispatcher dispatcher, int pageIndex = 0, int pageSize = 10) =>
                {
                    var paginatedRequest = new PaginatedRequest(pageIndex, pageSize);
                    var query = new AdminGetCustomerOrdersQuery(CustomerId: id, PaginatedRequest: paginatedRequest);

                    AdminGetCustomerOrdersResult result = await dispatcher.Send(request: query);

                    var response = new AdminGetCustomerOrdersResponse(Orders: result.Orders);
                    return Results.Ok(response);
                }
            )
            .WithName(endpointName: AdminGetCustomerOrdersMetaField.AdminGetCustomerOrders.Name)
            .WithSummary(summary: AdminGetCustomerOrdersMetaField.AdminGetCustomerOrders.Summary)
            .WithDescription(description: AdminGetCustomerOrdersMetaField.AdminGetCustomerOrders.Description)
            .WithAuthorization(AccountStatusPolicies.RequireActiveUser)
            .WithAuthorization(UserRolePolicies.RequireAdminOrSuperAdmin)
            .RequireRateLimiting(policyName: RateLimitPolicies.ContentBrowsing)
            .Produces<AdminGetCustomerOrdersResponse>(statusCode: StatusCodes.Status200OK)
            .ProducesProblem(statusCode: StatusCodes.Status401Unauthorized)
            .ProducesProblem(statusCode: StatusCodes.Status403Forbidden)
            .ProducesProblem(statusCode: StatusCodes.Status429TooManyRequests);
    }
}
