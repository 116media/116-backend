using System.Security.Claims;
using _116.BuildingBlocks.Constants.Authorization.Policies;
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

namespace _116.Content.Application.Interactions.UseCases.Public.Queries.GetOwnCommentsForArticle.V1;

/// <summary>
/// Defines the current-user comments-for-article endpoint.
/// </summary>
public class PublicGetOwnCommentsForArticleEndpointV1 : ICarterModule
{
    /// <inheritdoc />
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        RouteGroupBuilder group = app.MapApiVersionGroup(1)
            .MapGroup($"{ContentConstants.Public}/{InteractionsRouteConstants.Articles}")
            .WithTags($"{ContentConstants.Public}::{InteractionsRouteConstants.Articles}");

        group
            .MapGet(
                $"/{{id:guid}}/{InteractionsRouteConstants.Comments}/{InteractionsRouteConstants.Me}",
                async (
                    Guid id,
                    ClaimsPrincipal user,
                    IClaimsProvider claims,
                    IDispatcher dispatcher,
                    int pageIndex = 0,
                    int pageSize = 20
                ) =>
                {
                    Guid userId = claims.GetUserIdFromClaims(user);
                    var query = new PublicGetOwnCommentsForArticleQuery(
                        userId,
                        id,
                        new PaginatedRequest(pageIndex, pageSize)
                    );
                    PublicGetOwnCommentsForArticleResult result = await dispatcher.Send(query);
                    return Results.Ok(result.Comments);
                }
            )
            .WithName(PublicGetOwnCommentsForArticleMetaField.GetOwnCommentsForArticle.Name)
            .WithSummary(PublicGetOwnCommentsForArticleMetaField.GetOwnCommentsForArticle.Summary)
            .WithDescription(PublicGetOwnCommentsForArticleMetaField.GetOwnCommentsForArticle.Description)
            .WithAuthorization(UserRolePolicies.RequireVisitorOnly)
            .RequireRateLimiting(RateLimitPolicies.ContentBrowsing)
            .Produces<PaginatedResult<ArticleCommentDto>>(statusCode: StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status429TooManyRequests);
    }
}
