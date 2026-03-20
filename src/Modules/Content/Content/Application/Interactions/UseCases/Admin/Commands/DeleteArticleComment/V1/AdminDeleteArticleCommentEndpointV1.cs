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
/// Response model for a successful AdminDeleteArticleComment operation.
/// </summary>
/// <param name="IsSuccess">Indicates if the operation was successful.</param>
public record AdminDeleteArticleCommentResponse(bool IsSuccess);

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
                async (string id, string commentId, IDispatcher dispatcher) =>
                {
                    Guid articleId = Guid.Parse(id);
                    Guid parsedCommentId = Guid.Parse(commentId);

                    var command = new AdminDeleteArticleCommentCommand(
                        ArticleId: articleId,
                        CommentId: parsedCommentId
                    );

                    AdminDeleteArticleCommentResult result = await dispatcher.Send(request: command);

                    var response = new AdminDeleteArticleCommentResponse(IsSuccess: result.IsSuccess);
                    return Results.Ok(response);
                }
            )
            .WithName(endpointName: AdminDeleteArticleCommentMetaField.AdminDeleteArticleComment.Name)
            .WithSummary(summary: AdminDeleteArticleCommentMetaField.AdminDeleteArticleComment.Summary)
            .WithDescription(description: AdminDeleteArticleCommentMetaField.AdminDeleteArticleComment.Description)
            .WithAuthorization(UserRolePolicies.RequireAdminOrSuperAdmin)
            .RequireRateLimiting(policyName: RateLimitPolicies.ContentBrowsing)
            .Produces<AdminDeleteArticleCommentResponse>(statusCode: StatusCodes.Status200OK)
            .ProducesProblem(statusCode: StatusCodes.Status401Unauthorized)
            .ProducesProblem(statusCode: StatusCodes.Status403Forbidden)
            .ProducesProblem(statusCode: StatusCodes.Status404NotFound)
            .ProducesProblem(statusCode: StatusCodes.Status429TooManyRequests);
    }
}
