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

namespace _116.Content.Application.Editorial.UseCases.Admin.Commands.UpdateVideo.V1;

/// <summary>
/// Request model for updating video metadata.
/// </summary>
/// <param name="Title">The video display title.</param>
/// <param name="Slug">The URL-safe slug for this video.</param>
/// <param name="Description">Optional description shown below the video player.</param>
public record UpdateVideoRequest(string Title, string Slug, string? Description);

/// <summary>
/// Response model for successful video metadata update.
/// </summary>
/// <param name="Video">The updated video detail information.</param>
public record UpdateVideoResponse(VideoDetailDto Video);

/// <summary>
/// Defines the admin update video endpoint.
/// Handles updating a video's editable metadata fields.
/// </summary>
public class UpdateVideoEndpointV1 : ICarterModule
{
    /// <summary>
    /// Configures the video update route within the API pipeline.
    /// Maps the <c>PUT /api/v1/admin/videos/{id}</c> endpoint.
    /// </summary>
    /// <param name="app">The route builder used to register API endpoints.</param>
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        RouteGroupBuilder group = app.MapApiVersionGroup(1)
            .MapGroup($"{ContentConstants.Admin}/{EditorialRouteConstants.Videos}")
            .WithTags($"{ContentConstants.Admin}::{EditorialRouteConstants.Videos}");

        group
            .MapPut(
                "/{id}",
                async (string id, UpdateVideoRequest request, IDispatcher dispatcher) =>
                {
                    var command = new UpdateVideoCommand(
                        Id: id,
                        Title: request.Title,
                        Slug: request.Slug,
                        Description: request.Description
                    );

                    UpdateVideoResult result = await dispatcher.Send(request: command);
                    return Results.Ok(new UpdateVideoResponse(Video: result.Video));
                }
            )
            .WithName(endpointName: UpdateVideoMetaField.UpdateVideo.Name)
            .WithSummary(summary: UpdateVideoMetaField.UpdateVideo.Summary)
            .WithDescription(description: UpdateVideoMetaField.UpdateVideo.Description)
            .WithAuthorization(AccountStatusPolicies.RequireActiveUser)
            .WithAuthorization(UserRolePolicies.RequireAdminOrSuperAdmin)
            .RequireRateLimiting(policyName: RateLimitPolicies.ContentBrowsing)
            .ProducesValidationProblem()
            .Produces<UpdateVideoResponse>(statusCode: StatusCodes.Status200OK)
            .ProducesProblem(statusCode: StatusCodes.Status400BadRequest)
            .ProducesProblem(statusCode: StatusCodes.Status401Unauthorized)
            .ProducesProblem(statusCode: StatusCodes.Status403Forbidden)
            .ProducesProblem(statusCode: StatusCodes.Status404NotFound)
            .ProducesProblem(statusCode: StatusCodes.Status409Conflict)
            .ProducesProblem(statusCode: StatusCodes.Status429TooManyRequests);
    }
}
