using _116.BuildingBlocks.Constants.Authorization.Policies;
using _116.BuildingBlocks.Constants.RateLimit;
using _116.Content.Application.Editorial.Constants;
using _116.Content.Application.Shared.DTOs;
using _116.Content.Domain.Constants;
using _116.Shared.Application.Extensions;
using _116.Shared.Contracts.Application.CQRS;
using Carter;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace _116.Content.Application.Editorial.UseCases.Admin.Queries.GetArticleById.V1;

/// <summary>
/// Response model for retrieving an article by its identifier.
/// </summary>
/// <param name="Article">The full article detail information.</param>
public record AdminGetArticleByIdResponse(ArticleDetailDto Article);

/// <summary>
/// Defines the admin get article by id endpoint.
/// Returns the full details of a single article.
/// </summary>
public class AdminGetArticleByIdEndpointV1 : ICarterModule
{
    /// <summary>
    /// Configures the article detail retrieval route within the API pipeline.
    /// Maps the <c>GET /api/v1/admin/articles/{id}</c> endpoint.
    /// </summary>
    /// <param name="app">The route builder used to register API endpoints.</param>
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        RouteGroupBuilder group = app.MapApiVersionGroup(1)
            .MapGroup($"{ContentConstants.Admin}/{EditorialRouteConstants.Articles}")
            .WithTags($"{ContentConstants.Admin}::{EditorialRouteConstants.Articles}");

        group
            .MapGet(
                "/{id}",
                async (string id, IDispatcher dispatcher) =>
                {
                    var query = new AdminGetArticleByIdQuery(Id: id);
                    AdminGetArticleByIdResult result = await dispatcher.Send(request: query);

                    var response = new AdminGetArticleByIdResponse(Article: result.Article);
                    return Results.Ok(response);
                }
            )
            .WithName(endpointName: AdminGetArticleByIdMetaField.AdminGetArticleById.Name)
            .WithSummary(summary: AdminGetArticleByIdMetaField.AdminGetArticleById.Summary)
            .WithDescription(description: AdminGetArticleByIdMetaField.AdminGetArticleById.Description)
            .WithAuthorization(AccountStatusPolicies.RequireActiveUser)
            .WithAuthorization(UserRolePolicies.RequireAdminOrSuperAdmin)
            .RequireRateLimiting(policyName: RateLimitPolicies.ContentBrowsing)
            .Produces<AdminGetArticleByIdResponse>(statusCode: StatusCodes.Status200OK)
            .ProducesProblem(statusCode: StatusCodes.Status401Unauthorized)
            .ProducesProblem(statusCode: StatusCodes.Status403Forbidden)
            .ProducesProblem(statusCode: StatusCodes.Status404NotFound)
            .ProducesProblem(statusCode: StatusCodes.Status429TooManyRequests);
    }
}
