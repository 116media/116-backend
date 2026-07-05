using System.Security.Claims;
using _116.BuildingBlocks.Constants.RateLimit;
using _116.Content.Application.Interactions.Constants;
using _116.Content.Application.Shared.DTOs;
using _116.Content.Domain.Constants;
using _116.Identity.Contracts.Application;
using _116.Shared.Application.Extensions;
using _116.Shared.Application.Pagination;
using _116.Shared.Contracts.Application.CQRS;
using Carter;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace _116.Content.Application.Interactions.UseCases.Public.Queries.GetCommentReplies.V1;

/// <summary>
/// Defines the get comment replies endpoint. Allows anonymous access.
/// </summary>
public class PublicGetCommentRepliesEndpointV1 : ICarterModule
{
    /// <inheritdoc />
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        RouteGroupBuilder group = app.MapApiVersionGroup(1)
            .MapGroup($"{ContentConstants.Public}/{InteractionsRouteConstants.Articles}")
            .WithTags($"{ContentConstants.Public}::{InteractionsRouteConstants.Articles}");

        group
            .MapGet(
                $"/{InteractionsRouteConstants.Comments}/{{commentId:guid}}/{InteractionsRouteConstants.Replies}",
                async (
                    Guid commentId,
                    ClaimsPrincipal user,
                    IClaimsProvider claimsProvider,
                    IDispatcher dispatcher,
                    int pageIndex = 0,
                    int pageSize = 10
                ) =>
                {
                    Guid? viewerUserId = null;

                    if (user.Identity?.IsAuthenticated == true)
                    {
                        viewerUserId = claimsProvider.GetUserIdFromClaims(user: user);
                    }

                    var paginatedRequest = new PaginatedRequest(PageIndex: pageIndex, PageSize: pageSize);
                    var query = new PublicGetCommentRepliesQuery(
                        CommentId: commentId,
                        PaginatedRequest: paginatedRequest,
                        ViewerUserId: viewerUserId
                    );

                    PublicGetCommentRepliesResult result = await dispatcher.Send(request: query);
                    return Results.Ok(result.Replies);
                }
            )
            .WithName(endpointName: PublicGetCommentRepliesMetaField.GetCommentReplies.Name)
            .WithSummary(summary: PublicGetCommentRepliesMetaField.GetCommentReplies.Summary)
            .WithDescription(description: PublicGetCommentRepliesMetaField.GetCommentReplies.Description)
            .AllowAnonymous()
            .RequireRateLimiting(policyName: RateLimitPolicies.ContentBrowsing)
            .Produces<PaginatedResult<ArticleCommentDto>>(statusCode: StatusCodes.Status200OK)
            .ProducesProblem(statusCode: StatusCodes.Status429TooManyRequests);
    }
}
