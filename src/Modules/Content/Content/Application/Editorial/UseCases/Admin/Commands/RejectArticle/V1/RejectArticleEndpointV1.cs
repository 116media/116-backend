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

namespace _116.Content.Application.Editorial.UseCases.Admin.Commands.RejectArticle.V1;

/// <summary>
/// Request model for rejecting an article.
/// </summary>
/// <param name="Reason">The rejection reason visible to the editorial team.</param>
public record RejectArticleRequest(string Reason);

/// <summary>
/// Defines the admin reject article endpoint.
/// Handles transitioning an article from PendingReview to Rejected.
/// </summary>
public class RejectArticleEndpointV1 : ICarterModule
{
    /// <summary>
    /// Configures the article rejection route within the API pipeline.
    /// Maps the <c>PATCH /api/v1/admin/articles/{id}/reject</c> endpoint.
    /// </summary>
    /// <param name="app">The route builder used to register API endpoints.</param>
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        RouteGroupBuilder group = app.MapApiVersionGroup(1)
            .MapGroup($"{ContentConstants.Admin}/{EditorialRouteConstants.Articles}")
            .WithTags($"{ContentConstants.Admin}::{EditorialRouteConstants.Articles}");

        group
            .MapPatch(
                $"/{{id}}/{EditorialRouteConstants.Reject}",
                async (string id, RejectArticleRequest request, IDispatcher dispatcher) =>
                {
                    var command = new RejectArticleCommand(Id: id, Reason: request.Reason);
                    await dispatcher.Send(request: command);
                    return Results.NoContent();
                }
            )
            .WithName(endpointName: RejectArticleMetaField.RejectArticle.Name)
            .WithSummary(summary: RejectArticleMetaField.RejectArticle.Summary)
            .WithDescription(description: RejectArticleMetaField.RejectArticle.Description)
            .WithAuthorization(AccountStatusPolicies.RequireActiveUser)
            .WithAuthorization(UserRolePolicies.RequireSuperAdminOnly)
            .RequireRateLimiting(policyName: RateLimitPolicies.ContentBrowsing)
            .ProducesValidationProblem()
            .Produces(statusCode: StatusCodes.Status204NoContent)
            .ProducesProblem(statusCode: StatusCodes.Status400BadRequest)
            .ProducesProblem(statusCode: StatusCodes.Status401Unauthorized)
            .ProducesProblem(statusCode: StatusCodes.Status403Forbidden)
            .ProducesProblem(statusCode: StatusCodes.Status404NotFound)
            .ProducesProblem(statusCode: StatusCodes.Status429TooManyRequests);
    }
}
