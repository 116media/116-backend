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

namespace _116.Content.Application.Editorial.UseCases.Public.Commands.VoteOnLyricsRevision.V1;

/// <summary>
/// Request model for voting on a lyrics-text correction revision.
/// </summary>
/// <param name="Vote">Whether the voter approves or rejects the proposed revision.</param>
/// <param name="Comment">Optional free-text comment justifying the vote.</param>
public record PublicVoteOnLyricsRevisionRequest(EnumVote Vote, string? Comment);

/// <summary>
/// Response model for a successful VoteOnLyricsRevision operation.
/// </summary>
/// <param name="IsSuccess">Indicates if the operation was successful.</param>
public record PublicVoteOnLyricsRevisionResponse(bool IsSuccess);

/// <summary>
/// Defines the public vote on lyrics revision endpoint.
/// </summary>
public class PublicVoteOnLyricsRevisionEndpointV1 : ICarterModule
{
    /// <summary>
    /// Configures the lyrics revision voting route within the API pipeline.
    /// Maps the <c>POST /api/v1/lyrics/revisions/{id}/votes</c> endpoint.
    /// </summary>
    /// <param name="app">The route builder used to register API endpoints.</param>
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        RouteGroupBuilder group = app.MapApiVersionGroup(1)
            .MapGroup($"{EditorialRouteConstants.Lyrics}")
            .WithTags(EditorialRouteConstants.Lyrics);

        group
            .MapPost(
                $"/{EditorialRouteConstants.Revisions}/{{id}}/{EditorialRouteConstants.Votes}",
                async (
                    Guid id,
                    PublicVoteOnLyricsRevisionRequest request,
                    ClaimsPrincipal user,
                    IClaimsProvider claimsProvider,
                    IDispatcher dispatcher
                ) =>
                {
                    Guid userId = claimsProvider.GetUserIdFromClaims(user: user);

                    var command = new PublicVoteOnLyricsRevisionCommand(
                        RevisionId: id,
                        Vote: request.Vote,
                        Comment: request.Comment,
                        UserId: userId
                    );
                    PublicVoteOnLyricsRevisionResult result = await dispatcher.Send(request: command);

                    var response = new PublicVoteOnLyricsRevisionResponse(IsSuccess: result.IsSuccess);
                    return Results.Ok(response);
                }
            )
            .WithName(endpointName: PublicVoteOnLyricsRevisionMetaField.VoteOnLyricsRevision.Name)
            .WithSummary(summary: PublicVoteOnLyricsRevisionMetaField.VoteOnLyricsRevision.Summary)
            .WithDescription(description: PublicVoteOnLyricsRevisionMetaField.VoteOnLyricsRevision.Description)
            .WithAuthorization(AccountStatusPolicies.RequireActiveUser)
            .RequireRateLimiting(policyName: RateLimitPolicies.ContentContribution)
            .Produces<PublicVoteOnLyricsRevisionResponse>(statusCode: StatusCodes.Status200OK)
            .ProducesProblem(statusCode: StatusCodes.Status401Unauthorized)
            .ProducesProblem(statusCode: StatusCodes.Status404NotFound)
            .ProducesProblem(statusCode: StatusCodes.Status409Conflict)
            .ProducesProblem(statusCode: StatusCodes.Status429TooManyRequests);
    }
}
