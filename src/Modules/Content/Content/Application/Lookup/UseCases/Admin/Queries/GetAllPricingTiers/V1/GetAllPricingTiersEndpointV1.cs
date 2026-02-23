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

namespace _116.Content.Application.Lookup.UseCases.Admin.Queries.GetAllPricingTiers.V1;

/// <summary>
/// Response model for listing all pricing tiers.
/// </summary>
/// <param name="PricingTiers">The list of pricing tiers.</param>
public record GetAllPricingTiersResponse(IReadOnlyList<PricingTierDto> PricingTiers);

/// <summary>
/// Defines the admin get all pricing tiers endpoint.
/// Returns all pricing tiers available for category pricing configuration.
/// </summary>
public class GetAllPricingTiersEndpointV1 : ICarterModule
{
    /// <inheritdoc />
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        RouteGroupBuilder group = app.MapApiVersionGroup(1)
            .MapGroup($"{ContentConstants.Admin}/{LookupRouteConstants.PricingTiers}")
            .WithTags($"{ContentConstants.Admin}::{LookupRouteConstants.PricingTiers}");

        group
            .MapGet(
                "/",
                async (IDispatcher dispatcher) =>
                {
                    var query = new GetAllPricingTiersQuery();

                    GetAllPricingTiersResult result = await dispatcher.Send(request: query);

                    var response = new GetAllPricingTiersResponse(PricingTiers: result.PricingTiers);

                    return Results.Ok(response);
                }
            )
            .WithName(endpointName: GetAllPricingTiersMetaField.GetAllPricingTiers.Name)
            .WithSummary(summary: GetAllPricingTiersMetaField.GetAllPricingTiers.Summary)
            .WithDescription(description: GetAllPricingTiersMetaField.GetAllPricingTiers.Description)
            .RequireAuthorization(AccountStatusPolicies.RequireActiveUser)
            .RequireAuthorization(UserRolePolicies.RequireAdminOrSuperAdmin)
            .RequireRateLimiting(policyName: RateLimitPolicies.ContentBrowsing)
            .Produces<GetAllPricingTiersResponse>(statusCode: StatusCodes.Status200OK)
            .ProducesProblem(statusCode: StatusCodes.Status401Unauthorized)
            .ProducesProblem(statusCode: StatusCodes.Status403Forbidden)
            .ProducesProblem(statusCode: StatusCodes.Status429TooManyRequests);
    }
}
