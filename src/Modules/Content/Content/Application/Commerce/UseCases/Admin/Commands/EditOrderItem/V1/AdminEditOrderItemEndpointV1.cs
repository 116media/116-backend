using _116.BuildingBlocks.Constants.Authorization.Policies;
using _116.BuildingBlocks.Constants.RateLimit;
using _116.Content.Application.Commerce.Constants;
using _116.Content.Application.Shared.DTOs;
using _116.Content.Domain.Constants;
using _116.Content.Domain.Enums;
using _116.Shared.Application.Extensions;
using _116.Shared.Contracts.Application.CQRS;
using Carter;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace _116.Content.Application.Commerce.UseCases.Admin.Commands.EditOrderItem.V1;

/// <summary>
/// Request model for editing an order item.
/// </summary>
/// <param name="ContentKind">The new content kind, or null to keep the current one.</param>
/// <param name="CategoryId">The new category ID, or null to keep the current one.</param>
/// <param name="PromotionLevelId">The new promotion level ID, or null to clear it.</param>
/// <param name="SocialBoost">The new social boost flag, or null to keep the current one.</param>
/// <param name="IsBonus">The new bonus flag, or null to keep the current one.</param>
public record AdminEditOrderItemRequest(
    EnumCoreContentType? ContentKind,
    string? CategoryId,
    Guid? PromotionLevelId,
    bool? SocialBoost,
    bool? IsBonus
);

/// <summary>
/// Response model for successful order item editing.
/// </summary>
/// <param name="Item">The updated order item.</param>
public record AdminEditOrderItemResponse(OrderItemDto Item);

/// <summary>
/// Defines the admin edit order item endpoint.
/// </summary>
public class AdminEditOrderItemEndpointV1 : ICarterModule
{
    /// <inheritdoc />
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        RouteGroupBuilder group = app.MapApiVersionGroup(1)
            .MapGroup($"{ContentConstants.Admin}/{CommerceRouteConstants.Orders}")
            .WithTags($"{ContentConstants.Admin}::{CommerceRouteConstants.Orders}");

        group
            .MapPatch(
                $"/{{id}}/{CommerceRouteConstants.Items}/{{itemId}}",
                async (string id, string itemId, AdminEditOrderItemRequest request, IDispatcher dispatcher) =>
                {
                    var command = new AdminEditOrderItemCommand(
                        OrderId: id,
                        ItemId: itemId,
                        ContentKind: request.ContentKind,
                        CategoryId: request.CategoryId,
                        PromotionLevelId: request.PromotionLevelId,
                        SocialBoost: request.SocialBoost,
                        IsBonus: request.IsBonus
                    );

                    AdminEditOrderItemResult result = await dispatcher.Send(request: command);

                    var response = new AdminEditOrderItemResponse(Item: result.Item);
                    return Results.Ok(response);
                }
            )
            .WithName(endpointName: AdminEditOrderItemMetaField.EditOrderItem.Name)
            .WithSummary(summary: AdminEditOrderItemMetaField.EditOrderItem.Summary)
            .WithDescription(description: AdminEditOrderItemMetaField.EditOrderItem.Description)
            .WithAuthorization(AccountStatusPolicies.RequireActiveUser)
            .WithAuthorization(UserRolePolicies.RequireAdminOrSuperAdmin)
            .RequireRateLimiting(policyName: RateLimitPolicies.ContentBrowsing)
            .ProducesValidationProblem()
            .Produces<AdminEditOrderItemResponse>(statusCode: StatusCodes.Status200OK)
            .ProducesProblem(statusCode: StatusCodes.Status400BadRequest)
            .ProducesProblem(statusCode: StatusCodes.Status401Unauthorized)
            .ProducesProblem(statusCode: StatusCodes.Status403Forbidden)
            .ProducesProblem(statusCode: StatusCodes.Status404NotFound)
            .ProducesProblem(statusCode: StatusCodes.Status429TooManyRequests);
    }
}
