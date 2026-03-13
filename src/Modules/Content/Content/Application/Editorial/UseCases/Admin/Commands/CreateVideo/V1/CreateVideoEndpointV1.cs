using System.Security.Claims;
using _116.BuildingBlocks.Constants.Authorization.Policies;
using _116.BuildingBlocks.Constants.RateLimit;
using _116.BuildingBlocks.Utils;
using _116.Content.Application.Editorial.Constants;
using _116.Content.Application.Shared.DTOs;
using _116.Content.Domain.Constants;
using _116.Shared.Application.Extensions;
using _116.Shared.Contracts.Application.CQRS;
using Carter;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace _116.Content.Application.Editorial.UseCases.Admin.Commands.CreateVideo.V1;

/// <summary>
/// Request model for creating a video draft.
/// </summary>
/// <param name="CategoryId">The category this video belongs to.</param>
/// <param name="Title">The video display title.</param>
/// <param name="Slug">The URL-safe slug for this video.</param>
/// <param name="CustomerId">The B2B customer who commissioned this video. Null for free content.</param>
/// <param name="OrderItemId">The order item this video fulfils. Null for free content.</param>
/// <param name="Description">Optional description shown below the video player.</param>
/// <param name="ShootingScheduledAt">Optional scheduled shooting date for pre-booked productions.</param>
public record CreateVideoRequest(
    Guid CategoryId,
    string Title,
    string Slug,
    Guid? CustomerId,
    Guid? OrderItemId,
    string? Description,
    DateTimeOffset? ShootingScheduledAt
);

/// <summary>
/// Response model for successful video draft creation.
/// </summary>
/// <param name="Video">The created video detail information.</param>
public record CreateVideoResponse(VideoDetailDto Video);

/// <summary>
/// Defines the admin create video endpoint.
/// Handles creation of new video drafts (step 1 of the video creation flow).
/// </summary>
public class CreateVideoEndpointV1 : ICarterModule
{
    /// <summary>
    /// Configures the video creation route within the API pipeline.
    /// Maps the <c>POST /api/v1/admin/videos</c> endpoint to handle video creation requests.
    /// </summary>
    /// <param name="app">The route builder used to register API endpoints.</param>
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        RouteGroupBuilder group = app.MapApiVersionGroup(1)
            .MapGroup($"{ContentConstants.Admin}/{EditorialRouteConstants.Videos}")
            .WithTags($"{ContentConstants.Admin}::{EditorialRouteConstants.Videos}");

        group
            .MapPost(
                "/",
                async (CreateVideoRequest request, IDispatcher dispatcher, HttpContext httpContext) =>
                {
                    string? authorId = httpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);

                    var command = new CreateVideoCommand(
                        CategoryId: request.CategoryId,
                        Title: request.Title,
                        Slug: request.Slug,
                        AuthorId: authorId!,
                        CustomerId: request.CustomerId,
                        OrderItemId: request.OrderItemId,
                        Description: request.Description,
                        ShootingScheduledAt: request.ShootingScheduledAt
                    );

                    CreateVideoResult result = await dispatcher.Send(request: command);

                    var response = new CreateVideoResponse(Video: result.Video);
                    Guid videoId = response.Video.Id;

                    string path = $"{ContentConstants.Admin}/{EditorialRouteConstants.Videos}/{videoId}";
                    string locationUrl = ApiVersionUrl.Build(context: httpContext, path: path);

                    return Results.Created(uri: locationUrl, value: response);
                }
            )
            .WithName(endpointName: CreateVideoMetaField.CreateVideo.Name)
            .WithSummary(summary: CreateVideoMetaField.CreateVideo.Summary)
            .WithDescription(description: CreateVideoMetaField.CreateVideo.Description)
            .WithAuthorization(AccountStatusPolicies.RequireActiveUser)
            .WithAuthorization(UserRolePolicies.RequireSuperAdminOnly)
            .RequireRateLimiting(policyName: RateLimitPolicies.ContentBrowsing)
            .ProducesValidationProblem()
            .Produces<CreateVideoResponse>(statusCode: StatusCodes.Status201Created)
            .ProducesProblem(statusCode: StatusCodes.Status400BadRequest)
            .ProducesProblem(statusCode: StatusCodes.Status401Unauthorized)
            .ProducesProblem(statusCode: StatusCodes.Status403Forbidden)
            .ProducesProblem(statusCode: StatusCodes.Status404NotFound)
            .ProducesProblem(statusCode: StatusCodes.Status409Conflict)
            .ProducesProblem(statusCode: StatusCodes.Status429TooManyRequests);
    }
}
