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
                async (
                    string id,
                    string itemId,
                    string tierId,
                    IDispatcher dispatcher,
                    CancellationToken cancellationToken
                ) =>
                {
                    var command = new AdminRemoveItemTierCommand(OrderId: id, ItemId: itemId, TierId: tierId);

                    await dispatcher.Send(request: command, cancellationToken: cancellationToken);
                    return Results.NoContent();
                }
            )
            .WithName(endpointName: AdminRemoveItemTierMetaField.RemoveItemTier.Name)
            .WithSummary(summary: AdminRemoveItemTierMetaField.RemoveItemTier.Summary)
            .WithDescription(description: AdminRemoveItemTierMetaField.RemoveItemTier.Description)
            .WithAuthorization(AccountStatusPolicies.RequireActiveUser)
            .WithAuthorization(UserRolePolicies.RequireAdminOrSuperAdmin)
            .RequireRateLimiting(policyName: RateLimitPolicies.ContentManagement)
            .Produces(statusCode: StatusCodes.Status204NoContent)
            .ProducesProblem(statusCode: StatusCodes.Status400BadRequest)
            .ProducesProblem(statusCode: StatusCodes.Status401Unauthorized)
            .ProducesProblem(statusCode: StatusCodes.Status403Forbidden)
            .ProducesProblem(statusCode: StatusCodes.Status404NotFound)
            .ProducesProblem(statusCode: StatusCodes.Status429TooManyRequests);
    }
}
