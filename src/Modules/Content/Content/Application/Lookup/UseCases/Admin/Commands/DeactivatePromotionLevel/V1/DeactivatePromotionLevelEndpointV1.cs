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

/// <summary>Response model for a successful promotion level deactivation.</summary>
/// <param name="PromotionLevel">The updated promotion level information.</param>
public record DeactivatePromotionLevelResponse(PromotionLevelDto PromotionLevel);

/// <summary>
/// Defines the admin deactivate promotion level endpoint.
/// </summary>
public class DeactivatePromotionLevelEndpointV1 : ICarterModule
{
    /// <inheritdoc />
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        RouteGroupBuilder group = app.MapApiVersionGroup(1)
            .MapGroup($"{ContentConstants.Admin}/{LookupRouteConstants.PromotionLevels}")
            .WithTags($"{ContentConstants.Admin}::{LookupRouteConstants.PromotionLevels}");

        group
            .MapPatch(
                $"/{{id:guid}}/{LookupRouteConstants.Deactivate}",
                async (Guid id, IDispatcher dispatcher) =>
                {
                    var command = new DeactivatePromotionLevelCommand(Id: id);
                    DeactivatePromotionLevelResult result = await dispatcher.Send(request: command);
                    return Results.Ok(new DeactivatePromotionLevelResponse(PromotionLevel: result.PromotionLevel));
                }
            )
            .WithName(endpointName: DeactivatePromotionLevelMetaField.DeactivatePromotionLevel.Name)
            .WithSummary(summary: DeactivatePromotionLevelMetaField.DeactivatePromotionLevel.Summary)
            .WithDescription(description: DeactivatePromotionLevelMetaField.DeactivatePromotionLevel.Description)
            .RequireAuthorization(AccountStatusPolicies.RequireActiveUser)
            .RequireAuthorization(UserRolePolicies.RequireSuperAdminOnly)
            .RequireRateLimiting(policyName: RateLimitPolicies.ContentBrowsing)
            .Produces<DeactivatePromotionLevelResponse>(statusCode: StatusCodes.Status200OK)
            .ProducesProblem(statusCode: StatusCodes.Status401Unauthorized)
            .ProducesProblem(statusCode: StatusCodes.Status403Forbidden)
            .ProducesProblem(statusCode: StatusCodes.Status404NotFound)
            .ProducesProblem(statusCode: StatusCodes.Status409Conflict)
            .ProducesProblem(statusCode: StatusCodes.Status429TooManyRequests);
    }
}
