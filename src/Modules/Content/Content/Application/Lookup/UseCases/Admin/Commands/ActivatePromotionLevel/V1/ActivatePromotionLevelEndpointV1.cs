using _116.BuildingBlocks.Constants.RateLimit;
using _116.Content.Application.Lookup.Constants;
using _116.Content.Domain.Constants;
using _116.Shared.Application.Extensions;
using _116.Shared.Contracts.Application.CQRS;
using Carter;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace _116.Content.Application.Lookup.UseCases.Admin.Commands.ActivatePromotionLevel.V1;

/// <summary>
/// Defines the admin activate promotion level endpoint.
/// </summary>
public class ActivatePromotionLevelEndpointV1 : ICarterModule
{
    /// <inheritdoc />
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        RouteGroupBuilder group = app.MapApiVersionGroup(1)
            .MapGroup($"{ContentConstants.Admin}/{LookupRouteConstants.PromotionLevels}")
            .WithTags($"{ContentConstants.Admin}::{LookupRouteConstants.PromotionLevels}");

        group
            .MapPatch(
                $"/{{id:guid}}/{LookupRouteConstants.Activate}",
                async (Guid id, IDispatcher dispatcher) =>
                {
                    var command = new ActivatePromotionLevelCommand(Id: id);
                    await dispatcher.Send(request: command);
                    return Results.NoContent();
                }
            )
            .WithName(endpointName: ActivatePromotionLevelMetaField.ActivatePromotionLevel.Name)
            .WithSummary(summary: ActivatePromotionLevelMetaField.ActivatePromotionLevel.Summary)
            .WithDescription(description: ActivatePromotionLevelMetaField.ActivatePromotionLevel.Description)
            .RequireAuthorization()
            .RequireRateLimiting(policyName: RateLimitPolicies.ContentBrowsing)
            .Produces(statusCode: StatusCodes.Status204NoContent)
            .ProducesProblem(statusCode: StatusCodes.Status401Unauthorized)
            .ProducesProblem(statusCode: StatusCodes.Status403Forbidden)
            .ProducesProblem(statusCode: StatusCodes.Status404NotFound)
            .ProducesProblem(statusCode: StatusCodes.Status409Conflict)
            .ProducesProblem(statusCode: StatusCodes.Status429TooManyRequests);
    }
}
