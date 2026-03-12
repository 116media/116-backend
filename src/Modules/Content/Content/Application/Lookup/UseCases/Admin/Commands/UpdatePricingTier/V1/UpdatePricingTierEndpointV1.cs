using _116.BuildingBlocks.Constants.Authorization.Policies;
using _116.BuildingBlocks.Constants.RateLimit;
using _116.Content.Application.Lookup.Constants;
using _116.Content.Application.Shared.DTOs;
using _116.Content.Domain.Constants;
using _116.Shared.Application.Extensions;
using _116.Shared.Contracts.Application.CQRS;
using Carter;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace _116.Content.Application.Lookup.UseCases.Admin.Commands.UpdatePricingTier.V1;

/// <summary>
/// Request model for updating a pricing tier.
/// </summary>
/// <param name="Name">The new name for the pricing tier.</param>
/// <param name="Description">The new description (may be null).</param>
public record UpdatePricingTierRequest(string Name, string? Description);

/// <summary>
/// Response model for a successful pricing tier update.
/// </summary>
/// <param name="PricingTier">The updated pricing tier information.</param>
public record UpdatePricingTierResponse(PricingTierDto PricingTier);

/// <summary>
/// Defines the admin update pricing tier endpoint.
/// Handles renaming and re-describing an existing pricing tier.
/// </summary>
public class UpdatePricingTierEndpointV1 : ICarterModule
{
    /// <summary>
    /// Configures the pricing tier update route within the API pipeline.
    /// Maps the <c>PUT /api/v1/admin/pricing-tiers/{id:guid}</c> endpoint to handle pricing tier update requests.
    /// </summary>
    /// <param name="app">The route builder used to register API endpoints.</param>
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        RouteGroupBuilder group = app.MapApiVersionGroup(1)
            .MapGroup($"{ContentConstants.Admin}/{LookupRouteConstants.PricingTiers}")
            .WithTags($"{ContentConstants.Admin}::{LookupRouteConstants.PricingTiers}");

        group
            .MapPut(
                "/{id}",
                async (string id, UpdatePricingTierRequest request, IDispatcher dispatcher) =>
                {
                    var command = new UpdatePricingTierCommand(
                        Id: id,
                        Name: request.Name,
                        Description: request.Description
                    );

                    UpdatePricingTierResult result = await dispatcher.Send(request: command);

                    var response = new UpdatePricingTierResponse(PricingTier: result.PricingTier);

                    return Results.Ok(response);
                }
            )
            .WithName(endpointName: UpdatePricingTierMetaField.UpdatePricingTier.Name)
            .WithSummary(summary: UpdatePricingTierMetaField.UpdatePricingTier.Summary)
            .WithDescription(description: UpdatePricingTierMetaField.UpdatePricingTier.Description)
            .WithAuthorization(AccountStatusPolicies.RequireActiveUser)
            .WithAuthorization(UserRolePolicies.RequireSuperAdminOnly)
            .RequireRateLimiting(policyName: RateLimitPolicies.ContentBrowsing)
            .ProducesValidationProblem()
            .Produces<UpdatePricingTierResponse>(statusCode: StatusCodes.Status200OK)
            .ProducesProblem(statusCode: StatusCodes.Status400BadRequest)
            .ProducesProblem(statusCode: StatusCodes.Status401Unauthorized)
            .ProducesProblem(statusCode: StatusCodes.Status403Forbidden)
            .ProducesProblem(statusCode: StatusCodes.Status404NotFound)
            .ProducesProblem(statusCode: StatusCodes.Status409Conflict)
            .ProducesProblem(statusCode: StatusCodes.Status429TooManyRequests);
    }
}
