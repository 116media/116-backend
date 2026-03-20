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

namespace _116.Content.Application.Interactions.UseCases.Public.Commands.UnbookmarkShortVideo.V1;

/// <summary>
/// Response model for a successful PublicUnbookmarkShortVideo operation.
/// </summary>
/// <param name="IsSuccess">Indicates if the operation was successful.</param>
public record PublicUnbookmarkShortVideoResponse(bool IsSuccess);

/// <summary>
/// Defines the unbookmark short video endpoint.
/// </summary>
public class PublicUnbookmarkShortVideoEndpointV1 : ICarterModule
{
    /// <inheritdoc />
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        RouteGroupBuilder group = app.MapApiVersionGroup(1)
            .MapGroup($"{ContentConstants.Public}/{InteractionsRouteConstants.Shorts}")
            .WithTags($"{ContentConstants.Public}::{InteractionsRouteConstants.Shorts}");

        group
            .MapDelete(
                $"/{{id}}/{InteractionsRouteConstants.Bookmarks}",
                async (string id, ClaimsPrincipal user, IClaimsProvider claimsProvider, IDispatcher dispatcher) =>
                {
                    Guid shortVideoId = Guid.Parse(id);
                    Guid userId = claimsProvider.GetUserIdFromClaims(user: user);

                    var command = new PublicUnbookmarkShortVideoCommand(ShortVideoId: shortVideoId, UserId: userId);
                    PublicUnbookmarkShortVideoResult result = await dispatcher.Send(request: command);

                    var response = new PublicUnbookmarkShortVideoResponse(IsSuccess: result.IsSuccess);
                    return Results.Ok(response);
                }
            )
            .WithName(endpointName: PublicUnbookmarkShortVideoMetaField.PublicUnbookmarkShortVideo.Name)
            .WithSummary(summary: PublicUnbookmarkShortVideoMetaField.PublicUnbookmarkShortVideo.Summary)
            .WithDescription(description: PublicUnbookmarkShortVideoMetaField.PublicUnbookmarkShortVideo.Description)
            .WithAuthorization(UserRolePolicies.RequireVisitorOnly)
            .RequireRateLimiting(policyName: RateLimitPolicies.ContentBrowsing)
            .Produces<PublicUnbookmarkShortVideoResponse>(statusCode: StatusCodes.Status200OK)
            .ProducesProblem(statusCode: StatusCodes.Status400BadRequest)
            .ProducesProblem(statusCode: StatusCodes.Status401Unauthorized)
            .ProducesProblem(statusCode: StatusCodes.Status404NotFound)
            .ProducesProblem(statusCode: StatusCodes.Status429TooManyRequests);
    }
}
