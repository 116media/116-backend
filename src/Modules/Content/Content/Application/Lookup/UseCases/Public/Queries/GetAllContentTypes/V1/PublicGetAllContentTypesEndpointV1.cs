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

namespace _116.Content.Application.Lookup.UseCases.Public.Queries.GetAllContentTypes.V1;

/// <summary>
/// Response model for listing all public content types.
/// </summary>
/// <param name="ContentTypes">The list of content types.</param>
public record PublicGetAllContentTypesResponse(IReadOnlyList<ContentTypeDto> ContentTypes);

/// <summary>
/// Defines the public get all content types endpoint.
/// Returns all content types for use in category resolution.
/// </summary>
public class PublicGetAllContentTypesEndpointV1 : ICarterModule
{
    /// <summary>
    /// Configures the content type retrieval route within the API pipeline.
    /// Maps the <c>GET /api/v1/public/content-types</c> endpoint to handle content type retrieval requests.
    /// </summary>
    /// <param name="app">The route builder used to register API endpoints.</param>
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        RouteGroupBuilder group = app.MapApiVersionGroup(1)
            .MapGroup($"{ContentConstants.Public}/{LookupRouteConstants.ContentTypes}")
            .WithTags($"{ContentConstants.Public}::{LookupRouteConstants.ContentTypes}");

        group
            .MapGet(
                "/",
                async (IDispatcher dispatcher) =>
                {
                    var query = new PublicGetAllContentTypesQuery();

                    PublicGetAllContentTypesResult result = await dispatcher.Send(request: query);

                    var response = new PublicGetAllContentTypesResponse(ContentTypes: result.ContentTypes);
                    return Results.Ok(response);
                }
            )
            .WithName(endpointName: PublicGetAllContentTypesMetaField.GetAllContentTypes.Name)
            .WithSummary(summary: PublicGetAllContentTypesMetaField.GetAllContentTypes.Summary)
            .WithDescription(description: PublicGetAllContentTypesMetaField.GetAllContentTypes.Description)
            .AllowAnonymous()
            .RequireRateLimiting(policyName: RateLimitPolicies.ContentBrowsing)
            .Produces<PublicGetAllContentTypesResponse>(statusCode: StatusCodes.Status200OK)
            .ProducesProblem(statusCode: StatusCodes.Status429TooManyRequests);
    }
}
