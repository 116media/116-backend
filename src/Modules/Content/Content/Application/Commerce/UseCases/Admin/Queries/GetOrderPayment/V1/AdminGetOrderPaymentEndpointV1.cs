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

namespace _116.Content.Application.Commerce.UseCases.Admin.Queries.GetOrderPayment.V1;

/// <summary>
/// Response model for retrieving an order's payment.
/// </summary>
/// <param name="Payment">The payment details.</param>
public record AdminGetOrderPaymentResponse(PaymentDto Payment);

/// <summary>
/// Defines the admin get order payment endpoint.
/// </summary>
public class AdminGetOrderPaymentEndpointV1 : ICarterModule
{
    /// <inheritdoc />
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        RouteGroupBuilder group = app.MapApiVersionGroup(1)
            .MapGroup($"{ContentConstants.Admin}/{CommerceRouteConstants.Orders}")
            .WithTags($"{ContentConstants.Admin}::{CommerceRouteConstants.Orders}");

        group
            .MapGet(
                $"/{{id}}/{CommerceRouteConstants.Payment}",
                async (string id, IDispatcher dispatcher) =>
                {
                    var query = new AdminGetOrderPaymentQuery(OrderId: id);

                    AdminGetOrderPaymentResult result = await dispatcher.Send(request: query);

                    var response = new AdminGetOrderPaymentResponse(Payment: result.Payment);
                    return Results.Ok(response);
                }
            )
            .WithName(endpointName: AdminGetOrderPaymentMetaField.AdminGetOrderPayment.Name)
            .WithSummary(summary: AdminGetOrderPaymentMetaField.AdminGetOrderPayment.Summary)
            .WithDescription(description: AdminGetOrderPaymentMetaField.AdminGetOrderPayment.Description)
            .WithAuthorization(AccountStatusPolicies.RequireActiveUser)
            .WithAuthorization(UserRolePolicies.RequireAdminOrSuperAdmin)
            .RequireRateLimiting(policyName: RateLimitPolicies.ContentBrowsing)
            .Produces<AdminGetOrderPaymentResponse>(statusCode: StatusCodes.Status200OK)
            .ProducesProblem(statusCode: StatusCodes.Status401Unauthorized)
            .ProducesProblem(statusCode: StatusCodes.Status403Forbidden)
            .ProducesProblem(statusCode: StatusCodes.Status404NotFound)
            .ProducesProblem(statusCode: StatusCodes.Status429TooManyRequests);
    }
}
