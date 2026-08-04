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

namespace _116.Content.Application.Editorial.UseCases.Admin.Commands.UploadLyricsCover.V1;

/// <summary>
/// Response model for successful lyrics cover image upload.
/// </summary>
/// <param name="CoverImageUrl">The publicly accessible URL of the uploaded cover image.</param>
/// <param name="CoverImageStorageKey">The provider-agnostic storage key for the cover asset.</param>
public record AdminUploadLyricsCoverResponse(string CoverImageUrl, string CoverImageStorageKey);

/// <summary>
/// Defines the admin upload lyrics cover endpoint.
/// Handles uploading or replacing a lyrics page's cover/album art image.
/// </summary>
public class AdminUploadLyricsCoverEndpointV1 : ICarterModule
{
    /// <summary>
    /// Configures the lyrics cover upload route within the API pipeline.
    /// Maps the <c>POST /api/v1/admin/lyrics/{id}/cover</c> endpoint.
    /// </summary>
    /// <param name="app">The route builder used to register API endpoints.</param>
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        RouteGroupBuilder group = app.MapApiVersionGroup(1)
            .MapGroup($"{ContentConstants.Admin}/{EditorialRouteConstants.Lyrics}")
            .WithTags($"{ContentConstants.Admin}::{EditorialRouteConstants.Lyrics}");

        group
            .MapPost(
                $"/{{id}}/{EditorialRouteConstants.Cover}",
                async (Guid id, IFormFile? file, IDispatcher dispatcher) =>
                {
                    var command = new AdminUploadLyricsCoverCommand(LyricsId: id, File: file);
                    AdminUploadLyricsCoverResult result = await dispatcher.Send(request: command);

                    var response = new AdminUploadLyricsCoverResponse(
                        CoverImageUrl: result.CoverImageUrl,
                        CoverImageStorageKey: result.CoverImageStorageKey
                    );

                    return Results.Ok(response);
                }
            )
            .DisableAntiforgery()
            .WithName(endpointName: AdminUploadLyricsCoverMetaField.UploadLyricsCover.Name)
            .WithSummary(summary: AdminUploadLyricsCoverMetaField.UploadLyricsCover.Summary)
            .WithDescription(description: AdminUploadLyricsCoverMetaField.UploadLyricsCover.Description)
            .WithAuthorization(AccountStatusPolicies.RequireActiveUser)
            .WithAuthorization(UserRolePolicies.RequireAdminOrSuperAdmin)
            .RequireRateLimiting(policyName: RateLimitPolicies.FileUpload)
            .ProducesValidationProblem()
            .Produces<AdminUploadLyricsCoverResponse>(statusCode: StatusCodes.Status200OK)
            .ProducesProblem(statusCode: StatusCodes.Status400BadRequest)
            .ProducesProblem(statusCode: StatusCodes.Status401Unauthorized)
            .ProducesProblem(statusCode: StatusCodes.Status403Forbidden)
            .ProducesProblem(statusCode: StatusCodes.Status404NotFound)
            .ProducesProblem(statusCode: StatusCodes.Status429TooManyRequests);
    }
}
