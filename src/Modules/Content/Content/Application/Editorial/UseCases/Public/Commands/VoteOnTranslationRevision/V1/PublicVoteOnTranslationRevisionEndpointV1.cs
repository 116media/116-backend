using System.Security.Claims;
using _116.BuildingBlocks.Constants.Authorization.Policies;
using _116.BuildingBlocks.Constants.RateLimit;
using _116.Content.Application.Editorial.Constants;
using _116.Content.Domain.Enums;
using _116.Identity.Contracts.Application;
using _116.Shared.Application.Extensions;
using _116.Shared.Contracts.Application.CQRS;
using Carter;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace _116.Content.Application.Editorial.UseCases.Public.Commands.VoteOnTranslationRevision.V1;

/// <summary>
/// Request model for voting on a translation revision.
/// </summary>
/// <param name="Vote">Whether the voter approves or rejects the proposed revision.</param>
/// <param name="Comment">Optional free-text comment justifying the vote.</param>
public record PublicVoteOnTranslationRevisionRequest(EnumVote Vote, string? Comment);

/// <summary>
/// Response model for a successful VoteOnTranslationRevision operation.
/// </summary>
/// <param name="IsSuccess">Indicates if the operation was successful.</param>
public record PublicVoteOnTranslationRevisionResponse(bool IsSuccess);

/// <summary>
/// Defines the public vote on translation revision endpoint.
/// </summary>
public class PublicVoteOnTranslationRevisionEndpointV1 : ICarterModule
{
    /// <summary>
    /// Configures the translation revision voting route within the API pipeline.
    /// Maps the <c>POST /api/v1/translations/revisions/{id}/votes</c> endpoint.
    /// </summary>
    /// <param name="app">The route builder used to register API endpoints.</param>
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        RouteGroupBuilder group = app.MapApiVersionGroup(1)
            .MapGroup($"{EditorialRouteConstants.Translations}")
            .WithTags(EditorialRouteConstants.Translations);

        group
            .MapPost(
                $"/{EditorialRouteConstants.Revisions}/{{id}}/{EditorialRouteConstants.Votes}",
                async (
                    Guid id,
                    PublicVoteOnTranslationRevisionRequest request,
                    ClaimsPrincipal user,
                    IClaimsProvider claimsProvider,
                    IDispatcher dispatcher
                ) =>
                {
                    Guid userId = claimsProvider.GetUserIdFromClaims(user: user);

                    var command = new PublicVoteOnTranslationRevisionCommand(
                        RevisionId: id,
                        Vote: request.Vote,
                        Comment: request.Comment,
                        UserId: userId
                    );
                    PublicVoteOnTranslationRevisionResult result = await dispatcher.Send(request: command);

                    var response = new PublicVoteOnTranslationRevisionResponse(IsSuccess: result.IsSuccess);
                    return Results.Ok(response);
                }
            )
            .WithName(endpointName: PublicVoteOnTranslationRevisionMetaField.VoteOnTranslationRevision.Name)
            .WithSummary(summary: PublicVoteOnTranslationRevisionMetaField.VoteOnTranslationRevision.Summary)
            .WithDescription(
                description: PublicVoteOnTranslationRevisionMetaField.VoteOnTranslationRevision.Description
            )
            .WithAuthorization(AccountStatusPolicies.RequireActiveUser)
            .RequireRateLimiting(policyName: RateLimitPolicies.ContentContribution)
            .Produces<PublicVoteOnTranslationRevisionResponse>(statusCode: StatusCodes.Status200OK)
            .ProducesProblem(statusCode: StatusCodes.Status401Unauthorized)
            .ProducesProblem(statusCode: StatusCodes.Status404NotFound)
            .ProducesProblem(statusCode: StatusCodes.Status409Conflict)
            .ProducesProblem(statusCode: StatusCodes.Status429TooManyRequests);
    }
}
