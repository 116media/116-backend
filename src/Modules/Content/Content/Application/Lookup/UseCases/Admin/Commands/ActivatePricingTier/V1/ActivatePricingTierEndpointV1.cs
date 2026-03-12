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

namespace _116.Content.Application.Lookup.UseCases.Admin.Commands.ActivatePricingTier.V1;

/// <summary>
/// Response model for a successful pricing tier activation.
/// </summary>
/// <param name="PricingTier">The updated pricing tier information.</param>
public record ActivatePricingTierResponse(PricingTierDto PricingTier);

/// <summary>
/// Defines the admin activate pricing tier endpoint.
/// </summary>
public class ActivatePricingTierEndpointV1 : ICarterModule
{
    /// <summary>
    /// Configures the pricing tier activation route within the API pipeline.
    /// Maps the <c>PATCH /api/v1/admin/pricing-tiers/{id:guid}/activate</c> endpoint to handle pricing tier activation requests.
    /// </summary>
    /// <param name="app">The route builder used to register API endpoints.</param>
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        RouteGroupBuilder group = app.MapApiVersionGroup(1)
            .MapGroup($"{ContentConstants.Admin}/{LookupRouteConstants.PricingTiers}")
            .WithTags($"{ContentConstants.Admin}::{LookupRouteConstants.PricingTiers}");

        group
            .MapPatch(
                $"/{{id}}/{LookupRouteConstants.Activate}",
                async (string id, IDispatcher dispatcher) =>
                {
                    var command = new ActivatePricingTierCommand(Id: id);
                    ActivatePricingTierResult result = await dispatcher.Send(request: command);
                    return Results.Ok(new ActivatePricingTierResponse(PricingTier: result.PricingTier));
                }
            )
            .WithName(endpointName: ActivatePricingTierMetaField.ActivatePricingTier.Name)
            .WithSummary(summary: ActivatePricingTierMetaField.ActivatePricingTier.Summary)
            .WithDescription(description: ActivatePricingTierMetaField.ActivatePricingTier.Description)
            .WithAuthorization(AccountStatusPolicies.RequireActiveUser)
            .WithAuthorization(UserRolePolicies.RequireSuperAdminOnly)
            .RequireRateLimiting(policyName: RateLimitPolicies.ContentBrowsing)
            .Produces<ActivatePricingTierResponse>(statusCode: StatusCodes.Status200OK)
            .ProducesProblem(statusCode: StatusCodes.Status401Unauthorized)
            .ProducesProblem(statusCode: StatusCodes.Status403Forbidden)
            .ProducesProblem(statusCode: StatusCodes.Status404NotFound)
            .ProducesProblem(statusCode: StatusCodes.Status409Conflict)
            .ProducesProblem(statusCode: StatusCodes.Status429TooManyRequests);
    }
}
