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

namespace _116.Content.Application.Interactions.UseCases.Public.Queries.GetMyArticleBookmarks.V1;

/// <summary>
/// Defines the get my article bookmarks endpoint.
/// </summary>
public class PublicGetMyArticleBookmarksEndpointV1 : ICarterModule
{
    /// <inheritdoc />
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        RouteGroupBuilder group = app.MapApiVersionGroup(1)
            .MapGroup($"{ContentConstants.Public}/{InteractionsRouteConstants.Articles}")
            .WithTags($"{ContentConstants.Public}::{InteractionsRouteConstants.Articles}");

        group
            .MapGet(
                $"/{InteractionsRouteConstants.Bookmarks}",
                async (
                    ClaimsPrincipal user,
                    IClaimsProvider claimsProvider,
                    IDispatcher dispatcher,
                    int pageIndex = 0,
                    int pageSize = 10
                ) =>
                {
                    Guid userId = claimsProvider.GetUserIdFromClaims(user: user);
                    var paginatedRequest = new PaginatedRequest(PageIndex: pageIndex, PageSize: pageSize);
                    var query = new PublicGetMyArticleBookmarksQuery(
                        UserId: userId,
                        PaginatedRequest: paginatedRequest
                    );

                    PublicGetMyArticleBookmarksResult result = await dispatcher.Send(request: query);
                    return Results.Ok(result.Articles);
                }
            )
            .WithName(endpointName: PublicGetMyArticleBookmarksMetaField.PublicGetMyArticleBookmarks.Name)
            .WithSummary(summary: PublicGetMyArticleBookmarksMetaField.PublicGetMyArticleBookmarks.Summary)
            .WithDescription(description: PublicGetMyArticleBookmarksMetaField.PublicGetMyArticleBookmarks.Description)
            .WithAuthorization(UserRolePolicies.RequireVisitorOnly)
            .RequireRateLimiting(policyName: RateLimitPolicies.ContentBrowsing)
            .Produces<PaginatedResult<ArticleSummaryDto>>(statusCode: StatusCodes.Status200OK)
            .ProducesProblem(statusCode: StatusCodes.Status401Unauthorized)
            .ProducesProblem(statusCode: StatusCodes.Status429TooManyRequests);
    }
}
