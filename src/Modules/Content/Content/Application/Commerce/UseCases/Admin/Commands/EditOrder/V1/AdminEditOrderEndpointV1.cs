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

namespace _116.Content.Application.Commerce.UseCases.Admin.Commands.EditOrder.V1;

/// <summary>
/// Request model for editing a content order.
/// </summary>
/// <param name="CustomerId">The new customer ID, or null to keep the current one.</param>
/// <param name="PackageId">The new package ID, or null to clear it.</param>
public record AdminEditOrderRequest(string? CustomerId, Guid? PackageId);

/// <summary>
/// Response model for successful order editing.
/// </summary>
/// <param name="Order">The updated order summary.</param>
public record AdminEditOrderResponse(ContentOrderSummaryDto Order);

/// <summary>
/// Defines the admin edit order endpoint.
/// Handles editing of draft content orders.
/// </summary>
public class AdminEditOrderEndpointV1 : ICarterModule
{
    /// <inheritdoc />
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        RouteGroupBuilder group = app.MapApiVersionGroup(1)
            .MapGroup($"{ContentConstants.Admin}/{CommerceRouteConstants.Orders}")
            .WithTags($"{ContentConstants.Admin}::{CommerceRouteConstants.Orders}");

        group
            .MapPatch(
                "/{id}",
                async (string id, AdminEditOrderRequest request, IDispatcher dispatcher) =>
                {
                    var command = new AdminEditOrderCommand(
                        OrderId: id,
                        CustomerId: request.CustomerId,
                        PackageId: request.PackageId
                    );

                    AdminEditOrderResult result = await dispatcher.Send(request: command);

                    var response = new AdminEditOrderResponse(Order: result.Order);
                    return Results.Ok(response);
                }
            )
            .WithName(endpointName: AdminEditOrderMetaField.EditOrder.Name)
            .WithSummary(summary: AdminEditOrderMetaField.EditOrder.Summary)
            .WithDescription(description: AdminEditOrderMetaField.EditOrder.Description)
            .WithAuthorization(AccountStatusPolicies.RequireActiveUser)
            .WithAuthorization(UserRolePolicies.RequireAdminOrSuperAdmin)
            .RequireRateLimiting(policyName: RateLimitPolicies.ContentBrowsing)
            .ProducesValidationProblem()
            .Produces<AdminEditOrderResponse>(statusCode: StatusCodes.Status200OK)
            .ProducesProblem(statusCode: StatusCodes.Status400BadRequest)
            .ProducesProblem(statusCode: StatusCodes.Status401Unauthorized)
            .ProducesProblem(statusCode: StatusCodes.Status403Forbidden)
            .ProducesProblem(statusCode: StatusCodes.Status404NotFound)
            .ProducesProblem(statusCode: StatusCodes.Status429TooManyRequests);
    }
}
