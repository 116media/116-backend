using _116.BuildingBlocks.Constants.Authorization.Policies;
using _116.BuildingBlocks.Constants.RateLimit;
using _116.BuildingBlocks.Utils;
using _116.Content.Application.Editorial.Constants;
using _116.Content.Application.Shared.DTOs;
using _116.Content.Domain.Constants;
using _116.Content.Domain.Enums;
using _116.Shared.Application.Extensions;
using _116.Shared.Contracts.Application.CQRS;
using Carter;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace _116.Content.Application.Editorial.UseCases.Admin.Commands.UploadArticleImage.V1;

/// <summary>
/// Response model for successful article image upload.
/// </summary>
/// <param name="Image">The uploaded article image information.</param>
public record UploadArticleImageResponse(ArticleImageDto Image);

/// <summary>
/// Defines the admin upload article image endpoint.
/// Handles uploading cover and body images for an article.
/// </summary>
public class UploadArticleImageEndpointV1 : ICarterModule
{
    /// <summary>
    /// Configures the article image upload route within the API pipeline.
    /// Maps the <c>POST /api/v1/admin/articles/{id}/images</c> endpoint.
    /// </summary>
    /// <param name="app">The route builder used to register API endpoints.</param>
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        RouteGroupBuilder group = app.MapApiVersionGroup(1)
            .MapGroup($"{ContentConstants.Admin}/{EditorialRouteConstants.Articles}")
            .WithTags($"{ContentConstants.Admin}::{EditorialRouteConstants.Articles}");

        group
            .MapPost(
                $"/{{id}}/{EditorialRouteConstants.Images}",
                async (
                    string id,
                    IFormFile file,
                    EnumArticleImageType imageType,
                    IDispatcher dispatcher,
                    HttpContext httpContext
                ) =>
                {
                    var command = new UploadArticleImageCommand(ArticleId: id, File: file, ImageType: imageType);

                    UploadArticleImageResult result = await dispatcher.Send(request: command);

                    var response = new UploadArticleImageResponse(Image: result.Image);
                    Guid imageId = response.Image.Id;

                    string path =
                        $"{ContentConstants.Admin}/{EditorialRouteConstants.Articles}/{id}/{EditorialRouteConstants.Images}/{imageId}";
                    string locationUrl = ApiVersionUrl.Build(context: httpContext, path: path);

                    return Results.Created(uri: locationUrl, value: response);
                }
            )
            .DisableAntiforgery()
            .WithName(endpointName: UploadArticleImageMetaField.UploadArticleImage.Name)
            .WithSummary(summary: UploadArticleImageMetaField.UploadArticleImage.Summary)
            .WithDescription(description: UploadArticleImageMetaField.UploadArticleImage.Description)
            .WithAuthorization(AccountStatusPolicies.RequireActiveUser)
            .WithAuthorization(UserRolePolicies.RequireSuperAdminOnly)
            .RequireRateLimiting(policyName: RateLimitPolicies.FileUpload)
            .ProducesValidationProblem()
            .Produces<UploadArticleImageResponse>(statusCode: StatusCodes.Status201Created)
            .ProducesProblem(statusCode: StatusCodes.Status400BadRequest)
            .ProducesProblem(statusCode: StatusCodes.Status401Unauthorized)
            .ProducesProblem(statusCode: StatusCodes.Status403Forbidden)
            .ProducesProblem(statusCode: StatusCodes.Status404NotFound)
            .ProducesProblem(statusCode: StatusCodes.Status429TooManyRequests);
    }
}
