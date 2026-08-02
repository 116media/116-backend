using _116.BuildingBlocks.Constants.RateLimit;
using _116.Content.Application.Editorial.Constants;
using _116.Content.Domain.Constants;
using _116.Shared.Application.Extensions;
using _116.Shared.Contracts.Application.CQRS;
using Carter;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace _116.Content.Application.Editorial.UseCases.Public.Queries.GetLyricsTranslations.V1;

/// <summary>
/// Response model for listing a lyrics page's translations.
/// </summary>
/// <param name="Translations">Every translation of the lyrics page, one per requested language.</param>
public record PublicGetLyricsTranslationsResponse(IReadOnlyList<TranslationDto> Translations);

/// <summary>
/// Defines the public get lyrics translations endpoint.
/// </summary>
public class PublicGetLyricsTranslationsEndpointV1 : ICarterModule
{
    /// <summary>
    /// Configures the lyrics translations listing route within the API pipeline.
    /// Maps the <c>GET /api/v1/public/lyrics/{id}/translations</c> endpoint.
    /// </summary>
    /// <param name="app">The route builder used to register API endpoints.</param>
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        RouteGroupBuilder group = app.MapApiVersionGroup(1)
            .MapGroup($"{ContentConstants.Public}/{EditorialRouteConstants.Lyrics}")
            .WithTags($"{ContentConstants.Public}::{EditorialRouteConstants.Lyrics}");

        group
            .MapGet(
                $"/{{id}}/{EditorialRouteConstants.Translations}",
                async (Guid id, IDispatcher dispatcher) =>
                {
                    var query = new PublicGetLyricsTranslationsQuery(LyricsId: id);
                    PublicGetLyricsTranslationsResult result = await dispatcher.Send(request: query);

                    var response = new PublicGetLyricsTranslationsResponse(Translations: result.Translations);
                    return Results.Ok(response);
                }
            )
            .WithName(endpointName: PublicGetLyricsTranslationsMetaField.GetLyricsTranslations.Name)
            .WithSummary(summary: PublicGetLyricsTranslationsMetaField.GetLyricsTranslations.Summary)
            .WithDescription(description: PublicGetLyricsTranslationsMetaField.GetLyricsTranslations.Description)
            .AllowAnonymous()
            .RequireRateLimiting(policyName: RateLimitPolicies.ContentBrowsing)
            .Produces<PublicGetLyricsTranslationsResponse>(statusCode: StatusCodes.Status200OK)
            .ProducesProblem(statusCode: StatusCodes.Status404NotFound)
            .ProducesProblem(statusCode: StatusCodes.Status429TooManyRequests);
    }
}
