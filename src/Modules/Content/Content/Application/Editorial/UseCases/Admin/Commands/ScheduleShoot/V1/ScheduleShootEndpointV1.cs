using _116.BuildingBlocks.Constants.Authorization.Policies;
using _116.BuildingBlocks.Constants.RateLimit;
using _116.Content.Application.Editorial.Constants;
using _116.Content.Domain.Constants;
using _116.Shared.Application.Extensions;
using _116.Shared.Contracts.Application.CQRS;
using Carter;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace _116.Content.Application.Editorial.UseCases.Admin.Commands.ScheduleShoot.V1;

/// <summary>
/// Request model for scheduling a video shoot.
/// </summary>
/// <param name="ShootingScheduledAt">The scheduled shooting date (must be in the future).</param>
public record ScheduleShootRequest(DateTimeOffset ShootingScheduledAt);

/// <summary>
/// Defines the admin schedule shoot endpoint.
/// Handles scheduling or updating a video's shooting date.
/// </summary>
public class ScheduleShootEndpointV1 : ICarterModule
{
    /// <summary>
    /// Configures the schedule shoot route within the API pipeline.
    /// Maps the <c>PATCH /api/v1/admin/videos/{id}/shoot</c> endpoint.
    /// </summary>
    /// <param name="app">The route builder used to register API endpoints.</param>
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        RouteGroupBuilder group = app.MapApiVersionGroup(1)
            .MapGroup($"{ContentConstants.Admin}/{EditorialRouteConstants.Videos}")
            .WithTags($"{ContentConstants.Admin}::{EditorialRouteConstants.Videos}");

        group
            .MapPatch(
                $"/{{id}}/{EditorialRouteConstants.Shoot}",
                async (string id, ScheduleShootRequest request, IDispatcher dispatcher) =>
                {
                    var command = new ScheduleShootCommand(
                        VideoId: id,
                        ShootingScheduledAt: request.ShootingScheduledAt
                    );

                    await dispatcher.Send(request: command);
                    return Results.NoContent();
                }
            )
            .WithName(endpointName: ScheduleShootMetaField.ScheduleShoot.Name)
            .WithSummary(summary: ScheduleShootMetaField.ScheduleShoot.Summary)
            .WithDescription(description: ScheduleShootMetaField.ScheduleShoot.Description)
            .WithAuthorization(AccountStatusPolicies.RequireActiveUser)
            .WithAuthorization(UserRolePolicies.RequireAdminOrSuperAdmin)
            .RequireRateLimiting(policyName: RateLimitPolicies.ContentBrowsing)
            .ProducesValidationProblem()
            .Produces(statusCode: StatusCodes.Status204NoContent)
            .ProducesProblem(statusCode: StatusCodes.Status400BadRequest)
            .ProducesProblem(statusCode: StatusCodes.Status401Unauthorized)
            .ProducesProblem(statusCode: StatusCodes.Status403Forbidden)
            .ProducesProblem(statusCode: StatusCodes.Status404NotFound)
            .ProducesProblem(statusCode: StatusCodes.Status429TooManyRequests);
    }
}
