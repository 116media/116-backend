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

namespace _116.Content.Application.Editorial.UseCases.Public.Queries.GetVideoBySlug.V1;

/// <summary>
/// Response model for retrieving a published video by its slug.
/// </summary>
/// <param name="Video">The full video detail information.</param>
public record PublicGetVideoBySlugResponse(VideoDetailDto Video);

/// <summary>
/// Defines the public get video by slug endpoint.
/// Returns the full details of a single published video.
/// </summary>
public class PublicGetVideoBySlugEndpointV1 : ICarterModule
{
    /// <summary>
    /// Configures the video detail retrieval route within the API pipeline.
    /// Maps the <c>GET /api/v1/public/videos/{slug}</c> endpoint.
    /// </summary>
    /// <param name="app">The route builder used to register API endpoints.</param>
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        RouteGroupBuilder group = app.MapApiVersionGroup(1)
            .MapGroup($"{ContentConstants.Public}/{EditorialRouteConstants.Videos}")
            .WithTags($"{ContentConstants.Public}::{EditorialRouteConstants.Videos}");

        group
            .MapGet(
                "/{slug}",
                async (string slug, IDispatcher dispatcher) =>
                {
                    var query = new PublicGetVideoBySlugQuery(Slug: slug);
                    PublicGetVideoBySlugResult result = await dispatcher.Send(request: query);

                    var response = new PublicGetVideoBySlugResponse(Video: result.Video);
                    return Results.Ok(response);
                }
            )
            .WithName(endpointName: PublicGetVideoBySlugMetaField.PublicGetVideoBySlug.Name)
            .WithSummary(summary: PublicGetVideoBySlugMetaField.PublicGetVideoBySlug.Summary)
            .WithDescription(description: PublicGetVideoBySlugMetaField.PublicGetVideoBySlug.Description)
            .AllowAnonymous()
            .RequireRateLimiting(policyName: RateLimitPolicies.ContentBrowsing)
            .Produces<PublicGetVideoBySlugResponse>(statusCode: StatusCodes.Status200OK)
            .ProducesProblem(statusCode: StatusCodes.Status404NotFound)
            .ProducesProblem(statusCode: StatusCodes.Status429TooManyRequests);
    }
}
