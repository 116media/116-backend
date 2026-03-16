using System.Security.Claims;
using _116.BuildingBlocks.Constants.Authorization.Policies;
using _116.BuildingBlocks.Constants.RateLimit;
using _116.BuildingBlocks.Utils;
using _116.Content.Application.Editorial.Constants;
using _116.Content.Application.Shared.DTOs;
using _116.Content.Domain.Constants;
using _116.Identity.Contracts.Application;
using _116.Shared.Application.Extensions;
using _116.Shared.Contracts.Application.CQRS;
using Carter;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace _116.Content.Application.Editorial.UseCases.Admin.Commands.CreateArticle.V1;

/// <summary>
/// Request model for creating an article draft.
/// </summary>
/// <param name="CategoryId">The category this article belongs to.</param>
/// <param name="Title">The article display title.</param>
/// <param name="Slug">The URL-safe slug for this article.</param>
/// <param name="CustomerId">The B2B customer who commissioned this article. Null for free content.</param>
/// <param name="OrderItemId">The order item this article fulfils. Null for free content.</param>
public record AdminCreateArticleRequest(
    Guid CategoryId,
    string Title,
    string Slug,
    Guid? CustomerId,
    Guid? OrderItemId
);

/// <summary>
/// Response model for successful article draft creation.
/// </summary>
/// <param name="Article">The created article detail information.</param>
public record AdminCreateArticleResponse(ArticleDetailDto Article);

/// <summary>
/// Defines the admin create article endpoint.
/// Handles creation of new article drafts (step 1 of the two-step article creation flow).
/// </summary>
public class AdminCreateArticleEndpointV1 : ICarterModule
{
    /// <summary>
    /// Configures the article creation route within the API pipeline.
    /// Maps the <c>POST /api/v1/admin/articles</c> endpoint to handle article creation requests.
    /// </summary>
    /// <param name="app">The route builder used to register API endpoints.</param>
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        RouteGroupBuilder group = app.MapApiVersionGroup(1)
            .MapGroup($"{ContentConstants.Admin}/{EditorialRouteConstants.Articles}")
            .WithTags($"{ContentConstants.Admin}::{EditorialRouteConstants.Articles}");

        group
            .MapPost(
                "/",
                async (
                    AdminCreateArticleRequest request,
                    ClaimsPrincipal user,
                    IClaimsProvider claimsProvider,
                    IDispatcher dispatcher,
                    HttpContext httpContext
                ) =>
                {
                    Guid authorId = claimsProvider.GetUserIdFromClaims(user: user);

                    var command = new AdminCreateArticleCommand(
                        CategoryId: request.CategoryId,
                        Title: request.Title,
                        Slug: request.Slug,
                        AuthorId: authorId,
                        CustomerId: request.CustomerId,
                        OrderItemId: request.OrderItemId
                    );

                    AdminCreateArticleResult result = await dispatcher.Send(request: command);

                    var response = new AdminCreateArticleResponse(Article: result.Article);
                    Guid articleId = response.Article.Id;

                    string path = $"{ContentConstants.Admin}/{EditorialRouteConstants.Articles}/{articleId}";
                    string locationUrl = ApiVersionUrl.Build(context: httpContext, path: path);

                    return Results.Created(uri: locationUrl, value: response);
                }
            )
            .WithName(endpointName: AdminCreateArticleMetaField.AdminCreateArticle.Name)
            .WithSummary(summary: AdminCreateArticleMetaField.AdminCreateArticle.Summary)
            .WithDescription(description: AdminCreateArticleMetaField.AdminCreateArticle.Description)
            .WithAuthorization(AccountStatusPolicies.RequireActiveUser)
            .WithAuthorization(UserRolePolicies.RequireSuperAdminOnly)
            .RequireRateLimiting(policyName: RateLimitPolicies.ContentBrowsing)
            .ProducesValidationProblem()
            .Produces<AdminCreateArticleResponse>(statusCode: StatusCodes.Status201Created)
            .ProducesProblem(statusCode: StatusCodes.Status400BadRequest)
            .ProducesProblem(statusCode: StatusCodes.Status401Unauthorized)
            .ProducesProblem(statusCode: StatusCodes.Status403Forbidden)
            .ProducesProblem(statusCode: StatusCodes.Status404NotFound)
            .ProducesProblem(statusCode: StatusCodes.Status409Conflict)
            .ProducesProblem(statusCode: StatusCodes.Status429TooManyRequests);
    }
}
