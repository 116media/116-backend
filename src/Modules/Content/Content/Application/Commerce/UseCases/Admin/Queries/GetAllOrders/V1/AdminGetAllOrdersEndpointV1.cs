using _116.BuildingBlocks.Constants.Authorization.Policies;
using _116.BuildingBlocks.Constants.RateLimit;
using _116.Content.Application.Commerce.Constants;
using _116.Content.Application.Shared.DTOs;
using _116.Content.Domain.Constants;
using _116.Content.Domain.Enums;
using _116.Shared.Application.Extensions;
using _116.Shared.Application.Pagination;
using _116.Shared.Contracts.Application.CQRS;
using Carter;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace _116.Content.Application.Commerce.UseCases.Admin.Queries.GetAllOrders.V1;

/// <summary>
/// Response model for listing all orders.
/// </summary>
/// <param name="Orders">Paginated result containing order summary DTOs and pagination metadata.</param>
public record AdminGetAllOrdersResponse(PaginatedResult<ContentOrderSummaryDto> Orders);

/// <summary>
/// Defines the admin get all orders endpoint.
/// </summary>
public class AdminGetAllOrdersEndpointV1 : ICarterModule
{
    /// <inheritdoc />
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        RouteGroupBuilder group = app.MapApiVersionGroup(1)
            .MapGroup($"{ContentConstants.Admin}/{CommerceRouteConstants.Orders}")
            .WithTags($"{ContentConstants.Admin}::{CommerceRouteConstants.Orders}");

        group
            .MapGet(
                "/",
                async (
                    IDispatcher dispatcher,
                    int pageIndex = 0,
                    int pageSize = 10,
                    EnumOrderStatus? status = null,
                    Guid? customerId = null
                ) =>
                {
                    var paginatedRequest = new PaginatedRequest(pageIndex, pageSize);
                    var query = new AdminGetAllOrdersQuery(
                        PaginatedRequest: paginatedRequest,
                        Status: status,
                        CustomerId: customerId
                    );

                    AdminGetAllOrdersResult result = await dispatcher.Send(request: query);

                    var response = new AdminGetAllOrdersResponse(Orders: result.Orders);
                    return Results.Ok(response);
                }
            )
            .WithName(endpointName: AdminGetAllOrdersMetaField.AdminGetAllOrders.Name)
            .WithSummary(summary: AdminGetAllOrdersMetaField.AdminGetAllOrders.Summary)
            .WithDescription(description: AdminGetAllOrdersMetaField.AdminGetAllOrders.Description)
            .WithAuthorization(AccountStatusPolicies.RequireActiveUser)
            .WithAuthorization(UserRolePolicies.RequireAdminOrSuperAdmin)
            .RequireRateLimiting(policyName: RateLimitPolicies.ContentBrowsing)
            .Produces<AdminGetAllOrdersResponse>(statusCode: StatusCodes.Status200OK)
            .ProducesProblem(statusCode: StatusCodes.Status401Unauthorized)
            .ProducesProblem(statusCode: StatusCodes.Status403Forbidden)
            .ProducesProblem(statusCode: StatusCodes.Status429TooManyRequests);
    }
}
