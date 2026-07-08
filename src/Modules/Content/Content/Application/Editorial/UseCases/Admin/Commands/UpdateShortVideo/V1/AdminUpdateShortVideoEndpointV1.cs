using _116.BuildingBlocks.Constants.Authorization.Policies;
using _116.BuildingBlocks.Constants.RateLimit;
using _116.Content.Application.Editorial.Constants;
using _116.Content.Application.Shared.DTOs;
using _116.Content.Domain.Constants;
using _116.Shared.Application.Extensions;
using _116.Shared.Contracts.Application.CQRS;
using Carter;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace _116.Content.Application.Editorial.UseCases.Admin.Commands.UpdateShortVideo.V1;

/// <summary>
/// Request model for updating a short video's metadata. The video file is replaced separately via
/// the dedicated <c>POST /api/v1/admin/shorts/{id}/video</c> endpoint.
/// </summary>
/// <param name="Title">The new display title.</param>
/// <param name="VideoId">Optional parent full video identifier. <c>null</c> for standalone.</param>
public record AdminUpdateShortVideoRequest(string Title, Guid? VideoId);

/// <summary>
/// Response model for a successful short video update.
/// </summary>
/// <param name="ShortVideo">The updated short video information.</param>
public record AdminUpdateShortVideoResponse(ShortVideoDto ShortVideo);

/// <summary>
/// Defines the admin update short video endpoint.
/// Handles updating metadata; the video file is replaced separately.
/// </summary>
public class AdminUpdateShortVideoEndpointV1 : ICarterModule
{
    /// <summary>
    /// Configures the short video update route within the API pipeline.
    /// Maps the <c>PUT /api/v1/admin/shorts/{id}</c> endpoint.
    /// </summary>
    /// <param name="app">The route builder used to register API endpoints.</param>
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        RouteGroupBuilder group = app.MapApiVersionGroup(1)
            .MapGroup($"{ContentConstants.Admin}/{EditorialRouteConstants.Shorts}")
            .WithTags($"{ContentConstants.Admin}::{EditorialRouteConstants.Shorts}");

        group
            .MapPut(
                "/{id}",
                async (string id, AdminUpdateShortVideoRequest request, IDispatcher dispatcher) =>
                {
                    var command = new AdminUpdateShortVideoCommand(
                        Id: id,
                        Title: request.Title,
                        VideoId: request.VideoId
                    );

                    AdminUpdateShortVideoResult result = await dispatcher.Send(request: command);
                    var response = new AdminUpdateShortVideoResponse(ShortVideo: result.ShortVideo);
                    return Results.Ok(response);
                }
            )
            .WithName(endpointName: AdminUpdateShortVideoMetaField.UpdateShortVideo.Name)
            .WithSummary(summary: AdminUpdateShortVideoMetaField.UpdateShortVideo.Summary)
            .WithDescription(description: AdminUpdateShortVideoMetaField.UpdateShortVideo.Description)
            .WithAuthorization(AccountStatusPolicies.RequireActiveUser)
            .WithAuthorization(UserRolePolicies.RequireAdminOrSuperAdmin)
            .RequireRateLimiting(policyName: RateLimitPolicies.ContentBrowsing)
            .ProducesValidationProblem()
            .Produces<AdminUpdateShortVideoResponse>(statusCode: StatusCodes.Status200OK)
            .ProducesProblem(statusCode: StatusCodes.Status400BadRequest)
            .ProducesProblem(statusCode: StatusCodes.Status401Unauthorized)
            .ProducesProblem(statusCode: StatusCodes.Status403Forbidden)
            .ProducesProblem(statusCode: StatusCodes.Status404NotFound)
            .ProducesProblem(statusCode: StatusCodes.Status429TooManyRequests);
    }
}
