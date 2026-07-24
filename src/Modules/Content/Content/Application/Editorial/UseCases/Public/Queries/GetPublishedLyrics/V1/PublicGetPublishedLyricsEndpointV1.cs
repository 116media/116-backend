using System.Security.Claims;
using _116.BuildingBlocks.Constants.RateLimit;
using _116.Content.Application.Editorial.Constants;
using _116.Content.Application.Shared.DTOs;
using _116.Content.Domain.Constants;
using _116.Identity.Contracts.Application;
using _116.Shared.Application.Extensions;
using _116.Shared.Application.Pagination;
using _116.Shared.Contracts.Application.CQRS;
using Carter;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace _116.Content.Application.Editorial.UseCases.Public.Queries.GetPublishedLyrics.V1;

/// <summary>
/// Response model for listing published lyrics pages.
/// </summary>
/// <param name="Lyrics">Paginated result containing lyrics summary DTOs and pagination metadata.</param>
public record PublicGetPublishedLyricsResponse(PaginatedResult<LyricsSummaryDto> Lyrics);

/// <summary>
/// Defines the public get published lyrics endpoint.
/// Returns a paginated list of published lyrics pages with optional filters.
/// </summary>
public class PublicGetPublishedLyricsEndpointV1 : ICarterModule
{
    /// <summary>
    /// Configures the published lyrics retrieval route within the API pipeline.
    /// Maps the <c>GET /api/v1/public/lyrics</c> endpoint to handle lyrics listing requests.
    /// </summary>
    /// <param name="app">The route builder used to register API endpoints.</param>
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        RouteGroupBuilder group = app.MapApiVersionGroup(1)
            .MapGroup($"{ContentConstants.Public}/{EditorialRouteConstants.Lyrics}")
            .WithTags($"{ContentConstants.Public}::{EditorialRouteConstants.Lyrics}");

        group
            .MapGet(
                "/",
                async (
                    ClaimsPrincipal user,
                    IClaimsProvider claimsProvider,
                    IDispatcher dispatcher,
                    int pageIndex = 0,
                    int pageSize = 10,
                    string? search = null,
                    string? language = null,
                    Guid? categoryId = null,
                    string? sort = null
                ) =>
                {
                    Guid? userId = null;

                    if (user.Identity?.IsAuthenticated == true)
                    {
                        userId = claimsProvider.GetUserIdFromClaims(user: user);
                    }

                    var paginatedRequest = new PaginatedRequest(pageIndex, pageSize);

                    var query = new PublicGetPublishedLyricsQuery(
                        PaginatedRequest: paginatedRequest,
                        Search: search,
                        Language: language,
                        CategoryId: categoryId,
                        Sort: sort,
                        CurrentUserId: userId
                    );

                    PublicGetPublishedLyricsResult result = await dispatcher.Send(request: query);

                    var response = new PublicGetPublishedLyricsResponse(Lyrics: result.Lyrics);
                    return Results.Ok(response);
                }
            )
            .WithName(endpointName: PublicGetPublishedLyricsMetaField.GetPublishedLyrics.Name)
            .WithSummary(summary: PublicGetPublishedLyricsMetaField.GetPublishedLyrics.Summary)
            .WithDescription(description: PublicGetPublishedLyricsMetaField.GetPublishedLyrics.Description)
            .AllowAnonymous()
            .RequireRateLimiting(policyName: RateLimitPolicies.ContentBrowsing)
            .Produces<PublicGetPublishedLyricsResponse>(statusCode: StatusCodes.Status200OK)
            .ProducesProblem(statusCode: StatusCodes.Status429TooManyRequests);
    }
}
