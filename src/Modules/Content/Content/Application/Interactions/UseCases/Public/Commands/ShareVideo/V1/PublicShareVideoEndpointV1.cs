using System.Security.Claims;
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

namespace _116.Content.Application.Interactions.UseCases.Public.Commands.ShareVideo.V1;

/// <summary>
/// Response model for a successful PublicShareVideo operation.
/// </summary>
/// <param name="IsSuccess">Indicates if the operation was successful.</param>
public record PublicShareVideoResponse(bool IsSuccess);

/// <summary>
/// Defines the share video endpoint. Allows anonymous access.
/// </summary>
public class PublicShareVideoEndpointV1 : ICarterModule
{
    /// <inheritdoc />
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        RouteGroupBuilder group = app.MapApiVersionGroup(1)
            .MapGroup($"{ContentConstants.Public}/{InteractionsRouteConstants.Videos}")
            .WithTags($"{ContentConstants.Public}::{InteractionsRouteConstants.Videos}");

        group
            .MapPost(
                $"/{{id}}/{InteractionsRouteConstants.Shares}",
                async (string id, ClaimsPrincipal user, IClaimsProvider claimsProvider, IDispatcher dispatcher) =>
                {
                    Guid videoId = Guid.Parse(id);
                    Guid? userId = null;

                    if (user.Identity?.IsAuthenticated == true)
                    {
                        userId = claimsProvider.GetUserIdFromClaims(user: user);
                    }

                    var command = new PublicShareVideoCommand(VideoId: videoId, UserId: userId);

                    PublicShareVideoResult result = await dispatcher.Send(request: command);

                    var response = new PublicShareVideoResponse(IsSuccess: result.IsSuccess);
                    return Results.Ok(response);
                }
            )
            .WithName(endpointName: PublicShareVideoMetaField.PublicShareVideo.Name)
            .WithSummary(summary: PublicShareVideoMetaField.PublicShareVideo.Summary)
            .WithDescription(description: PublicShareVideoMetaField.PublicShareVideo.Description)
            .AllowAnonymous()
            .RequireRateLimiting(policyName: RateLimitPolicies.ContentBrowsing)
            .Produces<PublicShareVideoResponse>(statusCode: StatusCodes.Status200OK)
            .ProducesProblem(statusCode: StatusCodes.Status404NotFound)
            .ProducesProblem(statusCode: StatusCodes.Status429TooManyRequests);
    }
}
