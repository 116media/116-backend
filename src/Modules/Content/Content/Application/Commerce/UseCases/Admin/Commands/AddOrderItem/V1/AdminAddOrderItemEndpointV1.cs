using _116.BuildingBlocks.Constants.Authorization.Policies;
using _116.BuildingBlocks.Constants.RateLimit;
using _116.BuildingBlocks.Utils;
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

namespace _116.Content.Application.Commerce.UseCases.Admin.Commands.AddOrderItem.V1;

/// <summary>
/// Request model for adding an order item.
/// </summary>
/// <param name="ContentKind">The kind of content being commissioned (Article or Video).</param>
/// <param name="CategoryId">The category for this content item.</param>
/// <param name="PromotionLevelId">An optional homepage promotion level.</param>
/// <param name="SocialBoost">Whether social media promotion is requested.</param>
/// <param name="IsBonus">Whether this is a bonus/complimentary item.</param>
public record AdminAddOrderItemRequest(
    EnumCoreContentType ContentKind,
    string CategoryId,
    Guid? PromotionLevelId,
    bool SocialBoost,
    bool IsBonus
);

/// <summary>
/// Response model for successful order item creation.
/// </summary>
/// <param name="Item">The created order item.</param>
public record AdminAddOrderItemResponse(OrderItemDto Item);

/// <summary>
/// Defines the admin add order item endpoint.
/// </summary>
public class AdminAddOrderItemEndpointV1 : ICarterModule
{
    /// <inheritdoc />
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        RouteGroupBuilder group = app.MapApiVersionGroup(1)
            .MapGroup($"{ContentConstants.Admin}/{CommerceRouteConstants.Orders}")
            .WithTags($"{ContentConstants.Admin}::{CommerceRouteConstants.Orders}");

        group
            .MapPost(
                $"/{{id}}/{CommerceRouteConstants.Items}",
                async (string id, AdminAddOrderItemRequest request, IDispatcher dispatcher, HttpContext httpContext) =>
                {
                    var command = new AdminAddOrderItemCommand(
                        OrderId: id,
                        ContentKind: request.ContentKind,
                        CategoryId: request.CategoryId,
                        PromotionLevelId: request.PromotionLevelId,
                        SocialBoost: request.SocialBoost,
                        IsBonus: request.IsBonus
                    );

                    AdminAddOrderItemResult result = await dispatcher.Send(request: command);

                    var response = new AdminAddOrderItemResponse(Item: result.Item);
                    string path =
                        $"{ContentConstants.Admin}/{CommerceRouteConstants.Orders}/{id}/{CommerceRouteConstants.Items}/{result.Item.Id}";
                    string locationUrl = ApiVersionUrl.Build(context: httpContext, path: path);

                    return Results.Created(uri: locationUrl, value: response);
                }
            )
            .WithName(endpointName: AdminAddOrderItemMetaField.AdminAddOrderItem.Name)
            .WithSummary(summary: AdminAddOrderItemMetaField.AdminAddOrderItem.Summary)
            .WithDescription(description: AdminAddOrderItemMetaField.AdminAddOrderItem.Description)
            .WithAuthorization(AccountStatusPolicies.RequireActiveUser)
            .WithAuthorization(UserRolePolicies.RequireAdminOrSuperAdmin)
            .RequireRateLimiting(policyName: RateLimitPolicies.ContentBrowsing)
            .ProducesValidationProblem()
            .Produces<AdminAddOrderItemResponse>(statusCode: StatusCodes.Status201Created)
            .ProducesProblem(statusCode: StatusCodes.Status400BadRequest)
            .ProducesProblem(statusCode: StatusCodes.Status401Unauthorized)
            .ProducesProblem(statusCode: StatusCodes.Status403Forbidden)
            .ProducesProblem(statusCode: StatusCodes.Status404NotFound)
            .ProducesProblem(statusCode: StatusCodes.Status429TooManyRequests);
    }
}
