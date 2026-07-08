using System.Security.Claims;
using _116.BuildingBlocks.Constants.Authorization.Policies;
using _116.BuildingBlocks.Constants.RateLimit;
using _116.BuildingBlocks.Utils;
using _116.Content.Application.Editorial.Constants;
using _116.Content.Application.Shared.DTOs;
using _116.Content.Domain.Constants;
using _116.Identity.Contracts.Application;
using _116.Shared.Application.Extensions;
using _116.Shared.Contracts.Application.CQRS;
using Carter;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace _116.Content.Application.Editorial.UseCases.Admin.Commands.CreateShortVideo.V1;

/// <summary>
/// Request model for creating a short video draft. The video file is uploaded separately via the
/// dedicated <c>POST /api/v1/admin/shorts/{id}/video</c> endpoint.
/// </summary>
/// <param name="Title">The display title of the short video.</param>
/// <param name="Slug">The URL-safe slug for the short video permalink.</param>
/// <param name="VideoId">Optional parent full video identifier. When provided, creates a teaser.</param>
public record AdminCreateShortVideoRequest(string Title, string Slug, Guid? VideoId);

/// <summary>
/// Response model for successful short video creation.
/// </summary>
/// <param name="ShortVideo">The created short video information.</param>
public record AdminCreateShortVideoResponse(ShortVideoDto ShortVideo);

/// <summary>
/// Defines the admin create short video endpoint.
/// Creates a short video draft; the video file is uploaded separately.
/// </summary>
public class AdminCreateShortVideoEndpointV1 : ICarterModule
{
    /// <summary>
    /// Configures the short video creation route within the API pipeline.
    /// Maps the <c>POST /api/v1/admin/shorts</c> endpoint to handle short video creation requests.
    /// </summary>
    /// <param name="app">The route builder used to register API endpoints.</param>
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        RouteGroupBuilder group = app.MapApiVersionGroup(1)
            .MapGroup($"{ContentConstants.Admin}/{EditorialRouteConstants.Shorts}")
            .WithTags($"{ContentConstants.Admin}::{EditorialRouteConstants.Shorts}");

        group
            .MapPost(
                "/",
                async (
                    AdminCreateShortVideoRequest request,
                    ClaimsPrincipal user,
                    IClaimsProvider claimsProvider,
                    IDispatcher dispatcher,
                    HttpContext httpContext
                ) =>
                {
                    Guid authorId = claimsProvider.GetUserIdFromClaims(user: user);

                    var command = new AdminCreateShortVideoCommand(
                        Title: request.Title,
                        Slug: request.Slug,
                        AuthorId: authorId,
                        VideoId: request.VideoId
                    );

                    AdminCreateShortVideoResult result = await dispatcher.Send(request: command);

                    var response = new AdminCreateShortVideoResponse(ShortVideo: result.ShortVideo);
                    Guid shortVideoId = response.ShortVideo.Id;

                    string path = $"{ContentConstants.Admin}/{EditorialRouteConstants.Shorts}/{shortVideoId}";
                    string locationUrl = ApiVersionUrl.Build(context: httpContext, path: path);

                    return Results.Created(uri: locationUrl, value: response);
                }
            )
            .WithName(endpointName: AdminCreateShortVideoMetaField.CreateShortVideo.Name)
            .WithSummary(summary: AdminCreateShortVideoMetaField.CreateShortVideo.Summary)
            .WithDescription(description: AdminCreateShortVideoMetaField.CreateShortVideo.Description)
            .WithAuthorization(AccountStatusPolicies.RequireActiveUser)
            .WithAuthorization(UserRolePolicies.RequireAdminOrSuperAdmin)
            .RequireRateLimiting(policyName: RateLimitPolicies.ContentBrowsing)
            .ProducesValidationProblem()
            .Produces<AdminCreateShortVideoResponse>(statusCode: StatusCodes.Status201Created)
            .ProducesProblem(statusCode: StatusCodes.Status400BadRequest)
            .ProducesProblem(statusCode: StatusCodes.Status401Unauthorized)
            .ProducesProblem(statusCode: StatusCodes.Status403Forbidden)
            .ProducesProblem(statusCode: StatusCodes.Status429TooManyRequests);
    }
}
