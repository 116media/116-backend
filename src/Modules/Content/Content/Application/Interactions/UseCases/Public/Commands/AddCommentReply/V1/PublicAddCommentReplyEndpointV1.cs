using System.Security.Claims;
using _116.BuildingBlocks.Constants.Authorization.Policies;
using _116.BuildingBlocks.Constants.RateLimit;
using _116.BuildingBlocks.Utils;
using _116.Content.Application.Interactions.Constants;
using _116.Content.Application.Shared.DTOs;
using _116.Content.Domain.Constants;
using _116.Identity.Contracts.Application;
using _116.Shared.Application.Extensions;
using _116.Shared.Contracts.Application.CQRS;
using Carter;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace _116.Content.Application.Interactions.UseCases.Public.Commands.AddCommentReply.V1;

/// <summary>
/// Request body for replying to an article comment.
/// </summary>
/// <param name="Body">The reply text.</param>
public record PublicAddCommentReplyRequest(string Body);

/// <summary>
/// Response model for a successful PublicAddCommentReply operation.
/// </summary>
/// <param name="Reply">The newly created reply DTO.</param>
public record PublicAddCommentReplyResponse(ArticleCommentDto Reply);

/// <summary>
/// Defines the reply-to-comment endpoint.
/// </summary>
public class PublicAddCommentReplyEndpointV1 : ICarterModule
{
    /// <inheritdoc />
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        RouteGroupBuilder group = app.MapApiVersionGroup(1)
            .MapGroup($"{ContentConstants.Public}/{InteractionsRouteConstants.Articles}")
            .WithTags($"{ContentConstants.Public}::{InteractionsRouteConstants.Articles}");

        group
            .MapPost(
                $"/{{id:guid}}/{InteractionsRouteConstants.Comments}/{{commentId:guid}}/{InteractionsRouteConstants.Replies}",
                async (
                    Guid id,
                    Guid commentId,
                    PublicAddCommentReplyRequest request,
                    ClaimsPrincipal user,
                    IClaimsProvider claimsProvider,
                    IDispatcher dispatcher,
                    HttpContext httpContext
                ) =>
                {
                    Guid userId = claimsProvider.GetUserIdFromClaims(user: user);

                    var command = new PublicAddCommentReplyCommand(
                        ArticleId: id,
                        ParentCommentId: commentId,
                        UserId: userId,
                        Body: request.Body
                    );
                    PublicAddCommentReplyResult result = await dispatcher.Send(request: command);

                    var response = new PublicAddCommentReplyResponse(Reply: result.Reply);
                    string path =
                        $"{ContentConstants.Public}/{InteractionsRouteConstants.Articles}/{id}/{InteractionsRouteConstants.Comments}/{commentId}/{InteractionsRouteConstants.Replies}";
                    string locationUrl = ApiVersionUrl.Build(context: httpContext, path: path);

                    return Results.Created(uri: locationUrl, value: response);
                }
            )
            .WithName(endpointName: PublicAddCommentReplyMetaField.AddCommentReply.Name)
            .WithSummary(summary: PublicAddCommentReplyMetaField.AddCommentReply.Summary)
            .WithDescription(description: PublicAddCommentReplyMetaField.AddCommentReply.Description)
            .WithAuthorization(UserRolePolicies.RequireVisitorOnly)
            .RequireRateLimiting(policyName: RateLimitPolicies.ContentBrowsing)
            .Produces<PublicAddCommentReplyResponse>(statusCode: StatusCodes.Status201Created)
            .ProducesProblem(statusCode: StatusCodes.Status400BadRequest)
            .ProducesProblem(statusCode: StatusCodes.Status401Unauthorized)
            .ProducesProblem(statusCode: StatusCodes.Status404NotFound)
            .ProducesProblem(statusCode: StatusCodes.Status429TooManyRequests);
    }
}
