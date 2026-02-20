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
public record GetAllTagsResponse(IReadOnlyList<TagDto> Tags);

/// <summary>
/// Defines the public get all tags endpoint.
/// Returns all tags for use in content discovery.
/// </summary>
public class GetAllTagsEndpointV1 : ICarterModule
{
    /// <inheritdoc />
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        RouteGroupBuilder group = app.MapApiVersionGroup(1)
            .MapGroup($"{ContentConstants.Public}/{LookupRouteConstants.Tags}")
            .WithTags($"{ContentConstants.Public}::{LookupRouteConstants.Tags}");

        group
            .MapGet(
                "/",
                async (IDispatcher dispatcher) =>
                {
                    var query = new GetAllTagsQuery();

                    GetAllTagsResult result = await dispatcher.Send(request: query);

                    var response = new GetAllTagsResponse(Tags: result.Tags);

                    return Results.Ok(response);
                }
            )
            .WithName(endpointName: GetAllTagsMetaField.GetAllTags.Name)
            .WithSummary(summary: GetAllTagsMetaField.GetAllTags.Summary)
            .WithDescription(description: GetAllTagsMetaField.GetAllTags.Description)
            .AllowAnonymous()
            .RequireRateLimiting(policyName: RateLimitPolicies.ContentBrowsing)
            .Produces<GetAllTagsResponse>(statusCode: StatusCodes.Status200OK)
            .ProducesProblem(statusCode: StatusCodes.Status429TooManyRequests);
    }
}
