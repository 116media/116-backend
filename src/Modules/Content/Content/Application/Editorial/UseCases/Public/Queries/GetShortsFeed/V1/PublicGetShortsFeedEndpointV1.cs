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

namespace _116.Content.Application.Editorial.UseCases.Public.Queries.GetShortsFeed.V1;

/// <summary>
/// Response model for the public randomized short videos feed.
/// </summary>
/// <param name="Items">The ordered short videos for this page.</param>
/// <param name="NextCursor">The cursor for the next page, or null when the feed is exhausted.</param>
public record PublicGetShortsFeedResponse(IReadOnlyList<ShortVideoDto> Items, string? NextCursor);

/// <summary>
/// Defines the public randomized short videos feed endpoint.
/// Returns a cursor-paginated, seeded pseudo-random feed of active short videos.
/// </summary>
public class PublicGetShortsFeedEndpointV1 : ICarterModule
{
    private const int DefaultPageSize = 10;
    private const int MaxPageSize = 20;

    /// <summary>
    /// Configures the public shorts feed route within the API pipeline.
    /// Maps the <c>GET /api/v1/public/shorts/feed</c> endpoint.
    /// </summary>
    /// <param name="app">The route builder used to register API endpoints.</param>
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        RouteGroupBuilder group = app.MapApiVersionGroup(1)
            .MapGroup($"{ContentConstants.Public}/{EditorialRouteConstants.Shorts}")
            .WithTags($"{ContentConstants.Public}::{EditorialRouteConstants.Shorts}");

        group
            .MapGet(
                $"/{EditorialRouteConstants.Feed}",
                async (
                    ClaimsPrincipal user,
                    IClaimsProvider claimsProvider,
                    IDispatcher dispatcher,
                    string? cursor = null,
                    int pageSize = DefaultPageSize
                ) =>
                {
                    Guid? userId = null;

                    if (user.Identity?.IsAuthenticated == true)
                    {
                        userId = claimsProvider.GetUserIdFromClaims(user: user);
                    }

                    int safePageSize = Math.Clamp(pageSize, 1, MaxPageSize);

                    var query = new PublicGetShortsFeedQuery(
                        Cursor: cursor,
                        PageSize: safePageSize,
                        CurrentUserId: userId
                    );

                    PublicGetShortsFeedResult result = await dispatcher.Send(request: query);

                    var response = new PublicGetShortsFeedResponse(Items: result.Items, NextCursor: result.NextCursor);
                    return Results.Ok(response);
                }
            )
            .WithName(endpointName: PublicGetShortsFeedMetaField.GetShortsFeed.Name)
            .WithSummary(summary: PublicGetShortsFeedMetaField.GetShortsFeed.Summary)
            .WithDescription(description: PublicGetShortsFeedMetaField.GetShortsFeed.Description)
            .AllowAnonymous()
            .RequireRateLimiting(policyName: RateLimitPolicies.ContentBrowsing)
            .Produces<PublicGetShortsFeedResponse>(statusCode: StatusCodes.Status200OK)
            .ProducesProblem(statusCode: StatusCodes.Status429TooManyRequests);
    }
}
