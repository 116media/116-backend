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

namespace _116.Content.Application.Lookup.UseCases.Public.Queries.GetActivePromotionLevels.V1;

/// <summary>
/// Response model for listing active public promotion levels.
/// </summary>
/// <param name="PromotionLevels">The list of active promotion levels.</param>
public record PublicGetActivePromotionLevelsResponse(IReadOnlyList<PromotionLevelDto> PromotionLevels);

/// <summary>
/// Defines the public get active promotion levels endpoint.
/// Returns only active promotion levels for content discovery and purchasing.
/// </summary>
public class PublicGetActivePromotionLevelsEndpointV1 : ICarterModule
{
    /// <summary>
    /// Configures the active promotion level retrieval route within the API pipeline.
    /// Maps the <c>GET /api/v1/public/promotion-levels</c> endpoint to handle active promotion level retrieval requests.
    /// </summary>
    /// <param name="app">The route builder used to register API endpoints.</param>
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        RouteGroupBuilder group = app.MapApiVersionGroup(1)
            .MapGroup($"{ContentConstants.Public}/{LookupRouteConstants.PromotionLevels}")
            .WithTags($"{ContentConstants.Public}::{LookupRouteConstants.PromotionLevels}");

        group
            .MapGet(
                "/",
                async (IDispatcher dispatcher) =>
                {
                    var query = new PublicGetActivePromotionLevelsQuery();

                    PublicGetActivePromotionLevelsResult result = await dispatcher.Send(request: query);

                    var response = new PublicGetActivePromotionLevelsResponse(PromotionLevels: result.PromotionLevels);
                    return Results.Ok(response);
                }
            )
            .WithName(endpointName: PublicGetActivePromotionLevelsMetaField.PublicGetActivePromotionLevels.Name)
            .WithSummary(summary: PublicGetActivePromotionLevelsMetaField.PublicGetActivePromotionLevels.Summary)
            .WithDescription(
                description: PublicGetActivePromotionLevelsMetaField.PublicGetActivePromotionLevels.Description
            )
            .AllowAnonymous()
            .RequireRateLimiting(policyName: RateLimitPolicies.ContentBrowsing)
            .Produces<PublicGetActivePromotionLevelsResponse>(statusCode: StatusCodes.Status200OK)
            .ProducesProblem(statusCode: StatusCodes.Status429TooManyRequests);
    }
}
