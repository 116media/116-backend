using _116.BuildingBlocks.Constants.Authorization.Policies;
using _116.BuildingBlocks.Constants.RateLimit;
using _116.Content.Application.Editorial.Constants;
using _116.Content.Application.Shared.DTOs;
using _116.Content.Domain.Constants;
using _116.Content.Domain.Enums;
using _116.Shared.Application.Extensions;
using _116.Shared.Application.Pagination;
using _116.Shared.Contracts.Application.CQRS;
using Carter;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace _116.Content.Application.Editorial.UseCases.Admin.Queries.GetAllArticles.V1;

/// <summary>
/// Response model for listing all articles.
/// </summary>
/// <param name="Articles">Paginated result containing article summary DTOs and pagination metadata.</param>
public record AdminGetAllArticlesResponse(PaginatedResult<ArticleSummaryDto> Articles);

/// <summary>
/// Defines the admin get all articles endpoint.
/// Returns a paginated list of articles with optional filters.
/// </summary>
public class AdminGetAllArticlesEndpointV1 : ICarterModule
{
    /// <summary>
    /// Configures the article retrieval route within the API pipeline.
    /// Maps the <c>GET /api/v1/admin/articles</c> endpoint to handle article listing requests.
    /// </summary>
    /// <param name="app">The route builder used to register API endpoints.</param>
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        RouteGroupBuilder group = app.MapApiVersionGroup(1)
            .MapGroup($"{ContentConstants.Admin}/{EditorialRouteConstants.Articles}")
            .WithTags($"{ContentConstants.Admin}::{EditorialRouteConstants.Articles}");

        group
            .MapGet(
                "/",
                async (
                    IDispatcher dispatcher,
                    int pageIndex = 0,
                    int pageSize = 10,
                    string? search = null,
                    EnumContentStatus? status = null,
                    Guid? categoryId = null
                ) =>
                {
                    var paginatedRequest = new PaginatedRequest(pageIndex, pageSize);

                    var query = new AdminGetAllArticlesQuery(
                        PaginatedRequest: paginatedRequest,
                        Search: search,
                        Status: status,
                        CategoryId: categoryId
                    );

                    AdminGetAllArticlesResult result = await dispatcher.Send(request: query);

                    var response = new AdminGetAllArticlesResponse(Articles: result.Articles);
                    return Results.Ok(response);
                }
            )
            .WithName(endpointName: AdminGetAllArticlesMetaField.AdminGetAllArticles.Name)
            .WithSummary(summary: AdminGetAllArticlesMetaField.AdminGetAllArticles.Summary)
            .WithDescription(description: AdminGetAllArticlesMetaField.AdminGetAllArticles.Description)
            .WithAuthorization(AccountStatusPolicies.RequireActiveUser)
            .WithAuthorization(UserRolePolicies.RequireAdminOrSuperAdmin)
            .RequireRateLimiting(policyName: RateLimitPolicies.ContentBrowsing)
            .Produces<AdminGetAllArticlesResponse>(statusCode: StatusCodes.Status200OK)
            .ProducesProblem(statusCode: StatusCodes.Status401Unauthorized)
            .ProducesProblem(statusCode: StatusCodes.Status403Forbidden)
            .ProducesProblem(statusCode: StatusCodes.Status429TooManyRequests);
    }
}
