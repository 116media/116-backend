using _116.BuildingBlocks.Constants.RateLimit;
using _116.Content.Application.Interactions.Constants;
using _116.Content.Application.Shared.DTOs;
using _116.Content.Domain.Constants;
using _116.Shared.Application.Extensions;
using _116.Shared.Application.Pagination;
using _116.Shared.Contracts.Application.CQRS;
using Carter;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace _116.Content.Application.Interactions.UseCases.Public.Queries.GetArticleComments.V1;

/// <summary>
/// Defines the get article comments endpoint. Allows anonymous access.
/// </summary>
public class PublicGetArticleCommentsEndpointV1 : ICarterModule
{
    /// <inheritdoc />
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        RouteGroupBuilder group = app.MapApiVersionGroup(1)
            .MapGroup($"{ContentConstants.Public}/{InteractionsRouteConstants.Articles}")
            .WithTags($"{ContentConstants.Public}::{InteractionsRouteConstants.Articles}");

        group
            .MapGet(
                $"/{{id:guid}}/{InteractionsRouteConstants.Comments}",
                async (Guid id, int pageIndex, int pageSize, IDispatcher dispatcher) =>
                {
                    var paginatedRequest = new PaginatedRequest(PageIndex: pageIndex, PageSize: pageSize);
                    var query = new PublicGetArticleCommentsQuery(ArticleId: id, PaginatedRequest: paginatedRequest);

                    PublicGetArticleCommentsResult result = await dispatcher.Send(request: query);
                    return Results.Ok(result.Comments);
                }
            )
            .WithName(endpointName: PublicGetArticleCommentsMetaField.PublicGetArticleComments.Name)
            .WithSummary(summary: PublicGetArticleCommentsMetaField.PublicGetArticleComments.Summary)
            .WithDescription(description: PublicGetArticleCommentsMetaField.PublicGetArticleComments.Description)
            .AllowAnonymous()
            .RequireRateLimiting(policyName: RateLimitPolicies.ContentBrowsing)
            .Produces<PaginatedResult<ArticleCommentDto>>(statusCode: StatusCodes.Status200OK)
            .ProducesProblem(statusCode: StatusCodes.Status404NotFound)
            .ProducesProblem(statusCode: StatusCodes.Status429TooManyRequests);
    }
}
