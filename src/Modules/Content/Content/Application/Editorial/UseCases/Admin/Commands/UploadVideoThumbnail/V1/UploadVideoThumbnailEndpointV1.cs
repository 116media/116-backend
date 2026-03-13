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

namespace _116.Content.Application.Editorial.UseCases.Admin.Commands.UploadVideoThumbnail.V1;

/// <summary>
/// Response model for successful video thumbnail upload.
/// </summary>
/// <param name="ThumbnailUrl">The publicly accessible URL of the uploaded thumbnail.</param>
/// <param name="ThumbnailStorageKey">The provider-agnostic storage key for the thumbnail asset.</param>
public record UploadVideoThumbnailResponse(string ThumbnailUrl, string ThumbnailStorageKey);

/// <summary>
/// Defines the admin upload video thumbnail endpoint.
/// Handles uploading or replacing a video's thumbnail image.
/// </summary>
public class UploadVideoThumbnailEndpointV1 : ICarterModule
{
    /// <summary>
    /// Configures the video thumbnail upload route within the API pipeline.
    /// Maps the <c>POST /api/v1/admin/videos/{id}/thumbnail</c> endpoint.
    /// </summary>
    /// <param name="app">The route builder used to register API endpoints.</param>
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        RouteGroupBuilder group = app.MapApiVersionGroup(1)
            .MapGroup($"{ContentConstants.Admin}/{EditorialRouteConstants.Videos}")
            .WithTags($"{ContentConstants.Admin}::{EditorialRouteConstants.Videos}");

        group
            .MapPost(
                $"/{{id}}/{EditorialRouteConstants.Thumbnail}",
                async (string id, IFormFile file, IDispatcher dispatcher) =>
                {
                    var command = new UploadVideoThumbnailCommand(VideoId: id, File: file);
                    UploadVideoThumbnailResult result = await dispatcher.Send(request: command);

                    return Results.Ok(
                        new UploadVideoThumbnailResponse(
                            ThumbnailUrl: result.ThumbnailUrl,
                            ThumbnailStorageKey: result.ThumbnailStorageKey
                        )
                    );
                }
            )
            .DisableAntiforgery()
            .WithName(endpointName: UploadVideoThumbnailMetaField.UploadVideoThumbnail.Name)
            .WithSummary(summary: UploadVideoThumbnailMetaField.UploadVideoThumbnail.Summary)
            .WithDescription(description: UploadVideoThumbnailMetaField.UploadVideoThumbnail.Description)
            .WithAuthorization(AccountStatusPolicies.RequireActiveUser)
            .WithAuthorization(UserRolePolicies.RequireAdminOrSuperAdmin)
            .RequireRateLimiting(policyName: RateLimitPolicies.FileUpload)
            .ProducesValidationProblem()
            .Produces<UploadVideoThumbnailResponse>(statusCode: StatusCodes.Status200OK)
            .ProducesProblem(statusCode: StatusCodes.Status400BadRequest)
            .ProducesProblem(statusCode: StatusCodes.Status401Unauthorized)
            .ProducesProblem(statusCode: StatusCodes.Status403Forbidden)
            .ProducesProblem(statusCode: StatusCodes.Status404NotFound)
            .ProducesProblem(statusCode: StatusCodes.Status429TooManyRequests);
    }
}
