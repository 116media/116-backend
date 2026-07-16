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

namespace _116.Content.Application.Interactions.UseCases.Public.Queries.GetOwnLikedArticles.V1;

/// <summary>
/// Defines the current-user liked articles endpoint.
/// </summary>
public class PublicGetOwnLikedArticlesEndpointV1 : ICarterModule
{
    /// <inheritdoc />
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        RouteGroupBuilder group = app.MapApiVersionGroup(1)
            .MapGroup($"{ContentConstants.Public}/{InteractionsRouteConstants.Articles}")
            .WithTags($"{ContentConstants.Public}::{InteractionsRouteConstants.Articles}");

        group
            .MapGet(
                $"/{InteractionsRouteConstants.Liked}",
                async (
                    ClaimsPrincipal user,
                    IClaimsProvider claims,
                    IDispatcher dispatcher,
                    int pageIndex = 0,
                    int pageSize = 10
                ) =>
                {
                    Guid userId = claims.GetUserIdFromClaims(user);
                    var query = new PublicGetOwnLikedArticlesQuery(userId, new PaginatedRequest(pageIndex, pageSize));
                    PublicGetOwnLikedArticlesResult result = await dispatcher.Send(query);
                    return Results.Ok(result.Articles);
                }
            )
            .WithName(PublicGetOwnLikedArticlesMetaField.GetOwnLikedArticles.Name)
            .WithSummary(PublicGetOwnLikedArticlesMetaField.GetOwnLikedArticles.Summary)
            .WithDescription(PublicGetOwnLikedArticlesMetaField.GetOwnLikedArticles.Description)
            .WithAuthorization(UserRolePolicies.RequireVisitorOnly)
            .RequireRateLimiting(RateLimitPolicies.ContentBrowsing)
            .Produces<PaginatedResult<UserArticleActivityDto>>(statusCode: StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status429TooManyRequests);
    }
}
