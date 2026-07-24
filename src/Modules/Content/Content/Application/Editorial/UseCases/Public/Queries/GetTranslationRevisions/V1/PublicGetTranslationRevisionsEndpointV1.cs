using _116.BuildingBlocks.Constants.RateLimit;
using _116.Content.Application.Editorial.Constants;
using _116.Shared.Application.Extensions;
using _116.Shared.Contracts.Application.CQRS;
using Carter;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace _116.Content.Application.Editorial.UseCases.Public.Queries.GetTranslationRevisions.V1;

/// <summary>
/// Response model for listing a translation's full revision history.
/// </summary>
/// <param name="Revisions">The translation's full revision history, newest first.</param>
public record PublicGetTranslationRevisionsResponse(IReadOnlyList<TranslationRevisionDto> Revisions);

/// <summary>
/// Defines the public get translation revisions endpoint.
/// </summary>
public class PublicGetTranslationRevisionsEndpointV1 : ICarterModule
{
    /// <summary>
    /// Configures the translation revision history route within the API pipeline.
    /// Maps the <c>GET /api/v1/translations/{id}/revisions</c> endpoint.
    /// </summary>
    /// <param name="app">The route builder used to register API endpoints.</param>
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        RouteGroupBuilder group = app.MapApiVersionGroup(1)
            .MapGroup($"{EditorialRouteConstants.Translations}")
            .WithTags(EditorialRouteConstants.Translations);

        group
            .MapGet(
                $"/{{id}}/{EditorialRouteConstants.Revisions}",
                async (Guid id, IDispatcher dispatcher) =>
                {
                    var query = new PublicGetTranslationRevisionsQuery(TranslationId: id);
                    PublicGetTranslationRevisionsResult result = await dispatcher.Send(request: query);

                    var response = new PublicGetTranslationRevisionsResponse(Revisions: result.Revisions);
                    return Results.Ok(response);
                }
            )
            .WithName(endpointName: PublicGetTranslationRevisionsMetaField.GetTranslationRevisions.Name)
            .WithSummary(summary: PublicGetTranslationRevisionsMetaField.GetTranslationRevisions.Summary)
            .WithDescription(description: PublicGetTranslationRevisionsMetaField.GetTranslationRevisions.Description)
            .AllowAnonymous()
            .RequireRateLimiting(policyName: RateLimitPolicies.ContentBrowsing)
            .Produces<PublicGetTranslationRevisionsResponse>(statusCode: StatusCodes.Status200OK)
            .ProducesProblem(statusCode: StatusCodes.Status404NotFound)
            .ProducesProblem(statusCode: StatusCodes.Status429TooManyRequests);
    }
}
