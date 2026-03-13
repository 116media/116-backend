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
/// Request model for updating article content.
/// </summary>
/// <param name="Headline">The short teaser text (100–300 characters).</param>
/// <param name="Body">The rich-text HTML body containing only Cloudinary image URLs.</param>
/// <param name="CoverImageUrl">Optional URL of the article's primary cover image.</param>
public record UpdateArticleRequest(string Headline, string Body, string? CoverImageUrl);

/// <summary>
/// Response model for successful article content update.
/// </summary>
/// <param name="Article">The updated article detail information.</param>
public record UpdateArticleResponse(ArticleDetailDto Article);

/// <summary>
/// Defines the admin update article content endpoint.
/// Handles saving article body, headline, and cover image (step 2 of the creation flow).
/// </summary>
public class UpdateArticleEndpointV1 : ICarterModule
{
    /// <summary>
    /// Configures the article update route within the API pipeline.
    /// Maps the <c>PUT /api/v1/admin/articles/{id}</c> endpoint.
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
                async (string id, UpdateArticleRequest request, IDispatcher dispatcher) =>
                {
                    var command = new UpdateArticleCommand(
                        Id: id,
                        Headline: request.Headline,
                        Body: request.Body,
                        CoverImageUrl: request.CoverImageUrl
                    );

                    UpdateArticleResult result = await dispatcher.Send(request: command);
                    return Results.Ok(new UpdateArticleResponse(Article: result.Article));
                }
            )
            .WithName(endpointName: UpdateArticleMetaField.UpdateArticle.Name)
            .WithSummary(summary: UpdateArticleMetaField.UpdateArticle.Summary)
            .WithDescription(description: UpdateArticleMetaField.UpdateArticle.Description)
            .WithAuthorization(AccountStatusPolicies.RequireActiveUser)
            .WithAuthorization(UserRolePolicies.RequireAdminOrSuperAdmin)
            .RequireRateLimiting(policyName: RateLimitPolicies.ContentBrowsing)
            .ProducesValidationProblem()
            .Produces<UpdateArticleResponse>(statusCode: StatusCodes.Status200OK)
            .ProducesProblem(statusCode: StatusCodes.Status400BadRequest)
            .ProducesProblem(statusCode: StatusCodes.Status401Unauthorized)
            .ProducesProblem(statusCode: StatusCodes.Status403Forbidden)
            .ProducesProblem(statusCode: StatusCodes.Status404NotFound)
            .ProducesProblem(statusCode: StatusCodes.Status429TooManyRequests);
    }
}
