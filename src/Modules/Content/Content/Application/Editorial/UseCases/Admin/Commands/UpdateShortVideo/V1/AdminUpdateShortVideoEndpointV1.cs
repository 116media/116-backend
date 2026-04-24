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
/// Response model for a successful short video update.
/// </summary>
/// <param name="ShortVideo">The updated short video information.</param>
public record AdminUpdateShortVideoResponse(ShortVideoDto ShortVideo);

/// <summary>
/// Defines the admin update short video endpoint.
/// Handles updating metadata and optionally replacing the video file.
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
                async (
                    string id,
                    string title,
                    IDispatcher dispatcher,
                    Guid? videoId = null,
                    IFormFile? videoFile = null
                ) =>
                {
                    var command = new AdminUpdateShortVideoCommand(
                        Id: id,
                        Title: title,
                        VideoId: videoId,
                        VideoFile: videoFile
                    );

                    AdminUpdateShortVideoResult result = await dispatcher.Send(request: command);
                    var response = new AdminUpdateShortVideoResponse(ShortVideo: result.ShortVideo);
                    return Results.Ok(response);
                }
            )
            .WithName(endpointName: AdminUpdateShortVideoMetaField.AdminUpdateShortVideo.Name)
            .WithSummary(summary: AdminUpdateShortVideoMetaField.AdminUpdateShortVideo.Summary)
            .WithDescription(description: AdminUpdateShortVideoMetaField.AdminUpdateShortVideo.Description)
            .WithAuthorization(AccountStatusPolicies.RequireActiveUser)
            .WithAuthorization(UserRolePolicies.RequireAdminOrSuperAdmin)
            .RequireRateLimiting(policyName: RateLimitPolicies.FileUpload)
            .DisableAntiforgery()
            .ProducesValidationProblem()
            .Produces<AdminUpdateShortVideoResponse>(statusCode: StatusCodes.Status200OK)
            .ProducesProblem(statusCode: StatusCodes.Status400BadRequest)
            .ProducesProblem(statusCode: StatusCodes.Status401Unauthorized)
            .ProducesProblem(statusCode: StatusCodes.Status403Forbidden)
            .ProducesProblem(statusCode: StatusCodes.Status404NotFound)
            .ProducesProblem(statusCode: StatusCodes.Status429TooManyRequests);
    }
}
