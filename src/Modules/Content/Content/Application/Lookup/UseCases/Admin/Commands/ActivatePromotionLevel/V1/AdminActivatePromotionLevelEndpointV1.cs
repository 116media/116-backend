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

namespace _116.Content.Application.Lookup.UseCases.Admin.Commands.ActivatePromotionLevel.V1;

/// <summary>
/// Response model for a successful promotion level activation.
/// </summary>
/// <param name="PromotionLevel">The updated promotion level information.</param>
public record AdminActivatePromotionLevelResponse(PromotionLevelDto PromotionLevel);

/// <summary>
/// Defines the admin activate promotion level endpoint.
/// </summary>
public class AdminActivatePromotionLevelEndpointV1 : ICarterModule
{
    /// <summary>
    /// Configures the promotion level activation route within the API pipeline.
    /// Maps the <c>PATCH /api/v1/admin/promotion-levels/{id:guid}/activate</c> endpoint to handle promotion level activation requests.
    /// </summary>
    /// <param name="app">The route builder used to register API endpoints.</param>
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        RouteGroupBuilder group = app.MapApiVersionGroup(1)
            .MapGroup($"{ContentConstants.Admin}/{LookupRouteConstants.PromotionLevels}")
            .WithTags($"{ContentConstants.Admin}::{LookupRouteConstants.PromotionLevels}");

        group
            .MapPatch(
                $"/{{id}}/{LookupRouteConstants.Activate}",
                async (string id, IDispatcher dispatcher) =>
                {
                    var command = new AdminActivatePromotionLevelCommand(Id: id);
                    AdminActivatePromotionLevelResult result = await dispatcher.Send(request: command);

                    var response = new AdminActivatePromotionLevelResponse(PromotionLevel: result.PromotionLevel);
                    return Results.Ok(response);
                }
            )
            .WithName(endpointName: AdminActivatePromotionLevelMetaField.ActivatePromotionLevel.Name)
            .WithSummary(summary: AdminActivatePromotionLevelMetaField.ActivatePromotionLevel.Summary)
            .WithDescription(description: AdminActivatePromotionLevelMetaField.ActivatePromotionLevel.Description)
            .WithAuthorization(AccountStatusPolicies.RequireActiveUser)
            .WithAuthorization(UserRolePolicies.RequireSuperAdminOnly)
            .RequireRateLimiting(policyName: RateLimitPolicies.ContentBrowsing)
            .Produces<AdminActivatePromotionLevelResponse>(statusCode: StatusCodes.Status200OK)
            .ProducesProblem(statusCode: StatusCodes.Status401Unauthorized)
            .ProducesProblem(statusCode: StatusCodes.Status403Forbidden)
            .ProducesProblem(statusCode: StatusCodes.Status404NotFound)
            .ProducesProblem(statusCode: StatusCodes.Status409Conflict)
            .ProducesProblem(statusCode: StatusCodes.Status429TooManyRequests);
    }
}
