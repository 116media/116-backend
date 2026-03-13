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

namespace _116.Content.Application.Editorial.UseCases.Public.Queries.GetLyricsBySlug.V1;

/// <summary>
/// Response model for retrieving lyrics by slug.
/// </summary>
/// <param name="Lyrics">The matched lyrics information.</param>
public record GetLyricsBySlugResponse(LyricsDto Lyrics);

/// <summary>
/// Defines the public get lyrics by slug endpoint.
/// Returns a lyrics page matching the given song title and artist name path parameters.
/// </summary>
public class GetLyricsBySlugEndpointV1 : ICarterModule
{
    /// <summary>
    /// Configures the lyrics by slug retrieval route within the API pipeline.
    /// Maps the <c>GET /api/v1/public/lyrics/{songTitle}/{artistName}</c> endpoint.
    /// </summary>
    /// <param name="app">The route builder used to register API endpoints.</param>
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        RouteGroupBuilder group = app.MapApiVersionGroup(1)
            .MapGroup($"{ContentConstants.Public}/{EditorialRouteConstants.Lyrics}")
            .WithTags($"{ContentConstants.Public}::{EditorialRouteConstants.Lyrics}");

        group
            .MapGet(
                "/{songTitle}/{artistName}",
                async (string songTitle, string artistName, IDispatcher dispatcher) =>
                {
                    var query = new GetLyricsBySlugQuery(SongTitle: songTitle, ArtistName: artistName);

                    GetLyricsBySlugResult result = await dispatcher.Send(request: query);

                    var response = new GetLyricsBySlugResponse(Lyrics: result.Lyrics);
                    return Results.Ok(response);
                }
            )
            .WithName(endpointName: GetLyricsBySlugMetaField.GetLyricsBySlug.Name)
            .WithSummary(summary: GetLyricsBySlugMetaField.GetLyricsBySlug.Summary)
            .WithDescription(description: GetLyricsBySlugMetaField.GetLyricsBySlug.Description)
            .AllowAnonymous()
            .RequireRateLimiting(policyName: RateLimitPolicies.ContentBrowsing)
            .Produces<GetLyricsBySlugResponse>(statusCode: StatusCodes.Status200OK)
            .ProducesProblem(statusCode: StatusCodes.Status404NotFound)
            .ProducesProblem(statusCode: StatusCodes.Status429TooManyRequests);
    }
}
