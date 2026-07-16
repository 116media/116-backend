using System.Security.Claims;
using _116.BuildingBlocks.Constants.Authorization.Policies;
using _116.BuildingBlocks.Constants.RateLimit;
using _116.Content.Application.Interactions.Constants;
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

namespace _116.Content.Application.Interactions.UseCases.Public.Queries.GetOwnRatedVideos.V1;

/// <summary>
/// Defines the current-user rated-video endpoint.
/// </summary>
public class PublicGetOwnRatedVideosEndpointV1 : ICarterModule
{
    /// <inheritdoc />
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        RouteGroupBuilder group = app.MapApiVersionGroup(1)
            .MapGroup($"{ContentConstants.Public}/{InteractionsRouteConstants.Videos}")
            .WithTags($"{ContentConstants.Public}::{InteractionsRouteConstants.Videos}");

        group
            .MapGet(
                $"/{InteractionsRouteConstants.Rated}",
                async (
                    ClaimsPrincipal user,
                    IClaimsProvider claimsProvider,
                    IDispatcher dispatcher,
                    int pageIndex = 0,
                    int pageSize = 10
                ) =>
                {
                    Guid userId = claimsProvider.GetUserIdFromClaims(user);
                    var query = new PublicGetOwnRatedVideosQuery(userId, new PaginatedRequest(pageIndex, pageSize));
                    PublicGetOwnRatedVideosResult result = await dispatcher.Send(query);
                    return Results.Ok(result.Videos);
                }
            )
            .WithName(PublicGetOwnRatedVideosMetaField.GetOwnRatedVideos.Name)
            .WithSummary(PublicGetOwnRatedVideosMetaField.GetOwnRatedVideos.Summary)
            .WithDescription(PublicGetOwnRatedVideosMetaField.GetOwnRatedVideos.Description)
            .WithAuthorization(UserRolePolicies.RequireVisitorOnly)
            .RequireRateLimiting(RateLimitPolicies.ContentBrowsing)
            .Produces<PaginatedResult<UserVideoActivityDto>>(statusCode: StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status429TooManyRequests);
    }
}
