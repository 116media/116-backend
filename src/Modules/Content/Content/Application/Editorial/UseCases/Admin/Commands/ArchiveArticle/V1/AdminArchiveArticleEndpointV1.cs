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

namespace _116.Content.Application.Editorial.UseCases.Admin.Commands.ArchiveArticle.V1;

/// <summary>
/// Response model for a successful ArchiveArticle operation.
/// </summary>
/// <param name="IsSuccess">Indicates if the operation was successful.</param>
public record AdminArchiveArticleResponse(bool IsSuccess);

/// <summary>
/// Defines the admin archive article endpoint.
/// Handles transitioning an article to Archived status.
/// </summary>
public class AdminArchiveArticleEndpointV1 : ICarterModule
{
    /// <summary>
    /// Configures the article archive route within the API pipeline.
    /// Maps the <c>PATCH /api/v1/admin/articles/{id}/archive</c> endpoint.
    /// </summary>
    /// <param name="app">The route builder used to register API endpoints.</param>
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        RouteGroupBuilder group = app.MapApiVersionGroup(1)
            .MapGroup($"{ContentConstants.Admin}/{EditorialRouteConstants.Articles}")
            .WithTags($"{ContentConstants.Admin}::{EditorialRouteConstants.Articles}");

        group
            .MapPatch(
                $"/{{id}}/{EditorialRouteConstants.Archive}",
                async (string id, IDispatcher dispatcher) =>
                {
                    var command = new AdminArchiveArticleCommand(Id: id);
                    AdminArchiveArticleResult result = await dispatcher.Send(request: command);

                    var response = new AdminArchiveArticleResponse(IsSuccess: result.IsSuccess);
                    return Results.Ok(response);
                }
            )
            .WithName(endpointName: AdminArchiveArticleMetaField.AdminArchiveArticle.Name)
            .WithSummary(summary: AdminArchiveArticleMetaField.AdminArchiveArticle.Summary)
            .WithDescription(description: AdminArchiveArticleMetaField.AdminArchiveArticle.Description)
            .WithAuthorization(AccountStatusPolicies.RequireActiveUser)
            .WithAuthorization(UserRolePolicies.RequireSuperAdminOnly)
            .RequireRateLimiting(policyName: RateLimitPolicies.ContentBrowsing)
            .Produces<AdminArchiveArticleResponse>(statusCode: StatusCodes.Status200OK)
            .ProducesProblem(statusCode: StatusCodes.Status401Unauthorized)
            .ProducesProblem(statusCode: StatusCodes.Status403Forbidden)
            .ProducesProblem(statusCode: StatusCodes.Status404NotFound)
            .ProducesProblem(statusCode: StatusCodes.Status429TooManyRequests);
    }
}
