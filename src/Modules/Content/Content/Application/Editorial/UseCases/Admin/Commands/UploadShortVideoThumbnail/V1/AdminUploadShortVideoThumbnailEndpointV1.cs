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

namespace _116.Content.Application.Editorial.UseCases.Admin.Commands.UploadShortVideoThumbnail.V1;

/// <summary>
/// Response model for uploading a short video thumbnail.
/// </summary>
/// <param name="ThumbnailUrl">The publicly accessible URL of the uploaded thumbnail.</param>
/// <param name="ThumbnailStorageKey">The provider-agnostic storage key for the uploaded thumbnail.</param>
public record AdminUploadShortVideoThumbnailResponse(string ThumbnailUrl, string ThumbnailStorageKey);

/// <summary>
/// Defines the admin upload short video thumbnail endpoint.
/// Handles thumbnail image upload for an existing short video.
/// </summary>
public class AdminUploadShortVideoThumbnailEndpointV1 : ICarterModule
{
    /// <summary>
    /// Configures the short video thumbnail upload route within the API pipeline.
    /// Maps the <c>POST /api/v1/admin/shorts/{id}/thumbnail</c> endpoint.
    /// </summary>
    /// <param name="app">The route builder used to register API endpoints.</param>
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        RouteGroupBuilder group = app.MapApiVersionGroup(1)
            .MapGroup($"{ContentConstants.Admin}/{EditorialRouteConstants.Shorts}")
            .WithTags($"{ContentConstants.Admin}::{EditorialRouteConstants.Shorts}");

        group
            .MapPost(
                $"/{{id}}/{EditorialRouteConstants.Thumbnail}",
                async (string id, IFormFile file, IDispatcher dispatcher) =>
                {
                    var command = new AdminUploadShortVideoThumbnailCommand(ShortVideoId: id, File: file);
                    AdminUploadShortVideoThumbnailResult result = await dispatcher.Send(request: command);

                    var response = new AdminUploadShortVideoThumbnailResponse(
                        ThumbnailUrl: result.ThumbnailUrl,
                        ThumbnailStorageKey: result.ThumbnailStorageKey
                    );

                    return Results.Ok(response);
                }
            )
            .WithName(endpointName: AdminUploadShortVideoThumbnailMetaField.AdminUploadShortVideoThumbnail.Name)
            .WithSummary(summary: AdminUploadShortVideoThumbnailMetaField.AdminUploadShortVideoThumbnail.Summary)
            .WithDescription(
                description: AdminUploadShortVideoThumbnailMetaField.AdminUploadShortVideoThumbnail.Description
            )
            .WithAuthorization(AccountStatusPolicies.RequireActiveUser)
            .WithAuthorization(UserRolePolicies.RequireAdminOrSuperAdmin)
            .RequireRateLimiting(policyName: RateLimitPolicies.FileUpload)
            .DisableAntiforgery()
            .ProducesValidationProblem()
            .Produces<AdminUploadShortVideoThumbnailResponse>(statusCode: StatusCodes.Status200OK)
            .ProducesProblem(statusCode: StatusCodes.Status400BadRequest)
            .ProducesProblem(statusCode: StatusCodes.Status401Unauthorized)
            .ProducesProblem(statusCode: StatusCodes.Status403Forbidden)
            .ProducesProblem(statusCode: StatusCodes.Status404NotFound)
            .ProducesProblem(statusCode: StatusCodes.Status429TooManyRequests);
    }
}
