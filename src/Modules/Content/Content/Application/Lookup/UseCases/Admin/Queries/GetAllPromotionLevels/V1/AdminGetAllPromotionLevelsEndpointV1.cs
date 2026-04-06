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

namespace _116.Content.Application.Lookup.UseCases.Admin.Queries.GetAllPromotionLevels.V1;

/// <summary>
/// Response model for listing all promotion levels.
/// </summary>
/// <param name="PromotionLevels">The list of promotion levels.</param>
public record AdminGetAllPromotionLevelsResponse(IReadOnlyList<PromotionLevelDto> PromotionLevels);

/// <summary>
/// Defines the admin get all promotion levels endpoint.
/// Returns all promotion levels for admin order management.
/// </summary>
public class AdminGetAllPromotionLevelsEndpointV1 : ICarterModule
{
    /// <summary>
    /// Configures the promotion level retrieval route within the API pipeline.
    /// Maps the <c>GET /api/v1/admin/promotion-levels</c> endpoint to handle promotion level retrieval requests.
    /// </summary>
    /// <param name="app">The route builder used to register API endpoints.</param>
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        RouteGroupBuilder group = app.MapApiVersionGroup(1)
            .MapGroup($"{ContentConstants.Admin}/{LookupRouteConstants.PromotionLevels}")
            .WithTags($"{ContentConstants.Admin}::{LookupRouteConstants.PromotionLevels}");

        group
            .MapGet(
                "/",
                async (IDispatcher dispatcher, string? search = null) =>
                {
                    var query = new AdminGetAllPromotionLevelsQuery(Search: search);

                    AdminGetAllPromotionLevelsResult result = await dispatcher.Send(request: query);

                    var response = new AdminGetAllPromotionLevelsResponse(PromotionLevels: result.PromotionLevels);
                    return Results.Ok(response);
                }
            )
            .WithName(endpointName: AdminGetAllPromotionLevelsMetaField.AdminGetAllPromotionLevels.Name)
            .WithSummary(summary: AdminGetAllPromotionLevelsMetaField.AdminGetAllPromotionLevels.Summary)
            .WithDescription(description: AdminGetAllPromotionLevelsMetaField.AdminGetAllPromotionLevels.Description)
            .WithAuthorization(AccountStatusPolicies.RequireActiveUser)
            .WithAuthorization(UserRolePolicies.RequireAdminOrSuperAdmin)
            .RequireRateLimiting(policyName: RateLimitPolicies.ContentBrowsing)
            .Produces<AdminGetAllPromotionLevelsResponse>(statusCode: StatusCodes.Status200OK)
            .ProducesProblem(statusCode: StatusCodes.Status401Unauthorized)
            .ProducesProblem(statusCode: StatusCodes.Status403Forbidden)
            .ProducesProblem(statusCode: StatusCodes.Status429TooManyRequests);
    }
}
