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
/// Response model for a successful DeleteArticle operation.
/// </summary>
/// <param name="IsSuccess">Indicates if the operation was successful.</param>
public record AdminDeleteArticleResponse(bool IsSuccess);

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
                async (string id, IDispatcher dispatcher) =>
                {
                    var command = new AdminDeleteArticleCommand(Id: id);
                    AdminDeleteArticleResult result = await dispatcher.Send(request: command);

                    var response = new AdminDeleteArticleResponse(IsSuccess: result.IsSuccess);
                    return Results.Ok(response);
                }
            )
            .WithName(endpointName: AdminDeleteArticleMetaField.AdminDeleteArticle.Name)
            .WithSummary(summary: AdminDeleteArticleMetaField.AdminDeleteArticle.Summary)
            .WithDescription(description: AdminDeleteArticleMetaField.AdminDeleteArticle.Description)
            .WithAuthorization(AccountStatusPolicies.RequireActiveUser)
            .WithAuthorization(UserRolePolicies.RequireSuperAdminOnly)
            .RequireRateLimiting(policyName: RateLimitPolicies.ContentBrowsing)
            .Produces<AdminDeleteArticleResponse>(statusCode: StatusCodes.Status200OK)
            .ProducesProblem(statusCode: StatusCodes.Status400BadRequest)
            .ProducesProblem(statusCode: StatusCodes.Status401Unauthorized)
            .ProducesProblem(statusCode: StatusCodes.Status403Forbidden)
            .ProducesProblem(statusCode: StatusCodes.Status404NotFound)
            .ProducesProblem(statusCode: StatusCodes.Status429TooManyRequests);
    }
}
