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

namespace _116.Content.Application.Editorial.UseCases.Admin.Commands.ForceUnpromoteArticle.V1;

/// <summary>
/// Request model for force-unpromoting a promoted article.
/// </summary>
/// <param name="Reason">The reason for removing the promotion (e.g. government takedown request).</param>
public record AdminForceUnpromoteArticleRequest(string Reason);

/// <summary>
/// Response model for a successful force-unpromote operation.
/// </summary>
/// <param name="ArticleId">The unique identifier of the unpromoted article.</param>
/// <param name="UnpromotedAt">The UTC timestamp at which the article was unpromoted.</param>
public record AdminForceUnpromoteArticleResponse(Guid ArticleId, DateTimeOffset UnpromotedAt);

/// <summary>
/// Defines the admin force-unpromote article endpoint.
/// Allows a SuperAdmin to immediately remove an active paid promotion from an article,
/// recording the audit trail needed for a future pro-rata refund calculation.
/// </summary>
public class AdminForceUnpromoteArticleEndpointV1 : ICarterModule
{
    /// <summary>
    /// Configures the force-unpromote article route within the API pipeline.
    /// Maps the <c>PATCH /api/v1/admin/articles/{slug}/unpromote</c> endpoint.
    /// </summary>
    /// <param name="app">The route builder used to register API endpoints.</param>
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        RouteGroupBuilder group = app.MapApiVersionGroup(1)
            .MapGroup($"{ContentConstants.Admin}/{EditorialRouteConstants.Articles}")
            .WithTags($"{ContentConstants.Admin}::{EditorialRouteConstants.Articles}");

        group
            .MapPatch(
                $"/{{slug}}/{EditorialRouteConstants.Unpromote}",
                async (string slug, AdminForceUnpromoteArticleRequest request, IDispatcher dispatcher) =>
                {
                    var command = new AdminForceUnpromoteArticleCommand(Slug: slug, Reason: request.Reason);

                    AdminForceUnpromoteArticleResult result = await dispatcher.Send(request: command);

                    var response = new AdminForceUnpromoteArticleResponse(
                        ArticleId: result.ArticleId,
                        UnpromotedAt: result.UnpromotedAt
                    );

                    return Results.Ok(response);
                }
            )
            .WithName(endpointName: AdminForceUnpromoteArticleMetaField.ForceUnpromoteArticle.Name)
            .WithSummary(summary: AdminForceUnpromoteArticleMetaField.ForceUnpromoteArticle.Summary)
            .WithDescription(description: AdminForceUnpromoteArticleMetaField.ForceUnpromoteArticle.Description)
            .WithAuthorization(AccountStatusPolicies.RequireActiveUser)
            .WithAuthorization(UserRolePolicies.RequireSuperAdminOnly)
            .RequireRateLimiting(policyName: RateLimitPolicies.ContentBrowsing)
            .ProducesValidationProblem()
            .Produces<AdminForceUnpromoteArticleResponse>(statusCode: StatusCodes.Status200OK)
            .ProducesProblem(statusCode: StatusCodes.Status400BadRequest)
            .ProducesProblem(statusCode: StatusCodes.Status401Unauthorized)
            .ProducesProblem(statusCode: StatusCodes.Status403Forbidden)
            .ProducesProblem(statusCode: StatusCodes.Status404NotFound)
            .ProducesProblem(statusCode: StatusCodes.Status429TooManyRequests);
    }
}
