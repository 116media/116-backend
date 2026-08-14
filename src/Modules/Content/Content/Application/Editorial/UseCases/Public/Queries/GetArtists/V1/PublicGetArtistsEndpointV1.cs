using _116.BuildingBlocks.Constants.RateLimit;
using _116.Content.Application.Editorial.Constants;
using _116.Content.Application.Shared.DTOs;
using _116.Content.Domain.Constants;
using _116.Shared.Application.Extensions;
using _116.Shared.Application.Pagination;
using _116.Shared.Contracts.Application.CQRS;
using Carter;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace _116.Content.Application.Editorial.UseCases.Public.Queries.GetArtists.V1;

/// <summary>
/// Response model for the public artist directory.
/// </summary>
/// <param name="Artists">The paginated directory cards.</param>
/// <param name="AvailableLetters">The distinct initial letters over the same filtered set.</param>
public record PublicGetArtistsResponse(
    PaginatedResult<ArtistSummaryDto> Artists,
    IReadOnlyList<string> AvailableLetters
);

/// <summary>
/// Defines the public artist directory endpoint.
/// Returns artists with surfaceable content, ordered by accent-folded name.
/// </summary>
public class PublicGetArtistsEndpointV1 : ICarterModule
{
    /// <summary>
    /// Configures the artist directory route within the API pipeline.
    /// Maps the <c>GET /api/v1/public/artists</c> endpoint.
    /// </summary>
    /// <param name="app">The route builder used to register API endpoints.</param>
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        RouteGroupBuilder group = app.MapApiVersionGroup(1)
            .MapGroup($"{ContentConstants.Public}/{EditorialRouteConstants.Artists}")
            .WithTags($"{ContentConstants.Public}::{EditorialRouteConstants.Artists}");

        group
            .MapGet(
                "/",
                async (
                    IDispatcher dispatcher,
                    int pageIndex = 0,
                    int pageSize = 30,
                    string? letter = null,
                    string? search = null
                ) =>
                {
                    var query = new PublicGetArtistsQuery(
                        Page: new PaginatedRequest(pageIndex, pageSize),
                        Letter: letter,
                        Search: search
                    );

                    PublicGetArtistsResult result = await dispatcher.Send(request: query);

                    var response = new PublicGetArtistsResponse(
                        Artists: result.Artists,
                        AvailableLetters: result.AvailableLetters
                    );
                    return Results.Ok(response);
                }
            )
            .WithName(endpointName: PublicGetArtistsMetaField.GetArtists.Name)
            .WithSummary(summary: PublicGetArtistsMetaField.GetArtists.Summary)
            .WithDescription(description: PublicGetArtistsMetaField.GetArtists.Description)
            .AllowAnonymous()
            .RequireRateLimiting(policyName: RateLimitPolicies.ContentBrowsing)
            .ProducesValidationProblem()
            .Produces<PublicGetArtistsResponse>(statusCode: StatusCodes.Status200OK)
            .ProducesProblem(statusCode: StatusCodes.Status400BadRequest)
            .ProducesProblem(statusCode: StatusCodes.Status429TooManyRequests);
    }
}
