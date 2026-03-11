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
public record AdminScheduleShootRequest(DateTimeOffset ShootingScheduledAt);

/// <summary>
/// Response model for a successful ScheduleShoot operation.
/// </summary>
/// <param name="IsSuccess">Indicates if the operation was successful.</param>
public record AdminScheduleShootResponse(bool IsSuccess);

/// <summary>
/// Defines the admin schedule shoot endpoint.
/// Handles scheduling or updating a video's shooting date.
/// </summary>
public class AdminScheduleShootEndpointV1 : ICarterModule
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
                async (string id, AdminScheduleShootRequest request, IDispatcher dispatcher) =>
                {
                    var command = new AdminScheduleShootCommand(
                        VideoId: id,
                        ShootingScheduledAt: request.ShootingScheduledAt
                    );
                    AdminScheduleShootResult result = await dispatcher.Send(request: command);

                    var response = new AdminScheduleShootResponse(IsSuccess: result.IsSuccess);
                    return Results.Ok(response);
                }
            )
            .WithName(endpointName: AdminScheduleShootMetaField.AdminScheduleShoot.Name)
            .WithSummary(summary: AdminScheduleShootMetaField.AdminScheduleShoot.Summary)
            .WithDescription(description: AdminScheduleShootMetaField.AdminScheduleShoot.Description)
            .WithAuthorization(AccountStatusPolicies.RequireActiveUser)
            .WithAuthorization(UserRolePolicies.RequireAdminOrSuperAdmin)
            .RequireRateLimiting(policyName: RateLimitPolicies.ContentBrowsing)
            .ProducesValidationProblem()
            .Produces<AdminScheduleShootResponse>(statusCode: StatusCodes.Status200OK)
            .ProducesProblem(statusCode: StatusCodes.Status400BadRequest)
            .ProducesProblem(statusCode: StatusCodes.Status401Unauthorized)
            .ProducesProblem(statusCode: StatusCodes.Status403Forbidden)
            .ProducesProblem(statusCode: StatusCodes.Status404NotFound)
            .ProducesProblem(statusCode: StatusCodes.Status429TooManyRequests);
    }
}
