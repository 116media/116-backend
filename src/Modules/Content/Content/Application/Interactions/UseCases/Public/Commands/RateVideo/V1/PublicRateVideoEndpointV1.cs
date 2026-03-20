using System.Security.Claims;
using _116.BuildingBlocks.Constants.Authorization.Policies;
using _116.BuildingBlocks.Constants.RateLimit;
using _116.Content.Application.Interactions.Constants;
using _116.Content.Domain.Constants;
using _116.Identity.Contracts.Application;
using _116.Shared.Application.Extensions;
using _116.Shared.Contracts.Application.CQRS;
using Carter;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace _116.Content.Application.Interactions.UseCases.Public.Commands.RateVideo.V1;

/// <summary>
/// Request body for rating a video.
/// </summary>
/// <param name="Stars">The star rating (1–5).</param>
public record PublicRateVideoRequest(short Stars);

/// <summary>
/// Response model for a successful PublicRateVideo operation.
/// </summary>
/// <param name="IsSuccess">Indicates if the operation was successful.</param>
public record PublicRateVideoResponse(bool IsSuccess);

/// <summary>
/// Defines the rate video endpoint.
/// </summary>
public class PublicRateVideoEndpointV1 : ICarterModule
{
    /// <inheritdoc />
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        RouteGroupBuilder group = app.MapApiVersionGroup(1)
            .MapGroup($"{ContentConstants.Public}/{InteractionsRouteConstants.Videos}")
            .WithTags($"{ContentConstants.Public}::{InteractionsRouteConstants.Videos}");

        group
            .MapPost(
                $"/{{id}}/{InteractionsRouteConstants.Ratings}",
                async (
                    string id,
                    PublicRateVideoRequest request,
                    ClaimsPrincipal user,
                    IClaimsProvider claimsProvider,
                    IDispatcher dispatcher
                ) =>
                {
                    Guid videoId = Guid.Parse(id);
                    Guid userId = claimsProvider.GetUserIdFromClaims(user: user);

                    var command = new PublicRateVideoCommand(VideoId: videoId, UserId: userId, Stars: request.Stars);
                    PublicRateVideoResult result = await dispatcher.Send(request: command);

                    var response = new PublicRateVideoResponse(IsSuccess: result.IsSuccess);
                    return Results.Ok(response);
                }
            )
            .WithName(endpointName: PublicRateVideoMetaField.PublicRateVideo.Name)
            .WithSummary(summary: PublicRateVideoMetaField.PublicRateVideo.Summary)
            .WithDescription(description: PublicRateVideoMetaField.PublicRateVideo.Description)
            .WithAuthorization(UserRolePolicies.RequireVisitorOnly)
            .RequireRateLimiting(policyName: RateLimitPolicies.ContentBrowsing)
            .Produces<PublicRateVideoResponse>(statusCode: StatusCodes.Status200OK)
            .ProducesProblem(statusCode: StatusCodes.Status400BadRequest)
            .ProducesProblem(statusCode: StatusCodes.Status401Unauthorized)
            .ProducesProblem(statusCode: StatusCodes.Status404NotFound)
            .ProducesProblem(statusCode: StatusCodes.Status429TooManyRequests);
    }
}
