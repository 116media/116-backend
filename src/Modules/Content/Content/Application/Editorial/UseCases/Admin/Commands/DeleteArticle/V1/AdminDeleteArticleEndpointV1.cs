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

namespace _116.Content.Application.Editorial.UseCases.Admin.Commands.DeleteArticle.V1;

/// <summary>
/// Defines the admin delete article endpoint.
/// Handles permanent deletion of draft or rejected articles.
/// </summary>
public class AdminDeleteArticleEndpointV1 : ICarterModule
{
    /// <summary>
    /// Configures the article deletion route within the API pipeline.
    /// Maps the <c>DELETE /api/v1/admin/articles/{id}</c> endpoint.
    /// </summary>
    /// <param name="app">The route builder used to register API endpoints.</param>
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        RouteGroupBuilder group = app.MapApiVersionGroup(1)
            .MapGroup($"{ContentConstants.Admin}/{EditorialRouteConstants.Articles}")
            .WithTags($"{ContentConstants.Admin}::{EditorialRouteConstants.Articles}");

        group
            .MapDelete(
                "/{id}",
                async (string id, IDispatcher dispatcher, CancellationToken cancellationToken) =>
                {
                    var command = new AdminDeleteArticleCommand(Id: id);
                    await dispatcher.Send(request: command, cancellationToken: cancellationToken);
                    return Results.NoContent();
                }
            )
            .WithName(endpointName: AdminDeleteArticleMetaField.DeleteArticle.Name)
            .WithSummary(summary: AdminDeleteArticleMetaField.DeleteArticle.Summary)
            .WithDescription(description: AdminDeleteArticleMetaField.DeleteArticle.Description)
            .WithAuthorization(AccountStatusPolicies.RequireActiveUser)
            .WithAuthorization(UserRolePolicies.RequireSuperAdminOnly)
            .RequireRateLimiting(policyName: RateLimitPolicies.ContentManagement)
            .Produces(statusCode: StatusCodes.Status204NoContent)
            .ProducesProblem(statusCode: StatusCodes.Status400BadRequest)
            .ProducesProblem(statusCode: StatusCodes.Status401Unauthorized)
            .ProducesProblem(statusCode: StatusCodes.Status403Forbidden)
            .ProducesProblem(statusCode: StatusCodes.Status404NotFound)
            .ProducesProblem(statusCode: StatusCodes.Status429TooManyRequests);
    }
}
