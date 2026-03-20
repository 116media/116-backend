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

namespace _116.Content.Application.Interactions.UseCases.Public.Commands.EditArticleComment.V1;

/// <summary>
/// Request body for editing an article comment.
/// </summary>
/// <param name="Body">The new comment text.</param>
public record PublicEditArticleCommentRequest(string Body);

/// <summary>
/// Response model for a successful PublicEditArticleComment operation.
/// </summary>
/// <param name="IsSuccess">Indicates if the operation was successful.</param>
public record PublicEditArticleCommentResponse(bool IsSuccess);

/// <summary>
/// Defines the edit article comment endpoint.
/// </summary>
public class PublicEditArticleCommentEndpointV1 : ICarterModule
{
    /// <inheritdoc />
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        RouteGroupBuilder group = app.MapApiVersionGroup(1)
            .MapGroup($"{ContentConstants.Public}/{InteractionsRouteConstants.Articles}")
            .WithTags($"{ContentConstants.Public}::{InteractionsRouteConstants.Articles}");

        group
            .MapPut(
                $"/{{id}}/{InteractionsRouteConstants.Comments}/{{commentId}}",
                async (
                    string id,
                    string commentId,
                    PublicEditArticleCommentRequest request,
                    ClaimsPrincipal user,
                    IClaimsProvider claimsProvider,
                    IDispatcher dispatcher
                ) =>
                {
                    Guid articleId = Guid.Parse(id);
                    Guid parsedCommentId = Guid.Parse(commentId);
                    Guid userId = claimsProvider.GetUserIdFromClaims(user: user);

                    var command = new PublicEditArticleCommentCommand(
                        ArticleId: articleId,
                        CommentId: parsedCommentId,
                        UserId: userId,
                        Body: request.Body
                    );

                    PublicEditArticleCommentResult result = await dispatcher.Send(request: command);

                    var response = new PublicEditArticleCommentResponse(IsSuccess: result.IsSuccess);
                    return Results.Ok(response);
                }
            )
            .WithName(endpointName: PublicEditArticleCommentMetaField.PublicEditArticleComment.Name)
            .WithSummary(summary: PublicEditArticleCommentMetaField.PublicEditArticleComment.Summary)
            .WithDescription(description: PublicEditArticleCommentMetaField.PublicEditArticleComment.Description)
            .WithAuthorization(UserRolePolicies.RequireVisitorOnly)
            .RequireRateLimiting(policyName: RateLimitPolicies.ContentBrowsing)
            .Produces<PublicEditArticleCommentResponse>(statusCode: StatusCodes.Status200OK)
            .ProducesProblem(statusCode: StatusCodes.Status400BadRequest)
            .ProducesProblem(statusCode: StatusCodes.Status401Unauthorized)
            .ProducesProblem(statusCode: StatusCodes.Status404NotFound)
            .ProducesProblem(statusCode: StatusCodes.Status429TooManyRequests);
    }
}
