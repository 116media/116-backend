using _116.BuildingBlocks.Constants.Authorization.Policies;
using _116.BuildingBlocks.Constants.RateLimit;
using _116.Content.Application.Editorial.Constants;
using _116.Content.Domain.Constants;
using _116.Shared.Application.Extensions;
using _116.Shared.Contracts.Application.CQRS;
using Carter;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace _116.Content.Application.Editorial.UseCases.Admin.Commands.UpdateArticleTags.V1;

/// <summary>
/// Request model for updating article tags.
/// </summary>
/// <param name="TagIds">The complete set of tag identifiers to assign to this article.</param>
public record AdminUpdateArticleTagsRequest(IReadOnlyList<Guid> TagIds);

/// <summary>
/// Response model for a successful UpdateArticleTags operation.
/// </summary>
/// <param name="IsSuccess">Indicates if the operation was successful.</param>
public record AdminUpdateArticleTagsResponse(bool IsSuccess);

/// <summary>
/// Defines the admin update article tags endpoint.
/// Handles replacing all tag associations on an article.
/// </summary>
public class AdminUpdateArticleTagsEndpointV1 : ICarterModule
{
    /// <summary>
    /// Configures the article tags update route within the API pipeline.
    /// Maps the <c>PUT /api/v1/admin/articles/{id}/tags</c> endpoint.
    /// </summary>
    /// <param name="app">The route builder used to register API endpoints.</param>
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        RouteGroupBuilder group = app.MapApiVersionGroup(1)
            .MapGroup($"{ContentConstants.Admin}/{EditorialRouteConstants.Articles}")
            .WithTags($"{ContentConstants.Admin}::{EditorialRouteConstants.Articles}");

        group
            .MapPut(
                $"/{{id}}/{EditorialRouteConstants.Tags}",
                async (string id, AdminUpdateArticleTagsRequest request, IDispatcher dispatcher) =>
                {
                    var command = new AdminUpdateArticleTagsCommand(ArticleId: id, TagIds: request.TagIds);
                    AdminUpdateArticleTagsResult result = await dispatcher.Send(request: command);

                    var response = new AdminUpdateArticleTagsResponse(IsSuccess: result.IsSuccess);
                    return Results.Ok(response);
                }
            )
            .WithName(endpointName: AdminUpdateArticleTagsMetaField.AdminUpdateArticleTags.Name)
            .WithSummary(summary: AdminUpdateArticleTagsMetaField.AdminUpdateArticleTags.Summary)
            .WithDescription(description: AdminUpdateArticleTagsMetaField.AdminUpdateArticleTags.Description)
            .WithAuthorization(AccountStatusPolicies.RequireActiveUser)
            .WithAuthorization(UserRolePolicies.RequireAdminOrSuperAdmin)
            .RequireRateLimiting(policyName: RateLimitPolicies.ContentBrowsing)
            .Produces<AdminUpdateArticleTagsResponse>(statusCode: StatusCodes.Status200OK)
            .ProducesProblem(statusCode: StatusCodes.Status401Unauthorized)
            .ProducesProblem(statusCode: StatusCodes.Status403Forbidden)
            .ProducesProblem(statusCode: StatusCodes.Status404NotFound)
            .ProducesProblem(statusCode: StatusCodes.Status429TooManyRequests);
    }
}
