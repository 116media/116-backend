using System.Security.Claims;
using _116.BuildingBlocks.Constants.RateLimit;
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

namespace _116.Content.Application.Editorial.UseCases.Public.Queries.GetPromotedArticles.V1;

/// <summary>
/// Response model for listing promoted articles.
/// </summary>
/// <param name="Articles">The list of promoted article summary DTOs.</param>
public record PublicGetPromotedArticlesResponse(IReadOnlyList<ArticleSummaryDto> Articles);

/// <summary>
/// Defines the public get promoted articles endpoint.
/// Returns the list of currently promoted published articles.
/// </summary>
public class PublicGetPromotedArticlesEndpointV1 : ICarterModule
{
    /// <summary>
    /// Configures the promoted articles retrieval route within the API pipeline.
    /// Maps the <c>GET /api/v1/public/articles/promoted</c> endpoint.
    /// </summary>
    /// <param name="app">The route builder used to register API endpoints.</param>
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        RouteGroupBuilder group = app.MapApiVersionGroup(1)
            .MapGroup($"{ContentConstants.Public}/{EditorialRouteConstants.Articles}")
            .WithTags($"{ContentConstants.Public}::{EditorialRouteConstants.Articles}");

        group
            .MapGet(
                $"/{EditorialRouteConstants.Promoted}",
                async (ClaimsPrincipal user, IClaimsProvider claimsProvider, IDispatcher dispatcher) =>
                {
                    Guid? userId = null;

                    if (user.Identity?.IsAuthenticated == true)
                    {
                        userId = claimsProvider.GetUserIdFromClaims(user: user);
                    }

                    var query = new PublicGetPromotedArticlesQuery(CurrentUserId: userId);
                    PublicGetPromotedArticlesResult result = await dispatcher.Send(request: query);

                    var response = new PublicGetPromotedArticlesResponse(Articles: result.Articles);
                    return Results.Ok(response);
                }
            )
            .WithName(endpointName: PublicGetPromotedArticlesMetaField.PublicGetPromotedArticles.Name)
            .WithSummary(summary: PublicGetPromotedArticlesMetaField.PublicGetPromotedArticles.Summary)
            .WithDescription(description: PublicGetPromotedArticlesMetaField.PublicGetPromotedArticles.Description)
            .AllowAnonymous()
            .RequireRateLimiting(policyName: RateLimitPolicies.ContentBrowsing)
            .Produces<PublicGetPromotedArticlesResponse>(statusCode: StatusCodes.Status200OK)
            .ProducesProblem(statusCode: StatusCodes.Status429TooManyRequests);
    }
}
