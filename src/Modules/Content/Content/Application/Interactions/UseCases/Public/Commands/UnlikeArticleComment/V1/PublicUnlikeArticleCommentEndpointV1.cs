using System.Security.Claims;
using _116.BuildingBlocks.Constants.Authorization.Policies;
using _116.BuildingBlocks.Constants.RateLimit;
using _116.Content.Application.Interactions.Constants;
using _116.Content.Domain.Constants;
using _116.Identity.Contracts.Application.Services;
using _116.Shared.Application.Extensions;
using _116.Shared.Contracts.Application.CQRS;
using Carter;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace _116.Content.Application.Interactions.UseCases.Public.Commands.UnlikeArticleComment.V1;

/// <summary>
/// Defines the unlike article comment endpoint.
/// </summary>
public class PublicUnlikeArticleCommentEndpointV1 : ICarterModule
{
    /// <inheritdoc />
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        RouteGroupBuilder group = app.MapApiVersionGroup(1)
            .MapGroup($"{ContentConstants.Public}/{InteractionsRouteConstants.Articles}")
            .WithTags($"{ContentConstants.Public}::{InteractionsRouteConstants.Articles}");

        group
            .MapDelete(
                $"/{InteractionsRouteConstants.Comments}/{{commentId:guid}}/{InteractionsRouteConstants.Likes}",
                async (
                    Guid commentId,
                    ClaimsPrincipal user,
                    IClaimsProvider claimsProvider,
                    IDispatcher dispatcher,
                    CancellationToken cancellationToken
                ) =>
                {
                    Guid userId = claimsProvider.GetUserIdFromClaims(user: user);

                    var command = new PublicUnlikeArticleCommentCommand(CommentId: commentId, UserId: userId);
                    await dispatcher.Send(request: command, cancellationToken: cancellationToken);
                    return Results.NoContent();
                }
            )
            .WithName(endpointName: PublicUnlikeArticleCommentMetaField.UnlikeArticleComment.Name)
            .WithSummary(summary: PublicUnlikeArticleCommentMetaField.UnlikeArticleComment.Summary)
            .WithDescription(description: PublicUnlikeArticleCommentMetaField.UnlikeArticleComment.Description)
            .WithAuthorization(UserRolePolicies.RequireVisitorOnly)
            .RequireRateLimiting(policyName: RateLimitPolicies.ContentContribution)
            .Produces(statusCode: StatusCodes.Status204NoContent)
            .ProducesProblem(statusCode: StatusCodes.Status401Unauthorized)
            .ProducesProblem(statusCode: StatusCodes.Status404NotFound)
            .ProducesProblem(statusCode: StatusCodes.Status429TooManyRequests);
    }
}
