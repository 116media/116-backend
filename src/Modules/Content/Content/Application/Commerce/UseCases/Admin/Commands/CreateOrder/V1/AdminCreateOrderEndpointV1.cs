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

namespace _116.Content.Application.Commerce.UseCases.Admin.Commands.CreateOrder.V1;

/// <summary>
/// Request model for creating a content order.
/// </summary>
/// <param name="CustomerId">The B2B customer placing the order.</param>
/// <param name="PackageId">An optional pre-configured package to apply.</param>
public record AdminCreateOrderRequest(string CustomerId, Guid? PackageId);

/// <summary>
/// Response model for successful order creation.
/// </summary>
/// <param name="Order">The created order summary.</param>
public record AdminCreateOrderResponse(ContentOrderSummaryDto Order);

/// <summary>
/// Defines the admin create order endpoint.
/// Handles creation of new B2B content orders.
/// </summary>
public class AdminCreateOrderEndpointV1 : ICarterModule
{
    /// <inheritdoc />
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        RouteGroupBuilder group = app.MapApiVersionGroup(1)
            .MapGroup($"{ContentConstants.Admin}/{CommerceRouteConstants.Orders}")
            .WithTags($"{ContentConstants.Admin}::{CommerceRouteConstants.Orders}");

        group
            .MapPost(
                "/",
                async (AdminCreateOrderRequest request, IDispatcher dispatcher, HttpContext httpContext) =>
                {
                    var command = new AdminCreateOrderCommand(
                        CustomerId: request.CustomerId,
                        PackageId: request.PackageId
                    );

                    AdminCreateOrderResult result = await dispatcher.Send(request: command);

                    var response = new AdminCreateOrderResponse(Order: result.Order);
                    string path = $"{ContentConstants.Admin}/{CommerceRouteConstants.Orders}/{response.Order.Id}";
                    string locationUrl = ApiVersionUrl.Build(context: httpContext, path: path);

                    return Results.Created(uri: locationUrl, value: response);
                }
            )
            .WithName(endpointName: AdminCreateOrderMetaField.AdminCreateOrder.Name)
            .WithSummary(summary: AdminCreateOrderMetaField.AdminCreateOrder.Summary)
            .WithDescription(description: AdminCreateOrderMetaField.AdminCreateOrder.Description)
            .WithAuthorization(AccountStatusPolicies.RequireActiveUser)
            .WithAuthorization(UserRolePolicies.RequireAdminOrSuperAdmin)
            .RequireRateLimiting(policyName: RateLimitPolicies.ContentBrowsing)
            .ProducesValidationProblem()
            .Produces<AdminCreateOrderResponse>(statusCode: StatusCodes.Status201Created)
            .ProducesProblem(statusCode: StatusCodes.Status400BadRequest)
            .ProducesProblem(statusCode: StatusCodes.Status401Unauthorized)
            .ProducesProblem(statusCode: StatusCodes.Status403Forbidden)
            .ProducesProblem(statusCode: StatusCodes.Status404NotFound)
            .ProducesProblem(statusCode: StatusCodes.Status429TooManyRequests);
    }
}
