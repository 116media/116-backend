using _116.BuildingBlocks.Constants.Authorization.Policies;
using _116.BuildingBlocks.Constants.RateLimit;
using _116.BuildingBlocks.Utils;
using _116.Content.Application.Commerce.Constants;
using _116.Content.Application.Shared.DTOs;
using _116.Content.Domain.Constants;
using _116.Shared.Application.Extensions;
using _116.Shared.Contracts.Application.CQRS;
using Carter;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace _116.Content.Application.Commerce.UseCases.Admin.Commands.AddItemTier.V1;

/// <summary>
/// Request model for attaching a pricing tier to an order item.
/// </summary>
/// <param name="PricingTierId">The identifier of the pricing tier to snapshot.</param>
public record AdminAddItemTierRequest(string PricingTierId);

/// <summary>
/// Response model for successful tier attachment.
/// </summary>
/// <param name="Tier">The created tier snapshot.</param>
public record AdminAddItemTierResponse(ItemTierDto Tier);

/// <summary>
/// Defines the admin add item tier endpoint.
/// </summary>
public class AdminAddItemTierEndpointV1 : ICarterModule
{
    /// <inheritdoc />
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        RouteGroupBuilder group = app.MapApiVersionGroup(1)
            .MapGroup($"{ContentConstants.Admin}/{CommerceRouteConstants.Orders}")
            .WithTags($"{ContentConstants.Admin}::{CommerceRouteConstants.Orders}");

        group
            .MapPost(
                $"/{{id}}/{CommerceRouteConstants.Items}/{{itemId}}/{CommerceRouteConstants.Tiers}",
                async (
                    string id,
                    string itemId,
                    AdminAddItemTierRequest request,
                    IDispatcher dispatcher,
                    HttpContext httpContext
                ) =>
                {
                    var command = new AdminAddItemTierCommand(
                        OrderId: id,
                        OrderItemId: itemId,
                        PricingTierId: request.PricingTierId
                    );

                    AdminAddItemTierResult result = await dispatcher.Send(request: command);

                    var response = new AdminAddItemTierResponse(Tier: result.Tier);
                    string path =
                        $"{ContentConstants.Admin}/{CommerceRouteConstants.Orders}/{id}/{CommerceRouteConstants.Items}/{itemId}/{CommerceRouteConstants.Tiers}";
                    string locationUrl = ApiVersionUrl.Build(context: httpContext, path: path);

                    return Results.Created(uri: locationUrl, value: response);
                }
            )
            .WithName(endpointName: AdminAddItemTierMetaField.AdminAddItemTier.Name)
            .WithSummary(summary: AdminAddItemTierMetaField.AdminAddItemTier.Summary)
            .WithDescription(description: AdminAddItemTierMetaField.AdminAddItemTier.Description)
            .WithAuthorization(AccountStatusPolicies.RequireActiveUser)
            .WithAuthorization(UserRolePolicies.RequireAdminOrSuperAdmin)
            .RequireRateLimiting(policyName: RateLimitPolicies.ContentBrowsing)
            .ProducesValidationProblem()
            .Produces<AdminAddItemTierResponse>(statusCode: StatusCodes.Status201Created)
            .ProducesProblem(statusCode: StatusCodes.Status400BadRequest)
            .ProducesProblem(statusCode: StatusCodes.Status401Unauthorized)
            .ProducesProblem(statusCode: StatusCodes.Status403Forbidden)
            .ProducesProblem(statusCode: StatusCodes.Status404NotFound)
            .ProducesProblem(statusCode: StatusCodes.Status429TooManyRequests);
    }
}
