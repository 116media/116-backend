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

namespace _116.Content.Application.Commerce.UseCases.Admin.Commands.AttachPaymentProof.V1;

/// <summary>
/// Response model for attaching a payment proof.
/// </summary>
/// <param name="Proof">Metadata of the uploaded proof file, including MIME type for frontend rendering.</param>
public record AdminAttachPaymentProofResponse(FileDto Proof);

/// <summary>
/// Defines the admin attach payment proof endpoint.
/// </summary>
public class AdminAttachPaymentProofEndpointV1 : ICarterModule
{
    /// <inheritdoc />
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        RouteGroupBuilder group = app.MapApiVersionGroup(1)
            .MapGroup($"{ContentConstants.Admin}/{CommerceRouteConstants.Orders}")
            .WithTags($"{ContentConstants.Admin}::{CommerceRouteConstants.Orders}");

        group
            .MapPost(
                $"/{{id}}/{CommerceRouteConstants.Payment}/{CommerceRouteConstants.Proof}",
                async (string id, IFormFile file, EnumPaymentMethod paymentMethod, IDispatcher dispatcher) =>
                {
                    var command = new AdminAttachPaymentProofCommand(
                        OrderId: id,
                        File: file,
                        PaymentMethod: paymentMethod
                    );

                    AdminAttachPaymentProofResult result = await dispatcher.Send(request: command);

                    var response = new AdminAttachPaymentProofResponse(Proof: result.Proof);
                    return Results.Ok(response);
                }
            )
            .DisableAntiforgery()
            .WithName(endpointName: AdminAttachPaymentProofMetaField.AdminAttachPaymentProof.Name)
            .WithSummary(summary: AdminAttachPaymentProofMetaField.AdminAttachPaymentProof.Summary)
            .WithDescription(description: AdminAttachPaymentProofMetaField.AdminAttachPaymentProof.Description)
            .WithAuthorization(AccountStatusPolicies.RequireActiveUser)
            .WithAuthorization(UserRolePolicies.RequireAdminOrSuperAdmin)
            .RequireRateLimiting(policyName: RateLimitPolicies.FileUpload)
            .ProducesValidationProblem()
            .Produces<AdminAttachPaymentProofResponse>(statusCode: StatusCodes.Status200OK)
            .ProducesProblem(statusCode: StatusCodes.Status400BadRequest)
            .ProducesProblem(statusCode: StatusCodes.Status401Unauthorized)
            .ProducesProblem(statusCode: StatusCodes.Status403Forbidden)
            .ProducesProblem(statusCode: StatusCodes.Status404NotFound)
            .ProducesProblem(statusCode: StatusCodes.Status429TooManyRequests);
    }
}
