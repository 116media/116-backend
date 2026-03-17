using _116.BuildingBlocks.Constants.Authorization.Policies;
using _116.BuildingBlocks.Constants.RateLimit;
using _116.Content.Application.Commerce.Constants;
using _116.Content.Domain.Constants;
using _116.Shared.Application.Extensions;
using _116.Shared.Contracts.Application.CQRS;
using Carter;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace _116.Content.Application.Commerce.UseCases.Admin.Commands.CancelOrder.V1;

/// <summary>
/// Response model for cancelling an order.
/// </summary>
/// <param name="IsSuccess">Indicates whether the order was successfully cancelled.</param>
public record AdminCancelOrderResponse(bool IsSuccess);

/// <summary>
/// Defines the admin cancel order endpoint.
/// </summary>
public class AdminCancelOrderEndpointV1 : ICarterModule
{
    /// <inheritdoc />
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        RouteGroupBuilder group = app.MapApiVersionGroup(1)
            .MapGroup($"{ContentConstants.Admin}/{CommerceRouteConstants.Orders}")
            .WithTags($"{ContentConstants.Admin}::{CommerceRouteConstants.Orders}");

        group
            .MapPatch(
                $"/{{id}}/{CommerceRouteConstants.Cancel}",
                async (string id, IDispatcher dispatcher) =>
                {
                    var command = new AdminCancelOrderCommand(OrderId: id);
                    AdminCancelOrderResult result = await dispatcher.Send(request: command);

                    var response = new AdminCancelOrderResponse(IsSuccess: result.IsSuccess);
                    return Results.Ok(response);
                }
            )
            .WithName(endpointName: AdminCancelOrderMetaField.AdminCancelOrder.Name)
            .WithSummary(summary: AdminCancelOrderMetaField.AdminCancelOrder.Summary)
            .WithDescription(description: AdminCancelOrderMetaField.AdminCancelOrder.Description)
            .WithAuthorization(AccountStatusPolicies.RequireActiveUser)
            .WithAuthorization(UserRolePolicies.RequireAdminOrSuperAdmin)
            .RequireRateLimiting(policyName: RateLimitPolicies.ContentBrowsing)
            .Produces<AdminCancelOrderResponse>(statusCode: StatusCodes.Status200OK)
            .ProducesProblem(statusCode: StatusCodes.Status400BadRequest)
            .ProducesProblem(statusCode: StatusCodes.Status401Unauthorized)
            .ProducesProblem(statusCode: StatusCodes.Status403Forbidden)
            .ProducesProblem(statusCode: StatusCodes.Status404NotFound)
            .ProducesProblem(statusCode: StatusCodes.Status409Conflict)
            .ProducesProblem(statusCode: StatusCodes.Status429TooManyRequests);
    }
}
