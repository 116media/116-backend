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
/// Response model for successful short video creation.
/// </summary>
/// <param name="ShortVideo">The created short video information.</param>
public record AdminCreateShortVideoResponse(ShortVideoDto ShortVideo);

/// <summary>
/// Defines the admin create short video endpoint.
/// Handles upload and creation of new short video clips via multipart/form-data.
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
                    IFormFile videoFile,
                    ClaimsPrincipal user,
                    IClaimsProvider claimsProvider,
                    IDispatcher dispatcher,
                    HttpContext httpContext,
                    string title,
                    string slug,
                    Guid? videoId = null
                ) =>
                {
                    Guid authorId = claimsProvider.GetUserIdFromClaims(user: user);

                    var command = new AdminCreateShortVideoCommand(
                        Title: title,
                        Slug: slug,
                        VideoFile: videoFile,
                        AuthorId: authorId,
                        VideoId: videoId
                    );

                    AdminCreateShortVideoResult result = await dispatcher.Send(request: command);

                    var response = new AdminCreateShortVideoResponse(ShortVideo: result.ShortVideo);
                    Guid shortVideoId = response.ShortVideo.Id;

                    string path = $"{ContentConstants.Admin}/{EditorialRouteConstants.Shorts}/{shortVideoId}";
                    string locationUrl = ApiVersionUrl.Build(context: httpContext, path: path);

                    return Results.Created(uri: locationUrl, value: response);
                }
            )
            .WithName(endpointName: AdminCreateShortVideoMetaField.AdminCreateShortVideo.Name)
            .WithSummary(summary: AdminCreateShortVideoMetaField.AdminCreateShortVideo.Summary)
            .WithDescription(description: AdminCreateShortVideoMetaField.AdminCreateShortVideo.Description)
            .WithAuthorization(AccountStatusPolicies.RequireActiveUser)
            .WithAuthorization(UserRolePolicies.RequireSuperAdminOnly)
            .RequireRateLimiting(policyName: RateLimitPolicies.FileUpload)
            .DisableAntiforgery()
            .ProducesValidationProblem()
            .Produces<AdminCreateShortVideoResponse>(statusCode: StatusCodes.Status201Created)
            .ProducesProblem(statusCode: StatusCodes.Status400BadRequest)
            .ProducesProblem(statusCode: StatusCodes.Status401Unauthorized)
            .ProducesProblem(statusCode: StatusCodes.Status403Forbidden)
            .ProducesProblem(statusCode: StatusCodes.Status429TooManyRequests);
    }
}
