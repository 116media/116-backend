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

namespace _116.Content.Application.Editorial.UseCases.Admin.Commands.UploadShortVideoFile.V1;

/// <summary>
/// Response model for uploading a short video file.
/// </summary>
/// <param name="VideoUrl">The publicly accessible URL of the uploaded video file.</param>
/// <param name="VideoStorageKey">The provider-agnostic storage key for the uploaded video file.</param>
public record AdminUploadShortVideoFileResponse(string VideoUrl, string VideoStorageKey);

/// <summary>
/// Defines the admin upload short video file endpoint.
/// Handles video file upload for an existing short video draft.
/// </summary>
public class AdminUploadShortVideoFileEndpointV1 : ICarterModule
{
    /// <summary>
    /// Configures the short video file upload route within the API pipeline.
    /// Maps the <c>POST /api/v1/admin/shorts/{id}/video</c> endpoint.
    /// </summary>
    /// <param name="app">The route builder used to register API endpoints.</param>
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        RouteGroupBuilder group = app.MapApiVersionGroup(1)
            .MapGroup($"{ContentConstants.Admin}/{EditorialRouteConstants.Shorts}")
            .WithTags($"{ContentConstants.Admin}::{EditorialRouteConstants.Shorts}");

        group
            .MapPost(
                $"/{{id}}/{EditorialRouteConstants.Video}",
                async (string id, IFormFile? file, IDispatcher dispatcher) =>
                {
                    var command = new AdminUploadShortVideoFileCommand(ShortVideoId: id, File: file);
                    AdminUploadShortVideoFileResult result = await dispatcher.Send(request: command);

                    var response = new AdminUploadShortVideoFileResponse(
                        VideoUrl: result.VideoUrl,
                        VideoStorageKey: result.VideoStorageKey
                    );

                    return Results.Ok(response);
                }
            )
            .WithName(endpointName: AdminUploadShortVideoFileMetaField.UploadShortVideoFile.Name)
            .WithSummary(summary: AdminUploadShortVideoFileMetaField.UploadShortVideoFile.Summary)
            .WithDescription(description: AdminUploadShortVideoFileMetaField.UploadShortVideoFile.Description)
            .WithAuthorization(AccountStatusPolicies.RequireActiveUser)
            .WithAuthorization(UserRolePolicies.RequireAdminOrSuperAdmin)
            .RequireRateLimiting(policyName: RateLimitPolicies.FileUpload)
            .DisableAntiforgery()
            .ProducesValidationProblem()
            .Produces<AdminUploadShortVideoFileResponse>(statusCode: StatusCodes.Status200OK)
            .ProducesProblem(statusCode: StatusCodes.Status400BadRequest)
            .ProducesProblem(statusCode: StatusCodes.Status401Unauthorized)
            .ProducesProblem(statusCode: StatusCodes.Status403Forbidden)
            .ProducesProblem(statusCode: StatusCodes.Status404NotFound)
            .ProducesProblem(statusCode: StatusCodes.Status429TooManyRequests);
    }
}
