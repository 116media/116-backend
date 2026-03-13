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

namespace _116.Content.Application.Editorial.UseCases.Admin.Commands.UpdateVideoSeo.V1;

/// <summary>
/// Request model for updating video SEO metadata.
/// </summary>
/// <param name="MetaTitle">Custom SEO meta title (max 70 chars). Falls back to video title if null.</param>
/// <param name="MetaDescription">Custom SEO meta description (max 160 chars).</param>
public record UpdateVideoSeoRequest(string? MetaTitle, string? MetaDescription);

/// <summary>
/// Response model for successful video SEO update.
/// </summary>
/// <param name="Video">The updated video detail information.</param>
public record UpdateVideoSeoResponse(VideoDetailDto Video);

/// <summary>
/// Defines the admin update video SEO endpoint.
/// Handles updating a video's SEO metadata fields.
/// </summary>
public class UpdateVideoSeoEndpointV1 : ICarterModule
{
    /// <summary>
    /// Configures the video SEO update route within the API pipeline.
    /// Maps the <c>PATCH /api/v1/admin/videos/{id}/seo</c> endpoint.
    /// </summary>
    /// <param name="app">The route builder used to register API endpoints.</param>
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        RouteGroupBuilder group = app.MapApiVersionGroup(1)
            .MapGroup($"{ContentConstants.Admin}/{EditorialRouteConstants.Videos}")
            .WithTags($"{ContentConstants.Admin}::{EditorialRouteConstants.Videos}");

        group
            .MapPatch(
                $"/{{id}}/{EditorialRouteConstants.Seo}",
                async (string id, UpdateVideoSeoRequest request, IDispatcher dispatcher) =>
                {
                    var command = new UpdateVideoSeoCommand(
                        Id: id,
                        MetaTitle: request.MetaTitle,
                        MetaDescription: request.MetaDescription
                    );

                    UpdateVideoSeoResult result = await dispatcher.Send(request: command);
                    return Results.Ok(new UpdateVideoSeoResponse(Video: result.Video));
                }
            )
            .WithName(endpointName: UpdateVideoSeoMetaField.UpdateVideoSeo.Name)
            .WithSummary(summary: UpdateVideoSeoMetaField.UpdateVideoSeo.Summary)
            .WithDescription(description: UpdateVideoSeoMetaField.UpdateVideoSeo.Description)
            .WithAuthorization(AccountStatusPolicies.RequireActiveUser)
            .WithAuthorization(UserRolePolicies.RequireAdminOrSuperAdmin)
            .RequireRateLimiting(policyName: RateLimitPolicies.ContentBrowsing)
            .Produces<UpdateVideoSeoResponse>(statusCode: StatusCodes.Status200OK)
            .ProducesProblem(statusCode: StatusCodes.Status401Unauthorized)
            .ProducesProblem(statusCode: StatusCodes.Status403Forbidden)
            .ProducesProblem(statusCode: StatusCodes.Status404NotFound)
            .ProducesProblem(statusCode: StatusCodes.Status429TooManyRequests);
    }
}
