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

namespace _116.Content.Application.Lookup.UseCases.Admin.Commands.DeactivatePromotionLevel.V1;

/// <summary>
/// Response model for a successful promotion level deactivation.
/// </summary>
/// <param name="PromotionLevel">The updated promotion level information.</param>
public record AdminDeactivatePromotionLevelResponse(PromotionLevelDto PromotionLevel);

/// <summary>
/// Defines the admin deactivate promotion level endpoint.
/// </summary>
public class AdminDeactivatePromotionLevelEndpointV1 : ICarterModule
{
    /// <summary>
    /// Configures the promotion level deactivation route within the API pipeline.
    /// Maps the <c>PATCH /api/v1/admin/promotion-levels/{id:guid}/deactivate</c> endpoint to handle promotion level deactivation requests.
    /// </summary>
    /// <param name="app">The route builder used to register API endpoints.</param>
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        RouteGroupBuilder group = app.MapApiVersionGroup(1)
            .MapGroup($"{ContentConstants.Admin}/{LookupRouteConstants.PromotionLevels}")
            .WithTags($"{ContentConstants.Admin}::{LookupRouteConstants.PromotionLevels}");

        group
            .MapPatch(
                $"/{{id}}/{LookupRouteConstants.Deactivate}",
                async (string id, IDispatcher dispatcher) =>
                {
                    var command = new AdminDeactivatePromotionLevelCommand(Id: id);
                    AdminDeactivatePromotionLevelResult result = await dispatcher.Send(request: command);
                    return Results.Ok(new AdminDeactivatePromotionLevelResponse(PromotionLevel: result.PromotionLevel));
                }
            )
            .WithName(endpointName: AdminDeactivatePromotionLevelMetaField.AdminDeactivatePromotionLevel.Name)
            .WithSummary(summary: AdminDeactivatePromotionLevelMetaField.AdminDeactivatePromotionLevel.Summary)
            .WithDescription(
                description: AdminDeactivatePromotionLevelMetaField.AdminDeactivatePromotionLevel.Description
            )
            .WithAuthorization(AccountStatusPolicies.RequireActiveUser)
            .WithAuthorization(UserRolePolicies.RequireSuperAdminOnly)
            .RequireRateLimiting(policyName: RateLimitPolicies.ContentBrowsing)
            .Produces<AdminDeactivatePromotionLevelResponse>(statusCode: StatusCodes.Status200OK)
            .ProducesProblem(statusCode: StatusCodes.Status401Unauthorized)
            .ProducesProblem(statusCode: StatusCodes.Status403Forbidden)
            .ProducesProblem(statusCode: StatusCodes.Status404NotFound)
            .ProducesProblem(statusCode: StatusCodes.Status409Conflict)
            .ProducesProblem(statusCode: StatusCodes.Status429TooManyRequests);
    }
}
