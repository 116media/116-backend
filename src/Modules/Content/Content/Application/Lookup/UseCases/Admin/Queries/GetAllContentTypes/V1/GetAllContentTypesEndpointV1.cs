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

namespace _116.Content.Application.Lookup.UseCases.Admin.Queries.GetAllContentTypes.V1;

/// <summary>
/// Response model for listing all content types.
/// </summary>
/// <param name="ContentTypes">The list of content types.</param>
public record GetAllContentTypesResponse(IReadOnlyList<ContentTypeDto> ContentTypes);

/// <summary>
/// Defines the admin get all content types endpoint.
/// Returns all content types available for category assignment.
/// </summary>
public class GetAllContentTypesEndpointV1 : ICarterModule
{
    /// <inheritdoc />
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        RouteGroupBuilder group = app.MapApiVersionGroup(1)
            .MapGroup($"{ContentConstants.Admin}/{LookupRouteConstants.ContentTypes}")
            .WithTags($"{ContentConstants.Admin}::{LookupRouteConstants.ContentTypes}");

        group
            .MapGet(
                "/",
                async (IDispatcher dispatcher) =>
                {
                    var query = new GetAllContentTypesQuery();

                    GetAllContentTypesResult result = await dispatcher.Send(request: query);

                    var response = new GetAllContentTypesResponse(ContentTypes: result.ContentTypes);

                    return Results.Ok(response);
                }
            )
            .WithName(endpointName: GetAllContentTypesMetaField.GetAllContentTypes.Name)
            .WithSummary(summary: GetAllContentTypesMetaField.GetAllContentTypes.Summary)
            .WithDescription(description: GetAllContentTypesMetaField.GetAllContentTypes.Description)
            .RequireAuthorization()
            .RequireRateLimiting(policyName: RateLimitPolicies.ContentBrowsing)
            .Produces<GetAllContentTypesResponse>(statusCode: StatusCodes.Status200OK)
            .ProducesProblem(statusCode: StatusCodes.Status401Unauthorized)
            .ProducesProblem(statusCode: StatusCodes.Status403Forbidden)
            .ProducesProblem(statusCode: StatusCodes.Status429TooManyRequests);
    }
}
