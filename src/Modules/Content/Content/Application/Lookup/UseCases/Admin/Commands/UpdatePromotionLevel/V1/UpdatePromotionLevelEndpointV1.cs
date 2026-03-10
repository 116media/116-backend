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

namespace _116.Content.Application.Lookup.UseCases.Admin.Commands.UpdatePromotionLevel.V1;

/// <summary>
/// Request model for updating a promotion level.
/// </summary>
/// <param name="Name">The new name for the promotion level.</param>
/// <param name="DurationDays">The new promotion duration in days.</param>
/// <param name="PriceUsd">The new price in US dollars.</param>
public record UpdatePromotionLevelRequest(string Name, int DurationDays, decimal PriceUsd);

/// <summary>
/// Response model for a successful promotion level update.
/// </summary>
/// <param name="PromotionLevel">The updated promotion level information.</param>
public record UpdatePromotionLevelResponse(PromotionLevelDto PromotionLevel);

/// <summary>
/// Defines the admin update promotion level endpoint.
/// Handles updating the name, duration, and price of an existing promotion level.
/// </summary>
public class UpdatePromotionLevelEndpointV1 : ICarterModule
{
    /// <summary>
    /// Configures the promotion level update route within the API pipeline.
    /// Maps the <c>PUT /api/v1/admin/promotion-levels/{id:guid}</c> endpoint to handle promotion level update requests.
    /// </summary>
    /// <param name="app">The route builder used to register API endpoints.</param>
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        RouteGroupBuilder group = app.MapApiVersionGroup(1)
            .MapGroup($"{ContentConstants.Admin}/{LookupRouteConstants.PromotionLevels}")
            .WithTags($"{ContentConstants.Admin}::{LookupRouteConstants.PromotionLevels}");

        group
            .MapPut(
                "/{id:guid}",
                async (Guid id, UpdatePromotionLevelRequest request, IDispatcher dispatcher) =>
                {
                    var command = new UpdatePromotionLevelCommand(
                        Id: id,
                        Name: request.Name,
                        DurationDays: request.DurationDays,
                        PriceUsd: request.PriceUsd
                    );

                    UpdatePromotionLevelResult result = await dispatcher.Send(request: command);

                    var response = new UpdatePromotionLevelResponse(PromotionLevel: result.PromotionLevel);

                    return Results.Ok(response);
                }
            )
            .WithName(endpointName: UpdatePromotionLevelMetaField.UpdatePromotionLevel.Name)
            .WithSummary(summary: UpdatePromotionLevelMetaField.UpdatePromotionLevel.Summary)
            .WithDescription(description: UpdatePromotionLevelMetaField.UpdatePromotionLevel.Description)
            .WithAuthorization(AccountStatusPolicies.RequireActiveUser)
            .WithAuthorization(UserRolePolicies.RequireSuperAdminOnly)
            .RequireRateLimiting(policyName: RateLimitPolicies.ContentBrowsing)
            .ProducesValidationProblem()
            .Produces<UpdatePromotionLevelResponse>(statusCode: StatusCodes.Status200OK)
            .ProducesProblem(statusCode: StatusCodes.Status400BadRequest)
            .ProducesProblem(statusCode: StatusCodes.Status401Unauthorized)
            .ProducesProblem(statusCode: StatusCodes.Status403Forbidden)
            .ProducesProblem(statusCode: StatusCodes.Status404NotFound)
            .ProducesProblem(statusCode: StatusCodes.Status409Conflict)
            .ProducesProblem(statusCode: StatusCodes.Status429TooManyRequests);
    }
}
