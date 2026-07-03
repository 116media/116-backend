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

namespace _116.Content.Application.Editorial.UseCases.Admin.Commands.AttachYoutubeVideoUrl.V1;

/// <summary>
/// Request model for attaching a YouTube video URL.
/// </summary>
/// <param name="YoutubeVideoUrl">
/// The full YouTube video URL (e.g., "https://www.youtube.com/watch?v=dQw4w9WgXcQ").
/// </param>
public record AdminAttachYoutubeVideoUrlRequest(string YoutubeVideoUrl);

/// <summary>
/// Response model for successful YouTube ID attachment.
/// </summary>
/// <param name="Video">The updated video detail information.</param>
public record AdminAttachYoutubeVideoUrlResponse(VideoDetailDto Video);

/// <summary>
/// Defines the "admin attach" YouTube ID endpoint.
/// Handles attaching a YouTube video ID and auto-downloading the thumbnail.
/// </summary>
public class AdminAttachYoutubeVideoUrlEndpointV1 : ICarterModule
{
    /// <summary>
    /// Configures the YouTube ID attachment route within the API pipeline.
    /// Maps the <c>PATCH /api/v1/admin/videos/{id}/YouTube</c> endpoint.
    /// </summary>
    /// <param name="app">The route builder used to register API endpoints.</param>
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        RouteGroupBuilder group = app.MapApiVersionGroup(1)
            .MapGroup($"{ContentConstants.Admin}/{EditorialRouteConstants.Videos}")
            .WithTags($"{ContentConstants.Admin}::{EditorialRouteConstants.Videos}");

        group
            .MapPatch(
                $"/{{id}}/{EditorialRouteConstants.Youtube}",
                async (string id, AdminAttachYoutubeVideoUrlRequest request, IDispatcher dispatcher) =>
                {
                    var command = new AdminAttachYoutubeVideoUrlCommand(
                        VideoId: id,
                        YoutubeVideoUrl: request.YoutubeVideoUrl
                    );

                    AdminAttachYoutubeVideoUrlResult result = await dispatcher.Send(request: command);

                    var response = new AdminAttachYoutubeVideoUrlResponse(Video: result.Video);
                    return Results.Ok(response);
                }
            )
            .WithName(endpointName: AdminAttachYoutubeVideoUrlMetaField.AttachYoutubeVideoUrl.Name)
            .WithSummary(summary: AdminAttachYoutubeVideoUrlMetaField.AttachYoutubeVideoUrl.Summary)
            .WithDescription(description: AdminAttachYoutubeVideoUrlMetaField.AttachYoutubeVideoUrl.Description)
            .WithAuthorization(AccountStatusPolicies.RequireActiveUser)
            .WithAuthorization(UserRolePolicies.RequireAdminOrSuperAdmin)
            .RequireRateLimiting(policyName: RateLimitPolicies.ContentBrowsing)
            .ProducesValidationProblem()
            .Produces<AdminAttachYoutubeVideoUrlResponse>(statusCode: StatusCodes.Status200OK)
            .ProducesProblem(statusCode: StatusCodes.Status400BadRequest)
            .ProducesProblem(statusCode: StatusCodes.Status401Unauthorized)
            .ProducesProblem(statusCode: StatusCodes.Status403Forbidden)
            .ProducesProblem(statusCode: StatusCodes.Status404NotFound)
            .ProducesProblem(statusCode: StatusCodes.Status429TooManyRequests);
    }
}
