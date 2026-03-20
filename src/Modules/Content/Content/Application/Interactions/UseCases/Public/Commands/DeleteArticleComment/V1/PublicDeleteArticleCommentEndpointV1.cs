using System.Security.Claims;
using _116.BuildingBlocks.Constants.Authorization.Policies;
using _116.BuildingBlocks.Constants.RateLimit;
using _116.Content.Application.Interactions.Constants;
using _116.Content.Domain.Constants;
using _116.Identity.Contracts.Application;
using _116.Shared.Application.Extensions;
using _116.Shared.Contracts.Application.CQRS;
using Carter;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace _116.Content.Application.Interactions.UseCases.Public.Commands.DeleteArticleComment.V1;

/// <summary>
/// Response model for a successful PublicDeleteArticleComment operation.
/// </summary>
/// <param name="IsSuccess">Indicates if the operation was successful.</param>
public record PublicDeleteArticleCommentResponse(bool IsSuccess);

/// <summary>
/// Defines the delete article comment endpoint.
/// </summary>
public class PublicDeleteArticleCommentEndpointV1 : ICarterModule
{
    /// <inheritdoc />
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        RouteGroupBuilder group = app.MapApiVersionGroup(1)
            .MapGroup($"{ContentConstants.Public}/{InteractionsRouteConstants.Articles}")
            .WithTags($"{ContentConstants.Public}::{InteractionsRouteConstants.Articles}");

        group
            .MapDelete(
                $"/{{id}}/{InteractionsRouteConstants.Comments}/{{commentId}}",
                async (
                    string id,
                    string commentId,
                    ClaimsPrincipal user,
                    IClaimsProvider claimsProvider,
                    IDispatcher dispatcher
                ) =>
                {
                    Guid userId = claimsProvider.GetUserIdFromClaims(user: user);
                    Guid articleId = Guid.Parse(id);
                    Guid parsedCommentId = Guid.Parse(commentId);

                    var command = new PublicDeleteArticleCommentCommand(
                        UserId: userId,
                        ArticleId: articleId,
                        CommentId: parsedCommentId
                    );

                    PublicDeleteArticleCommentResult result = await dispatcher.Send(request: command);

                    var response = new PublicDeleteArticleCommentResponse(IsSuccess: result.IsSuccess);
                    return Results.Ok(response);
                }
            )
            .WithName(endpointName: PublicDeleteArticleCommentMetaField.PublicDeleteArticleComment.Name)
            .WithSummary(summary: PublicDeleteArticleCommentMetaField.PublicDeleteArticleComment.Summary)
            .WithDescription(description: PublicDeleteArticleCommentMetaField.PublicDeleteArticleComment.Description)
            .WithAuthorization(UserRolePolicies.RequireVisitorOnly)
            .RequireRateLimiting(policyName: RateLimitPolicies.ContentBrowsing)
            .Produces<PublicDeleteArticleCommentResponse>(statusCode: StatusCodes.Status200OK)
            .ProducesProblem(statusCode: StatusCodes.Status400BadRequest)
            .ProducesProblem(statusCode: StatusCodes.Status401Unauthorized)
            .ProducesProblem(statusCode: StatusCodes.Status404NotFound)
            .ProducesProblem(statusCode: StatusCodes.Status429TooManyRequests);
    }
}
