using System.Security.Claims;
using _116.BuildingBlocks.Constants.Authorization.Policies;
using _116.BuildingBlocks.Constants.RateLimit;
using _116.Content.Application.Editorial.Constants;
using _116.Identity.Contracts.Application;
using _116.Shared.Application.Extensions;
using _116.Shared.Contracts.Application.CQRS;
using Carter;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace _116.Content.Application.Editorial.UseCases.Public.Commands.ProposeTranslationRevision.V1;

/// <summary>
/// Request model for proposing a correction to a translation.
/// </summary>
/// <param name="ProposedText">The proposed replacement text.</param>
/// <param name="EditSummary">Optional free-text summary of what changed and why.</param>
public record PublicProposeTranslationRevisionRequest(string ProposedText, string? EditSummary);

/// <summary>
/// Response model for a successful ProposeTranslationRevision operation.
/// </summary>
/// <param name="RevisionId">The unique identifier of the newly proposed revision.</param>
public record PublicProposeTranslationRevisionResponse(Guid RevisionId);

/// <summary>
/// Defines the public propose translation revision endpoint.
/// </summary>
public class PublicProposeTranslationRevisionEndpointV1 : ICarterModule
{
    /// <summary>
    /// Configures the translation revision proposal route within the API pipeline.
    /// Maps the <c>POST /api/v1/translations/{id}/revisions</c> endpoint.
    /// </summary>
    /// <param name="app">The route builder used to register API endpoints.</param>
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        RouteGroupBuilder group = app.MapApiVersionGroup(1)
            .MapGroup($"{EditorialRouteConstants.Translations}")
            .WithTags(EditorialRouteConstants.Translations);

        group
            .MapPost(
                $"/{{id}}/{EditorialRouteConstants.Revisions}",
                async (
                    Guid id,
                    PublicProposeTranslationRevisionRequest request,
                    ClaimsPrincipal user,
                    IClaimsProvider claimsProvider,
                    IDispatcher dispatcher
                ) =>
                {
                    Guid userId = claimsProvider.GetUserIdFromClaims(user: user);

                    var command = new PublicProposeTranslationRevisionCommand(
                        TranslationId: id,
                        ProposedText: request.ProposedText,
                        EditSummary: request.EditSummary,
                        UserId: userId
                    );
                    PublicProposeTranslationRevisionResult result = await dispatcher.Send(request: command);

                    var response = new PublicProposeTranslationRevisionResponse(RevisionId: result.RevisionId);
                    return Results.Ok(response);
                }
            )
            .WithName(endpointName: PublicProposeTranslationRevisionMetaField.ProposeTranslationRevision.Name)
            .WithSummary(summary: PublicProposeTranslationRevisionMetaField.ProposeTranslationRevision.Summary)
            .WithDescription(
                description: PublicProposeTranslationRevisionMetaField.ProposeTranslationRevision.Description
            )
            .WithAuthorization(AccountStatusPolicies.RequireActiveUser)
            .RequireRateLimiting(policyName: RateLimitPolicies.ContentContribution)
            .ProducesValidationProblem()
            .Produces<PublicProposeTranslationRevisionResponse>(statusCode: StatusCodes.Status200OK)
            .ProducesProblem(statusCode: StatusCodes.Status400BadRequest)
            .ProducesProblem(statusCode: StatusCodes.Status401Unauthorized)
            .ProducesProblem(statusCode: StatusCodes.Status404NotFound)
            .ProducesProblem(statusCode: StatusCodes.Status429TooManyRequests);
    }
}
