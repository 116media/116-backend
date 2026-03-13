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

namespace _116.Content.Application.Editorial.UseCases.Admin.Commands.AttachYoutubeId.V1;

/// <summary>
/// Request model for attaching a YouTube video ID.
/// </summary>
/// <param name="YoutubeVideoId">The YouTube video ID (e.g., "dQw4w9WgXcQ").</param>
public record AttachYoutubeIdRequest(string YoutubeVideoId);

/// <summary>
/// Response model for successful YouTube ID attachment.
/// </summary>
/// <param name="Video">The updated video detail information.</param>
public record AttachYoutubeIdResponse(VideoDetailDto Video);

/// <summary>
/// Defines the admin attach YouTube ID endpoint.
/// Handles attaching a YouTube video ID and auto-downloading the thumbnail.
/// </summary>
public class AttachYoutubeIdEndpointV1 : ICarterModule
{
    /// <summary>
    /// Configures the YouTube ID attachment route within the API pipeline.
    /// Maps the <c>PATCH /api/v1/admin/videos/{id}/youtube</c> endpoint.
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
                async (string id, AttachYoutubeIdRequest request, IDispatcher dispatcher) =>
                {
                    var command = new AttachYoutubeIdCommand(VideoId: id, YoutubeVideoId: request.YoutubeVideoId);

                    AttachYoutubeIdResult result = await dispatcher.Send(request: command);
                    return Results.Ok(new AttachYoutubeIdResponse(Video: result.Video));
                }
            )
            .WithName(endpointName: AttachYoutubeIdMetaField.AttachYoutubeId.Name)
            .WithSummary(summary: AttachYoutubeIdMetaField.AttachYoutubeId.Summary)
            .WithDescription(description: AttachYoutubeIdMetaField.AttachYoutubeId.Description)
            .WithAuthorization(AccountStatusPolicies.RequireActiveUser)
            .WithAuthorization(UserRolePolicies.RequireAdminOnly)
            .RequireRateLimiting(policyName: RateLimitPolicies.ContentBrowsing)
            .ProducesValidationProblem()
            .Produces<AttachYoutubeIdResponse>(statusCode: StatusCodes.Status200OK)
            .ProducesProblem(statusCode: StatusCodes.Status400BadRequest)
            .ProducesProblem(statusCode: StatusCodes.Status401Unauthorized)
            .ProducesProblem(statusCode: StatusCodes.Status403Forbidden)
            .ProducesProblem(statusCode: StatusCodes.Status404NotFound)
            .ProducesProblem(statusCode: StatusCodes.Status429TooManyRequests);
    }
}
