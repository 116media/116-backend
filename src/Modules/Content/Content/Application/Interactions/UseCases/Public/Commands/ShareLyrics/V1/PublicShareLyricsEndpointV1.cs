using System.Security.Claims;
using _116.BuildingBlocks.Constants.RateLimit;
using _116.Content.Application.Interactions.Constants;
using _116.Content.Domain.Constants;
using _116.Content.Domain.ValueObjects;
using _116.Identity.Contracts.Application;
using _116.Shared.Application.Extensions;
using _116.Shared.Contracts.Application.CQRS;
using Carter;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace _116.Content.Application.Interactions.UseCases.Public.Commands.ShareLyrics.V1;

/// <summary>
/// Request body for the PublicShareLyrics operation. Optional — a missing body records no channel.
/// </summary>
/// <param name="ShareChannel">The channel the share targeted (e.g. facebook, x, whatsapp, clipboard, web-share).</param>
public record PublicShareLyricsRequest(string? ShareChannel);

/// <summary>
/// Response model for a successful PublicShareLyrics operation.
/// </summary>
/// <param name="IsSuccess">Indicates if the operation was successful.</param>
public record PublicShareLyricsResponse(bool IsSuccess);

/// <summary>
/// Defines the share lyrics endpoint.
/// </summary>
public class PublicShareLyricsEndpointV1 : ICarterModule
{
    /// <inheritdoc />
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        RouteGroupBuilder group = app.MapApiVersionGroup(1)
            .MapGroup($"{ContentConstants.Public}/{InteractionsRouteConstants.Lyrics}")
            .WithTags($"{ContentConstants.Public}::{InteractionsRouteConstants.Lyrics}");

        group
            .MapPost(
                $"/{{id}}/{InteractionsRouteConstants.Shares}",
                async (
                    string id,
                    PublicShareLyricsRequest? request,
                    ClaimsPrincipal user,
                    IClaimsProvider claimsProvider,
                    IDispatcher dispatcher
                ) =>
                {
                    Guid? userId = null;
                    Guid lyricsId = Guid.Parse(id);

                    if (user.Identity?.IsAuthenticated == true)
                    {
                        userId = claimsProvider.GetUserIdFromClaims(user: user);
                    }

                    var command = new PublicShareLyricsCommand(
                        LyricsId: lyricsId,
                        UserId: userId,
                        ShareChannel: ShareChannel.TryFrom(request?.ShareChannel)?.Value
                    );
                    PublicShareLyricsResult result = await dispatcher.Send(request: command);

                    var response = new PublicShareLyricsResponse(IsSuccess: result.IsSuccess);
                    return Results.Ok(response);
                }
            )
            .WithName(endpointName: PublicShareLyricsMetaField.ShareLyrics.Name)
            .WithSummary(summary: PublicShareLyricsMetaField.ShareLyrics.Summary)
            .WithDescription(description: PublicShareLyricsMetaField.ShareLyrics.Description)
            .AllowAnonymous()
            .RequireRateLimiting(policyName: RateLimitPolicies.ContentBrowsing)
            .Produces<PublicShareLyricsResponse>(statusCode: StatusCodes.Status200OK)
            .ProducesProblem(statusCode: StatusCodes.Status404NotFound)
            .ProducesProblem(statusCode: StatusCodes.Status429TooManyRequests);
    }
}
