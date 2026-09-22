using _116.BuildingBlocks.Constants.Authorization.Policies;
using _116.BuildingBlocks.Constants.RateLimit;
using _116.Content.Application.Interactions.Constants;
using _116.Content.Domain.Constants;
using _116.Shared.Application.Extensions;
using _116.Shared.Contracts.Application.CQRS;
using Carter;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace _116.Content.Application.Interactions.UseCases.Admin.Commands.DeleteArticleComment.V1;

/// <summary>
/// Defines the admin delete article comment endpoint.
/// </summary>
public class AdminDeleteArticleCommentEndpointV1 : ICarterModule
{
    /// <inheritdoc />
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        RouteGroupBuilder group = app.MapApiVersionGroup(1)
            .MapGroup($"{ContentConstants.Admin}/{InteractionsRouteConstants.Articles}")
            .WithTags($"{ContentConstants.Admin}::{InteractionsRouteConstants.Articles}");

        group
            .MapDelete(
                $"/{{id}}/{InteractionsRouteConstants.Comments}/{{commentId}}",
                async (string id, string commentId, IDispatcher dispatcher, CancellationToken cancellationToken) =>
                {
                    Guid articleId = Guid.Parse(id);
                    Guid parsedCommentId = Guid.Parse(commentId);

                    var command = new AdminDeleteArticleCommentCommand(
                        ArticleId: articleId,
                        CommentId: parsedCommentId
                    );

                    await dispatcher.Send(request: command, cancellationToken: cancellationToken);
                    return Results.NoContent();
                }
            )
            .WithName(endpointName: AdminDeleteArticleCommentMetaField.DeleteArticleComment.Name)
            .WithSummary(summary: AdminDeleteArticleCommentMetaField.DeleteArticleComment.Summary)
            .WithDescription(description: AdminDeleteArticleCommentMetaField.DeleteArticleComment.Description)
            .WithAuthorization(UserRolePolicies.RequireAdminOrSuperAdmin)
            .RequireRateLimiting(policyName: RateLimitPolicies.ContentManagement)
            .Produces(statusCode: StatusCodes.Status204NoContent)
            .ProducesProblem(statusCode: StatusCodes.Status401Unauthorized)
            .ProducesProblem(statusCode: StatusCodes.Status403Forbidden)
            .ProducesProblem(statusCode: StatusCodes.Status404NotFound)
            .ProducesProblem(statusCode: StatusCodes.Status429TooManyRequests);
    }
}
