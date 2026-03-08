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

namespace _116.Content.Application.Lookup.UseCases.Admin.Commands.DeactivatePricingTier.V1;

/// <summary>Response model for a successful pricing tier deactivation.</summary>
/// <param name="PricingTier">The updated pricing tier information.</param>
public record DeactivatePricingTierResponse(PricingTierDto PricingTier);

/// <summary>
/// Defines the admin deactivate pricing tier endpoint.
/// </summary>
public class DeactivatePricingTierEndpointV1 : ICarterModule
{
    /// <inheritdoc />
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        RouteGroupBuilder group = app.MapApiVersionGroup(1)
            .MapGroup($"{ContentConstants.Admin}/{LookupRouteConstants.PricingTiers}")
            .WithTags($"{ContentConstants.Admin}::{LookupRouteConstants.PricingTiers}");

        group
            .MapPatch(
                $"/{{id:guid}}/{LookupRouteConstants.Deactivate}",
                async (Guid id, IDispatcher dispatcher) =>
                {
                    var command = new DeactivatePricingTierCommand(Id: id);
                    DeactivatePricingTierResult result = await dispatcher.Send(request: command);
                    return Results.Ok(new DeactivatePricingTierResponse(PricingTier: result.PricingTier));
                }
            )
            .WithName(endpointName: DeactivatePricingTierMetaField.DeactivatePricingTier.Name)
            .WithSummary(summary: DeactivatePricingTierMetaField.DeactivatePricingTier.Summary)
            .WithDescription(description: DeactivatePricingTierMetaField.DeactivatePricingTier.Description)
            .WithAuthorization(AccountStatusPolicies.RequireActiveUser)
            .WithAuthorization(UserRolePolicies.RequireSuperAdminOnly)
            .RequireRateLimiting(policyName: RateLimitPolicies.ContentBrowsing)
            .Produces<DeactivatePricingTierResponse>(statusCode: StatusCodes.Status200OK)
            .ProducesProblem(statusCode: StatusCodes.Status401Unauthorized)
            .ProducesProblem(statusCode: StatusCodes.Status403Forbidden)
            .ProducesProblem(statusCode: StatusCodes.Status404NotFound)
            .ProducesProblem(statusCode: StatusCodes.Status409Conflict)
            .ProducesProblem(statusCode: StatusCodes.Status429TooManyRequests);
    }
}
