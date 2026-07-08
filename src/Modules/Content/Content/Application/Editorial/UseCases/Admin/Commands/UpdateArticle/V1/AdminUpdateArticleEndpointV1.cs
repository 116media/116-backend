using _116.BuildingBlocks.Constants.Authorization.Policies;
using _116.BuildingBlocks.Constants.RateLimit;
using _116.Content.Application.Editorial.Constants;
using _116.Content.Application.Shared.DTOs;
using _116.Content.Domain.Constants;
using _116.Shared.Application.Extensions;
using _116.Shared.Contracts.Application.CQRS;
using Carter;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace _116.Content.Application.Editorial.UseCases.Admin.Commands.UpdateArticle.V1;

/// <summary>
/// Request model for a full article update.
/// </summary>
/// <param name="CategoryId">The category this article belongs to.</param>
/// <param name="Title">The article title.</param>
/// <param name="Slug">The URL-safe slug. Must be unique across all articles.</param>
/// <param name="Headline">The short teaser text (100–300 characters).</param>
/// <param name="Body">The rich-text HTML body containing only Cloudinary image URLs.</param>
/// <param name="CustomerId">Optional B2B customer who commissioned this article.</param>
/// <param name="OrderItemId">Optional order item this article fulfils.</param>
/// <param name="SocialBoost">Whether the article is flagged for social media promotion.</param>
/// <param name="MetaTitle">Optional SEO meta title (max 70 chars).</param>
/// <param name="MetaDescription">Optional SEO meta description (max 160 chars).</param>
public record AdminUpdateArticleRequest(
    Guid CategoryId,
    string Title,
    string Slug,
    string Headline,
    string Body,
    Guid? CustomerId,
    Guid? OrderItemId,
    bool SocialBoost,
    string? MetaTitle,
    string? MetaDescription
);

/// <summary>
/// Response model for a successful article update.
/// </summary>
/// <param name="Article">The updated article detail information.</param>
public record AdminUpdateArticleResponse(ArticleDetailDto Article);

/// <summary>
/// Defines the admin update article endpoint.
/// Maps <c>PUT /api/v1/admin/articles/{id}</c>.
/// Allowed when the article status is <c>Draft</c>, <c>PendingPayment</c>,
/// <c>PendingReview</c>, or <c>Rejected</c>.
/// </summary>
public class AdminUpdateArticleEndpointV1 : ICarterModule
{
    /// <summary>
    /// Configures the article update route within the API pipeline.
    /// </summary>
    /// <param name="app">The route builder used to register API endpoints.</param>
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        RouteGroupBuilder group = app.MapApiVersionGroup(1)
            .MapGroup($"{ContentConstants.Admin}/{EditorialRouteConstants.Articles}")
            .WithTags($"{ContentConstants.Admin}::{EditorialRouteConstants.Articles}");

        group
            .MapPut(
                "/{id}",
                async (string id, AdminUpdateArticleRequest request, IDispatcher dispatcher) =>
                {
                    var command = new AdminUpdateArticleCommand(
                        Id: id,
                        CategoryId: request.CategoryId,
                        Title: request.Title,
                        Slug: request.Slug,
                        Headline: request.Headline,
                        Body: request.Body,
                        CustomerId: request.CustomerId,
                        OrderItemId: request.OrderItemId,
                        SocialBoost: request.SocialBoost,
                        MetaTitle: request.MetaTitle,
                        MetaDescription: request.MetaDescription
                    );

                    AdminUpdateArticleResult result = await dispatcher.Send(request: command);

                    var response = new AdminUpdateArticleResponse(Article: result.Article);
                    return Results.Ok(response);
                }
            )
            .WithName(endpointName: AdminUpdateArticleMetaField.UpdateArticle.Name)
            .WithSummary(summary: AdminUpdateArticleMetaField.UpdateArticle.Summary)
            .WithDescription(description: AdminUpdateArticleMetaField.UpdateArticle.Description)
            .WithAuthorization(AccountStatusPolicies.RequireActiveUser)
            .WithAuthorization(UserRolePolicies.RequireAdminOrSuperAdmin)
            .RequireRateLimiting(policyName: RateLimitPolicies.ContentBrowsing)
            .ProducesValidationProblem()
            .Produces<AdminUpdateArticleResponse>(statusCode: StatusCodes.Status200OK)
            .ProducesProblem(statusCode: StatusCodes.Status400BadRequest)
            .ProducesProblem(statusCode: StatusCodes.Status401Unauthorized)
            .ProducesProblem(statusCode: StatusCodes.Status403Forbidden)
            .ProducesProblem(statusCode: StatusCodes.Status404NotFound)
            .ProducesProblem(statusCode: StatusCodes.Status409Conflict)
            .ProducesProblem(statusCode: StatusCodes.Status429TooManyRequests);
    }
}
