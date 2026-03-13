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

namespace _116.Content.Application.Editorial.UseCases.Admin.Commands.CreateShortVideo.V1;

/// <summary>
/// Response model for successful short video creation.
/// </summary>
/// <param name="ShortVideo">The created short video information.</param>
public record CreateShortVideoResponse(ShortVideoDto ShortVideo);

/// <summary>
/// Defines the admin create short video endpoint.
/// Handles upload and creation of new short video clips via multipart/form-data.
/// </summary>
public class CreateShortVideoEndpointV1 : ICarterModule
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
                    IFormFile videoFile,
                    IDispatcher dispatcher,
                    HttpContext httpContext,
                    string title,
                    Guid? videoId = null
                ) =>
                {
                    var command = new CreateShortVideoCommand(Title: title, VideoFile: videoFile, VideoId: videoId);

                    CreateShortVideoResult result = await dispatcher.Send(request: command);

                    var response = new CreateShortVideoResponse(ShortVideo: result.ShortVideo);
                    Guid shortVideoId = response.ShortVideo.Id;

                    string path = $"{ContentConstants.Admin}/{EditorialRouteConstants.Shorts}/{shortVideoId}";
                    string locationUrl = ApiVersionUrl.Build(context: httpContext, path: path);

                    return Results.Created(uri: locationUrl, value: response);
                }
            )
            .WithName(endpointName: CreateShortVideoMetaField.CreateShortVideo.Name)
            .WithSummary(summary: CreateShortVideoMetaField.CreateShortVideo.Summary)
            .WithDescription(description: CreateShortVideoMetaField.CreateShortVideo.Description)
            .WithAuthorization(AccountStatusPolicies.RequireActiveUser)
            .WithAuthorization(UserRolePolicies.RequireSuperAdminOnly)
            .RequireRateLimiting(policyName: RateLimitPolicies.FileUpload)
            .DisableAntiforgery()
            .ProducesValidationProblem()
            .Produces<CreateShortVideoResponse>(statusCode: StatusCodes.Status201Created)
            .ProducesProblem(statusCode: StatusCodes.Status400BadRequest)
            .ProducesProblem(statusCode: StatusCodes.Status401Unauthorized)
            .ProducesProblem(statusCode: StatusCodes.Status403Forbidden)
            .ProducesProblem(statusCode: StatusCodes.Status429TooManyRequests);
    }
}
