using _116.BuildingBlocks.Constants.Authorization.Policies;
using _116.BuildingBlocks.Constants.RateLimit;
using _116.Content.Application.Commerce.Constants;
using _116.Content.Application.Shared.DTOs;
using _116.Content.Domain.Constants;
using _116.Shared.Application.Extensions;
using _116.Shared.Contracts.Application.CQRS;
using Carter;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace _116.Content.Application.Commerce.UseCases.Admin.Queries.GetOrderById.V1;

/// <summary>
/// Response model for retrieving a single order.
/// </summary>
/// <param name="Order">The full order details including items, tiers, and payment.</param>
public record AdminGetOrderByIdResponse(ContentOrderDetailDto Order);

/// <summary>
/// Defines the admin get order by ID endpoint.
/// </summary>
public class AdminGetOrderByIdEndpointV1 : ICarterModule
{
    /// <inheritdoc />
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        RouteGroupBuilder group = app.MapApiVersionGroup(1)
            .MapGroup($"{ContentConstants.Admin}/{CommerceRouteConstants.Orders}")
            .WithTags($"{ContentConstants.Admin}::{CommerceRouteConstants.Orders}");

        group
            .MapGet(
                "/{id:guid}",
                async (Guid id, IDispatcher dispatcher) =>
                {
                    var query = new AdminGetOrderByIdQuery(Id: id);

                    AdminGetOrderByIdResult result = await dispatcher.Send(request: query);

                    var response = new AdminGetOrderByIdResponse(Order: result.Order);
                    return Results.Ok(response);
                }
            )
            .WithName(endpointName: AdminGetOrderByIdMetaField.AdminGetOrderById.Name)
            .WithSummary(summary: AdminGetOrderByIdMetaField.AdminGetOrderById.Summary)
            .WithDescription(description: AdminGetOrderByIdMetaField.AdminGetOrderById.Description)
            .WithAuthorization(AccountStatusPolicies.RequireActiveUser)
            .WithAuthorization(UserRolePolicies.RequireAdminOrSuperAdmin)
            .RequireRateLimiting(policyName: RateLimitPolicies.ContentBrowsing)
            .Produces<AdminGetOrderByIdResponse>(statusCode: StatusCodes.Status200OK)
            .ProducesProblem(statusCode: StatusCodes.Status401Unauthorized)
            .ProducesProblem(statusCode: StatusCodes.Status403Forbidden)
            .ProducesProblem(statusCode: StatusCodes.Status404NotFound)
            .ProducesProblem(statusCode: StatusCodes.Status429TooManyRequests);
    }
}
