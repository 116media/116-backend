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

namespace _116.Content.Application.Editorial.UseCases.Admin.Commands.SubmitArticle.V1;

/// <summary>
/// Response model for a successful SubmitArticle operation.
/// </summary>
/// <param name="IsSuccess">Indicates if the operation was successful.</param>
public record AdminSubmitArticleResponse(bool IsSuccess);

/// <summary>
/// Defines the admin submit article endpoint.
/// Handles transitioning an article from Draft to PendingPayment or PendingReview.
/// </summary>
public class AdminSubmitArticleEndpointV1 : ICarterModule
{
    /// <summary>
    /// Configures the article submit route within the API pipeline.
    /// Maps the <c>PATCH /api/v1/admin/articles/{id}/submit</c> endpoint.
    /// </summary>
    /// <param name="app">The route builder used to register API endpoints.</param>
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        RouteGroupBuilder group = app.MapApiVersionGroup(1)
            .MapGroup($"{ContentConstants.Admin}/{EditorialRouteConstants.Articles}")
            .WithTags($"{ContentConstants.Admin}::{EditorialRouteConstants.Articles}");

        group
            .MapPatch(
                $"/{{id}}/{EditorialRouteConstants.Submit}",
                async (string id, IDispatcher dispatcher) =>
                {
                    var command = new AdminSubmitArticleCommand(Id: id);
                    AdminSubmitArticleResult result = await dispatcher.Send(request: command);

                    var response = new AdminSubmitArticleResponse(IsSuccess: result.IsSuccess);
                    return Results.Ok(response);
                }
            )
            .WithName(endpointName: AdminSubmitArticleMetaField.AdminSubmitArticle.Name)
            .WithSummary(summary: AdminSubmitArticleMetaField.AdminSubmitArticle.Summary)
            .WithDescription(description: AdminSubmitArticleMetaField.AdminSubmitArticle.Description)
            .WithAuthorization(AccountStatusPolicies.RequireActiveUser)
            .WithAuthorization(UserRolePolicies.RequireSuperAdminOnly)
            .RequireRateLimiting(policyName: RateLimitPolicies.ContentBrowsing)
            .Produces<AdminSubmitArticleResponse>(statusCode: StatusCodes.Status200OK)
            .ProducesProblem(statusCode: StatusCodes.Status401Unauthorized)
            .ProducesProblem(statusCode: StatusCodes.Status403Forbidden)
            .ProducesProblem(statusCode: StatusCodes.Status404NotFound)
            .ProducesProblem(statusCode: StatusCodes.Status429TooManyRequests);
    }
}
