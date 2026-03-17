using System.Security.Claims;
using _116.BuildingBlocks.Constants.Authorization.Policies;
using _116.BuildingBlocks.Constants.RateLimit;
using _116.Content.Application.Commerce.Constants;
using _116.Content.Domain.Constants;
using _116.Identity.Contracts.Application;
using _116.Shared.Application.Extensions;
using _116.Shared.Contracts.Application.CQRS;
using Carter;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace _116.Content.Application.Commerce.UseCases.Admin.Commands.VerifyPayment.V1;

/// <summary>
/// Request model for verifying an order payment.
/// </summary>
/// <param name="ReceiptUrl">The URL of the official payment receipt.</param>
public record AdminVerifyPaymentRequest(string ReceiptUrl);

/// <summary>
/// Response model for verifying an order payment.
/// </summary>
/// <param name="IsSuccess">Indicates whether the payment was successfully verified.</param>
public record AdminVerifyPaymentResponse(bool IsSuccess);

/// <summary>
/// Defines the admin verify payment endpoint.
/// </summary>
public class AdminVerifyPaymentEndpointV1 : ICarterModule
{
    /// <inheritdoc />
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        RouteGroupBuilder group = app.MapApiVersionGroup(1)
            .MapGroup($"{ContentConstants.Admin}/{CommerceRouteConstants.Orders}")
            .WithTags($"{ContentConstants.Admin}::{CommerceRouteConstants.Orders}");

        group
            .MapPatch(
                $"/{{id}}/{CommerceRouteConstants.Payment}/{CommerceRouteConstants.Verify}",
                async (
                    string id,
                    AdminVerifyPaymentRequest request,
                    ClaimsPrincipal user,
                    IClaimsProvider claimsProvider,
                    IDispatcher dispatcher
                ) =>
                {
                    Guid adminUserId = claimsProvider.GetUserIdFromClaims(user: user);

                    var command = new AdminVerifyPaymentCommand(
                        OrderId: id,
                        ReceiptUrl: request.ReceiptUrl,
                        AdminUserId: adminUserId
                    );

                    AdminVerifyPaymentResult result = await dispatcher.Send(request: command);

                    var response = new AdminVerifyPaymentResponse(IsSuccess: result.IsSuccess);
                    return Results.Ok(response);
                }
            )
            .WithName(endpointName: AdminVerifyPaymentMetaField.AdminVerifyPayment.Name)
            .WithSummary(summary: AdminVerifyPaymentMetaField.AdminVerifyPayment.Summary)
            .WithDescription(description: AdminVerifyPaymentMetaField.AdminVerifyPayment.Description)
            .WithAuthorization(AccountStatusPolicies.RequireActiveUser)
            .WithAuthorization(UserRolePolicies.RequireAdminOrSuperAdmin)
            .RequireRateLimiting(policyName: RateLimitPolicies.ContentBrowsing)
            .ProducesValidationProblem()
            .Produces<AdminVerifyPaymentResponse>(statusCode: StatusCodes.Status200OK)
            .ProducesProblem(statusCode: StatusCodes.Status400BadRequest)
            .ProducesProblem(statusCode: StatusCodes.Status401Unauthorized)
            .ProducesProblem(statusCode: StatusCodes.Status403Forbidden)
            .ProducesProblem(statusCode: StatusCodes.Status404NotFound)
            .ProducesProblem(statusCode: StatusCodes.Status409Conflict)
            .ProducesProblem(statusCode: StatusCodes.Status429TooManyRequests);
    }
}
