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

namespace _116.Content.Application.Commerce.UseCases.Admin.Commands.RejectPayment.V1;

/// <summary>
/// Request model for rejecting an order payment.
/// </summary>
/// <param name="Notes">Optional notes explaining the rejection reason.</param>
public record AdminRejectPaymentRequest(string? Notes);

/// <summary>
/// Response model for rejecting an order payment.
/// </summary>
/// <param name="IsSuccess">Indicates whether the payment was successfully rejected.</param>
public record AdminRejectPaymentResponse(bool IsSuccess);

/// <summary>
/// Defines the admin reject payment endpoint.
/// </summary>
public class AdminRejectPaymentEndpointV1 : ICarterModule
{
    /// <inheritdoc />
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        RouteGroupBuilder group = app.MapApiVersionGroup(1)
            .MapGroup($"{ContentConstants.Admin}/{CommerceRouteConstants.Orders}")
            .WithTags($"{ContentConstants.Admin}::{CommerceRouteConstants.Orders}");

        group
            .MapPatch(
                $"/{{id}}/{CommerceRouteConstants.Payment}/{CommerceRouteConstants.Reject}",
                async (string id, AdminRejectPaymentRequest request, IDispatcher dispatcher) =>
                {
                    var command = new AdminRejectPaymentCommand(OrderId: id, Notes: request.Notes);

                    AdminRejectPaymentResult result = await dispatcher.Send(request: command);

                    var response = new AdminRejectPaymentResponse(IsSuccess: result.IsSuccess);
                    return Results.Ok(response);
                }
            )
            .WithName(endpointName: AdminRejectPaymentMetaField.AdminRejectPayment.Name)
            .WithSummary(summary: AdminRejectPaymentMetaField.AdminRejectPayment.Summary)
            .WithDescription(description: AdminRejectPaymentMetaField.AdminRejectPayment.Description)
            .WithAuthorization(AccountStatusPolicies.RequireActiveUser)
            .WithAuthorization(UserRolePolicies.RequireAdminOrSuperAdmin)
            .RequireRateLimiting(policyName: RateLimitPolicies.ContentBrowsing)
            .Produces<AdminRejectPaymentResponse>(statusCode: StatusCodes.Status200OK)
            .ProducesProblem(statusCode: StatusCodes.Status401Unauthorized)
            .ProducesProblem(statusCode: StatusCodes.Status403Forbidden)
            .ProducesProblem(statusCode: StatusCodes.Status404NotFound)
            .ProducesProblem(statusCode: StatusCodes.Status409Conflict)
            .ProducesProblem(statusCode: StatusCodes.Status429TooManyRequests);
    }
}
