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

namespace _116.Content.Application.Editorial.UseCases.Admin.Commands.UpdateVideo.V1;

/// <summary>
/// Request model for updating all editable video fields.
/// </summary>
/// <param name="CategoryId">The category this video belongs to.</param>
/// <param name="Title">The video display title.</param>
/// <param name="Slug">The URL-safe slug for this video.</param>
/// <param name="Description">Optional description shown below the video player.</param>
/// <param name="CustomerId">The B2B customer who commissioned this video. <c>null</c> for free content.</param>
/// <param name="OrderItemId">The order item this video fulfils. Required when <c>CustomerId</c> is set.</param>
/// <param name="SocialBoost">Whether to flag this video for manual social media promotion.</param>
/// <param name="IsFeatured">Whether to activate a featured homepage placement.</param>
/// <param name="FeaturedUntil">When the featured placement expires.</param>
/// <param name="MetaTitle">Custom SEO meta title.</param>
/// <param name="MetaDescription">Custom SEO meta description.</param>
public record AdminUpdateVideoRequest(
    Guid CategoryId,
    string Title,
    string Slug,
    string? Description,
    Guid? CustomerId,
    Guid? OrderItemId,
    bool SocialBoost,
    bool IsFeatured,
    DateTimeOffset? FeaturedUntil,
    string? MetaTitle,
    string? MetaDescription
);

/// <summary>
/// Response model for a successful video update.
/// </summary>
/// <param name="Video">The updated video detail information.</param>
public record AdminUpdateVideoResponse(VideoDetailDto Video);

/// <summary>
/// Defines the admin update video endpoint.
/// Handles updating all editable fields of a video.
/// </summary>
public class AdminUpdateVideoEndpointV1 : ICarterModule
{
    /// <summary>
    /// Configures the video update route within the API pipeline.
    /// Maps the <c>PUT /api/v1/admin/videos/{id}</c> endpoint.
    /// </summary>
    /// <param name="app">The route builder used to register API endpoints.</param>
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        RouteGroupBuilder group = app.MapApiVersionGroup(1)
            .MapGroup($"{ContentConstants.Admin}/{EditorialRouteConstants.Videos}")
            .WithTags($"{ContentConstants.Admin}::{EditorialRouteConstants.Videos}");

        group
            .MapPut(
                "/{id}",
                async (string id, AdminUpdateVideoRequest request, IDispatcher dispatcher) =>
                {
                    var command = new AdminUpdateVideoCommand(
                        Id: id,
                        CategoryId: request.CategoryId,
                        Title: request.Title,
                        Slug: request.Slug,
                        Description: request.Description,
                        CustomerId: request.CustomerId,
                        OrderItemId: request.OrderItemId,
                        SocialBoost: request.SocialBoost,
                        IsFeatured: request.IsFeatured,
                        FeaturedUntil: request.FeaturedUntil,
                        MetaTitle: request.MetaTitle,
                        MetaDescription: request.MetaDescription
                    );

                    AdminUpdateVideoResult result = await dispatcher.Send(request: command);
                    var response = new AdminUpdateVideoResponse(Video: result.Video);
                    return Results.Ok(response);
                }
            )
            .WithName(endpointName: AdminUpdateVideoMetaField.AdminUpdateVideo.Name)
            .WithSummary(summary: AdminUpdateVideoMetaField.AdminUpdateVideo.Summary)
            .WithDescription(description: AdminUpdateVideoMetaField.AdminUpdateVideo.Description)
            .WithAuthorization(AccountStatusPolicies.RequireActiveUser)
            .WithAuthorization(UserRolePolicies.RequireAdminOrSuperAdmin)
            .RequireRateLimiting(policyName: RateLimitPolicies.ContentBrowsing)
            .ProducesValidationProblem()
            .Produces<AdminUpdateVideoResponse>(statusCode: StatusCodes.Status200OK)
            .ProducesProblem(statusCode: StatusCodes.Status400BadRequest)
            .ProducesProblem(statusCode: StatusCodes.Status401Unauthorized)
            .ProducesProblem(statusCode: StatusCodes.Status403Forbidden)
            .ProducesProblem(statusCode: StatusCodes.Status404NotFound)
            .ProducesProblem(statusCode: StatusCodes.Status409Conflict)
            .ProducesProblem(statusCode: StatusCodes.Status429TooManyRequests);
    }
}
