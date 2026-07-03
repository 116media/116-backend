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

namespace _116.Content.Application.Commerce.UseCases.Admin.Commands.RemoveOrderItem.V1;

/// <summary>
/// Response model for removing an order item.
/// </summary>
/// <param name="IsSuccess">Indicates whether the item was successfully removed.</param>
public record AdminRemoveOrderItemResponse(bool IsSuccess);

/// <summary>
/// Defines the admin remove order item endpoint.
/// </summary>
public class AdminRemoveOrderItemEndpointV1 : ICarterModule
{
    /// <inheritdoc />
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        RouteGroupBuilder group = app.MapApiVersionGroup(1)
            .MapGroup($"{ContentConstants.Admin}/{CommerceRouteConstants.Orders}")
            .WithTags($"{ContentConstants.Admin}::{CommerceRouteConstants.Orders}");

        group
            .MapDelete(
                $"/{{id}}/{CommerceRouteConstants.Items}/{{itemId}}",
                async (string id, string itemId, IDispatcher dispatcher) =>
                {
                    var command = new AdminRemoveOrderItemCommand(OrderId: id, ItemId: itemId);
                    AdminRemoveOrderItemResult result = await dispatcher.Send(request: command);

                    var response = new AdminRemoveOrderItemResponse(IsSuccess: result.IsSuccess);
                    return Results.Ok(response);
                }
            )
            .WithName(endpointName: AdminRemoveOrderItemMetaField.RemoveOrderItem.Name)
            .WithSummary(summary: AdminRemoveOrderItemMetaField.RemoveOrderItem.Summary)
            .WithDescription(description: AdminRemoveOrderItemMetaField.RemoveOrderItem.Description)
            .WithAuthorization(AccountStatusPolicies.RequireActiveUser)
            .WithAuthorization(UserRolePolicies.RequireAdminOrSuperAdmin)
            .RequireRateLimiting(policyName: RateLimitPolicies.ContentBrowsing)
            .Produces<AdminRemoveOrderItemResponse>(statusCode: StatusCodes.Status200OK)
            .ProducesProblem(statusCode: StatusCodes.Status400BadRequest)
            .ProducesProblem(statusCode: StatusCodes.Status401Unauthorized)
            .ProducesProblem(statusCode: StatusCodes.Status403Forbidden)
            .ProducesProblem(statusCode: StatusCodes.Status404NotFound)
            .ProducesProblem(statusCode: StatusCodes.Status429TooManyRequests);
    }
}
