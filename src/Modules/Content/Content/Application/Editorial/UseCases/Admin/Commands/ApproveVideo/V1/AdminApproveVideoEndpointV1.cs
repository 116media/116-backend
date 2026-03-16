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

namespace _116.Content.Application.Editorial.UseCases.Admin.Commands.ApproveVideo.V1;

/// <summary>
/// Response model for a successful ApproveVideo operation.
/// </summary>
/// <param name="IsSuccess">Indicates if the operation was successful.</param>
public record AdminApproveVideoResponse(bool IsSuccess);

/// <summary>
/// Defines the admin approve video endpoint.
/// Handles transitioning a video from PendingReview to Approved.
/// </summary>
public class AdminApproveVideoEndpointV1 : ICarterModule
{
    /// <summary>
    /// Configures the video approval route within the API pipeline.
    /// Maps the <c>PATCH /api/v1/admin/videos/{id}/approve</c> endpoint.
    /// </summary>
    /// <param name="app">The route builder used to register API endpoints.</param>
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        RouteGroupBuilder group = app.MapApiVersionGroup(1)
            .MapGroup($"{ContentConstants.Admin}/{EditorialRouteConstants.Videos}")
            .WithTags($"{ContentConstants.Admin}::{EditorialRouteConstants.Videos}");

        group
            .MapPatch(
                $"/{{id}}/{EditorialRouteConstants.Approve}",
                async (string id, IDispatcher dispatcher) =>
                {
                    var command = new AdminApproveVideoCommand(Id: id);
                    AdminApproveVideoResult result = await dispatcher.Send(request: command);

                    var response = new AdminApproveVideoResponse(IsSuccess: result.IsSuccess);
                    return Results.Ok(response);
                }
            )
            .WithName(endpointName: AdminApproveVideoMetaField.AdminApproveVideo.Name)
            .WithSummary(summary: AdminApproveVideoMetaField.AdminApproveVideo.Summary)
            .WithDescription(description: AdminApproveVideoMetaField.AdminApproveVideo.Description)
            .WithAuthorization(AccountStatusPolicies.RequireActiveUser)
            .WithAuthorization(UserRolePolicies.RequireSuperAdminOnly)
            .RequireRateLimiting(policyName: RateLimitPolicies.ContentBrowsing)
            .Produces<AdminApproveVideoResponse>(statusCode: StatusCodes.Status200OK)
            .ProducesProblem(statusCode: StatusCodes.Status400BadRequest)
            .ProducesProblem(statusCode: StatusCodes.Status401Unauthorized)
            .ProducesProblem(statusCode: StatusCodes.Status403Forbidden)
            .ProducesProblem(statusCode: StatusCodes.Status404NotFound)
            .ProducesProblem(statusCode: StatusCodes.Status429TooManyRequests);
    }
}
