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
public record UpdateArticleTagsRequest(IReadOnlyList<Guid> TagIds);

/// <summary>
/// Defines the admin update article tags endpoint.
/// Handles replacing all tag associations on an article.
/// </summary>
public class UpdateArticleTagsEndpointV1 : ICarterModule
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
                async (string id, UpdateArticleTagsRequest request, IDispatcher dispatcher) =>
                {
                    var command = new UpdateArticleTagsCommand(ArticleId: id, TagIds: request.TagIds);

                    await dispatcher.Send(request: command);
                    return Results.NoContent();
                }
            )
            .WithName(endpointName: UpdateArticleTagsMetaField.UpdateArticleTags.Name)
            .WithSummary(summary: UpdateArticleTagsMetaField.UpdateArticleTags.Summary)
            .WithDescription(description: UpdateArticleTagsMetaField.UpdateArticleTags.Description)
            .WithAuthorization(AccountStatusPolicies.RequireActiveUser)
            .WithAuthorization(UserRolePolicies.RequireAdminOrSuperAdmin)
            .RequireRateLimiting(policyName: RateLimitPolicies.ContentBrowsing)
            .Produces(statusCode: StatusCodes.Status204NoContent)
            .ProducesProblem(statusCode: StatusCodes.Status401Unauthorized)
            .ProducesProblem(statusCode: StatusCodes.Status403Forbidden)
            .ProducesProblem(statusCode: StatusCodes.Status404NotFound)
            .ProducesProblem(statusCode: StatusCodes.Status429TooManyRequests);
    }
}
