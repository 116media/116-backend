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

namespace _116.Content.Application.Commerce.UseCases.Admin.Commands.RemoveItemTier.V1;

/// <summary>
/// Response model for removing a pricing tier from an order item.
/// </summary>
/// <param name="IsSuccess">Indicates whether the tier was successfully removed.</param>
public record AdminRemoveItemTierResponse(bool IsSuccess);

/// <summary>
/// Defines the admin remove item tier endpoint.
/// </summary>
public class AdminRemoveItemTierEndpointV1 : ICarterModule
{
    /// <inheritdoc />
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        RouteGroupBuilder group = app.MapApiVersionGroup(1)
            .MapGroup($"{ContentConstants.Admin}/{CommerceRouteConstants.Orders}")
            .WithTags($"{ContentConstants.Admin}::{CommerceRouteConstants.Orders}");

        group
            .MapDelete(
                $"/{{id}}/{CommerceRouteConstants.Items}/{{itemId}}/{CommerceRouteConstants.Tiers}/{{tierId}}",
                async (string id, string itemId, string tierId, IDispatcher dispatcher) =>
                {
                    var command = new AdminRemoveItemTierCommand(OrderId: id, ItemId: itemId, TierId: tierId);

                    AdminRemoveItemTierResult result = await dispatcher.Send(request: command);

                    var response = new AdminRemoveItemTierResponse(IsSuccess: result.IsSuccess);
                    return Results.Ok(response);
                }
            )
            .WithName(endpointName: AdminRemoveItemTierMetaField.RemoveItemTier.Name)
            .WithSummary(summary: AdminRemoveItemTierMetaField.RemoveItemTier.Summary)
            .WithDescription(description: AdminRemoveItemTierMetaField.RemoveItemTier.Description)
            .WithAuthorization(AccountStatusPolicies.RequireActiveUser)
            .WithAuthorization(UserRolePolicies.RequireAdminOrSuperAdmin)
            .RequireRateLimiting(policyName: RateLimitPolicies.ContentBrowsing)
            .Produces<AdminRemoveItemTierResponse>(statusCode: StatusCodes.Status200OK)
            .ProducesProblem(statusCode: StatusCodes.Status400BadRequest)
            .ProducesProblem(statusCode: StatusCodes.Status401Unauthorized)
            .ProducesProblem(statusCode: StatusCodes.Status403Forbidden)
            .ProducesProblem(statusCode: StatusCodes.Status404NotFound)
            .ProducesProblem(statusCode: StatusCodes.Status429TooManyRequests);
    }
}
