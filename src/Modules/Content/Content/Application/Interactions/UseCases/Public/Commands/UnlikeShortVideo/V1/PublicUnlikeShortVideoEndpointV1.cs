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

namespace _116.Content.Application.Interactions.UseCases.Public.Commands.UnlikeShortVideo.V1;

/// <summary>
/// Response model for a successful PublicUnlikeShortVideo operation.
/// </summary>
/// <param name="IsSuccess">Indicates if the operation was successful.</param>
public record PublicUnlikeShortVideoResponse(bool IsSuccess);

/// <summary>
/// Defines the unlike short video endpoint.
/// </summary>
public class PublicUnlikeShortVideoEndpointV1 : ICarterModule
{
    /// <inheritdoc />
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        RouteGroupBuilder group = app.MapApiVersionGroup(1)
            .MapGroup($"{ContentConstants.Public}/{InteractionsRouteConstants.Shorts}")
            .WithTags($"{ContentConstants.Public}::{InteractionsRouteConstants.Shorts}");

        group
            .MapDelete(
                $"/{{id}}/{InteractionsRouteConstants.Likes}",
                async (string id, ClaimsPrincipal user, IClaimsProvider claimsProvider, IDispatcher dispatcher) =>
                {
                    Guid shortVideoId = Guid.Parse(id);
                    Guid userId = claimsProvider.GetUserIdFromClaims(user: user);

                    var command = new PublicUnlikeShortVideoCommand(ShortVideoId: shortVideoId, UserId: userId);
                    PublicUnlikeShortVideoResult result = await dispatcher.Send(request: command);

                    var response = new PublicUnlikeShortVideoResponse(IsSuccess: result.IsSuccess);
                    return Results.Ok(response);
                }
            )
            .WithName(endpointName: PublicUnlikeShortVideoMetaField.PublicUnlikeShortVideo.Name)
            .WithSummary(summary: PublicUnlikeShortVideoMetaField.PublicUnlikeShortVideo.Summary)
            .WithDescription(description: PublicUnlikeShortVideoMetaField.PublicUnlikeShortVideo.Description)
            .WithAuthorization(UserRolePolicies.RequireVisitorOnly)
            .RequireRateLimiting(policyName: RateLimitPolicies.ContentBrowsing)
            .Produces<PublicUnlikeShortVideoResponse>(statusCode: StatusCodes.Status200OK)
            .ProducesProblem(statusCode: StatusCodes.Status400BadRequest)
            .ProducesProblem(statusCode: StatusCodes.Status401Unauthorized)
            .ProducesProblem(statusCode: StatusCodes.Status404NotFound)
            .ProducesProblem(statusCode: StatusCodes.Status429TooManyRequests);
    }
}
