using _116.BuildingBlocks.Constants.RateLimit;
using _116.Content.Application.Lookup.Constants;
using _116.Content.Application.Shared.DTOs;
using _116.Content.Domain.Constants;
using _116.Shared.Application.Extensions;
using _116.Shared.Contracts.Application.CQRS;
using Carter;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace _116.Content.Application.Lookup.UseCases.Public.Queries.GetAllTags.V1;

/// <summary>
/// Response model for listing all public tags.
/// </summary>
/// <param name="Tags">The list of tags.</param>
public record PublicGetAllTagsResponse(IReadOnlyList<TagDto> Tags);

/// <summary>
/// Defines the public get all tags endpoint.
/// Returns all tags for use in content discovery.
/// </summary>
public class PublicGetAllTagsEndpointV1 : ICarterModule
{
    /// <summary>
    /// Configures the tag retrieval route within the API pipeline.
    /// Maps the <c>GET /api/v1/public/tags</c> endpoint to handle tag retrieval requests.
    /// </summary>
    /// <param name="app">The route builder used to register API endpoints.</param>
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        RouteGroupBuilder group = app.MapApiVersionGroup(1)
            .MapGroup($"{ContentConstants.Public}/{LookupRouteConstants.Tags}")
            .WithTags($"{ContentConstants.Public}::{LookupRouteConstants.Tags}");

        group
            .MapGet(
                "/",
                async (IDispatcher dispatcher, string? search = null) =>
                {
                    var query = new PublicGetAllTagsQuery(Search: search);

                    PublicGetAllTagsResult result = await dispatcher.Send(request: query);

                    var response = new PublicGetAllTagsResponse(Tags: result.Tags);
                    return Results.Ok(response);
                }
            )
            .WithName(endpointName: PublicGetAllTagsMetaField.PublicGetAllTags.Name)
            .WithSummary(summary: PublicGetAllTagsMetaField.PublicGetAllTags.Summary)
            .WithDescription(description: PublicGetAllTagsMetaField.PublicGetAllTags.Description)
            .AllowAnonymous()
            .RequireRateLimiting(policyName: RateLimitPolicies.ContentBrowsing)
            .Produces<PublicGetAllTagsResponse>(statusCode: StatusCodes.Status200OK)
            .ProducesProblem(statusCode: StatusCodes.Status429TooManyRequests);
    }
}
