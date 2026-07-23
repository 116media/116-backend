using _116.BuildingBlocks.Constants.Authorization.Policies;
using _116.BuildingBlocks.Constants.RateLimit;
using _116.Content.Application.Editorial.Constants;
using _116.Content.Domain.Constants;
using _116.Shared.Application.Extensions;
using _116.Shared.Contracts.Application.CQRS;
using Carter;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace _116.Content.Application.Editorial.UseCases.Public.Commands.RequestLyricsTranslation.V1;

/// <summary>
/// Request model for requesting a lyrics page translation.
/// </summary>
/// <param name="Language">ISO 639-1 (or BCP-47) code of the language to translate into.</param>
public record PublicRequestLyricsTranslationRequest(string Language);

/// <summary>
/// Response model for a successful RequestLyricsTranslation operation.
/// </summary>
/// <param name="Text">The translated text, either freshly generated or previously stored.</param>
/// <param name="Source">Where the translated text came from — <c>Ai</c> or <c>Community</c>.</param>
public record PublicRequestLyricsTranslationResponse(string Text, string Source);

/// <summary>
/// Defines the public request lyrics translation endpoint.
/// </summary>
public class PublicRequestLyricsTranslationEndpointV1 : ICarterModule
{
    /// <summary>
    /// Configures the lyrics translation request route within the API pipeline.
    /// Maps the <c>POST /api/v1/public/lyrics/{id}/translations</c> endpoint.
    /// </summary>
    /// <param name="app">The route builder used to register API endpoints.</param>
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        RouteGroupBuilder group = app.MapApiVersionGroup(1)
            .MapGroup($"{ContentConstants.Public}/{EditorialRouteConstants.Lyrics}")
            .WithTags($"{ContentConstants.Public}::{EditorialRouteConstants.Lyrics}");

        group
            .MapPost(
                $"/{{id}}/{EditorialRouteConstants.Translations}",
                async (Guid id, PublicRequestLyricsTranslationRequest request, IDispatcher dispatcher) =>
                {
                    var command = new PublicRequestLyricsTranslationCommand(LyricsId: id, Language: request.Language);
                    PublicRequestLyricsTranslationResult result = await dispatcher.Send(request: command);

                    var response = new PublicRequestLyricsTranslationResponse(Text: result.Text, Source: result.Source);
                    return Results.Ok(response);
                }
            )
            .WithName(endpointName: PublicRequestLyricsTranslationMetaField.RequestLyricsTranslation.Name)
            .WithSummary(summary: PublicRequestLyricsTranslationMetaField.RequestLyricsTranslation.Summary)
            .WithDescription(description: PublicRequestLyricsTranslationMetaField.RequestLyricsTranslation.Description)
            .WithAuthorization(AccountStatusPolicies.RequireActiveUser)
            .RequireRateLimiting(policyName: RateLimitPolicies.ContentContribution)
            .Produces<PublicRequestLyricsTranslationResponse>(statusCode: StatusCodes.Status200OK)
            .ProducesProblem(statusCode: StatusCodes.Status401Unauthorized)
            .ProducesProblem(statusCode: StatusCodes.Status404NotFound)
            .ProducesProblem(statusCode: StatusCodes.Status429TooManyRequests);
    }
}
