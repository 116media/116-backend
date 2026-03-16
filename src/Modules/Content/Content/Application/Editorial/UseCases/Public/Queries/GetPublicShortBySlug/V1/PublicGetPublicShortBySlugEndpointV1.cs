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

namespace _116.Content.Application.Editorial.UseCases.Public.Queries.GetPublicShortBySlug.V1;

/// <summary>
/// Response model for retrieving a public short video by its slug.
/// </summary>
/// <param name="ShortVideo">The short video detail information.</param>
public record PublicGetPublicShortBySlugResponse(ShortVideoDto ShortVideo);

/// <summary>
/// Defines the public get short video by slug endpoint.
/// Returns the full details of a single active short video.
/// </summary>
public class PublicGetPublicShortBySlugEndpointV1 : ICarterModule
{
    /// <summary>
    /// Configures the short video detail retrieval route within the API pipeline.
    /// Maps the <c>GET /api/v1/public/shorts/{slug}</c> endpoint.
    /// </summary>
    /// <param name="app">The route builder used to register API endpoints.</param>
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        RouteGroupBuilder group = app.MapApiVersionGroup(1)
            .MapGroup($"{ContentConstants.Public}/{EditorialRouteConstants.Shorts}")
            .WithTags($"{ContentConstants.Public}::{EditorialRouteConstants.Shorts}");

        group
            .MapGet(
                "/{slug}",
                async (string slug, IDispatcher dispatcher) =>
                {
                    var query = new PublicGetPublicShortBySlugQuery(Slug: slug);
                    PublicGetPublicShortBySlugResult result = await dispatcher.Send(request: query);

                    var response = new PublicGetPublicShortBySlugResponse(ShortVideo: result.ShortVideo);
                    return Results.Ok(response);
                }
            )
            .WithName(endpointName: PublicGetPublicShortBySlugMetaField.PublicGetPublicShortBySlug.Name)
            .WithSummary(summary: PublicGetPublicShortBySlugMetaField.PublicGetPublicShortBySlug.Summary)
            .WithDescription(description: PublicGetPublicShortBySlugMetaField.PublicGetPublicShortBySlug.Description)
            .AllowAnonymous()
            .RequireRateLimiting(policyName: RateLimitPolicies.ContentBrowsing)
            .Produces<PublicGetPublicShortBySlugResponse>(statusCode: StatusCodes.Status200OK)
            .ProducesProblem(statusCode: StatusCodes.Status404NotFound)
            .ProducesProblem(statusCode: StatusCodes.Status429TooManyRequests);
    }
}
