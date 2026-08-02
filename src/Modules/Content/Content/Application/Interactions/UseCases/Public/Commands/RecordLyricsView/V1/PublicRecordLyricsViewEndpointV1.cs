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

namespace _116.Content.Application.Interactions.UseCases.Public.Commands.RecordLyricsView.V1;

/// <summary>
/// Request body for the PublicRecordLyricsView operation, carrying the read-time signals
/// used by the read-time view-counting algorithm (spec 05).
/// </summary>
/// <param name="DwellMs">Total foreground dwell time on the lyrics page, in milliseconds.</param>
/// <param name="ScrollDepthRatio">Maximum scroll coverage reached, from 0.0 to 1.0.</param>
public record PublicRecordLyricsViewRequest(int DwellMs, double ScrollDepthRatio);

/// <summary>
/// Response model for a successful PublicRecordLyricsView operation.
/// </summary>
/// <param name="IsSuccess">Indicates if the operation was successful.</param>
/// <param name="IsCounted">Whether this view incremented the displayed count.</param>
public record PublicRecordLyricsViewResponse(bool IsSuccess, bool IsCounted);

/// <summary>
/// Defines the record lyrics view endpoint. Collects the caller's identity signals
/// (user id, X-Device-Id, IP, User-Agent) so views can be deduplicated per viewer, alongside
/// the reported dwell time and scroll depth consumed by the read-time view-counting algorithm.
/// </summary>
public class PublicRecordLyricsViewEndpointV1 : ICarterModule
{
    /// <summary>
    /// Maximum stored User-Agent length; longer values are truncated.
    /// </summary>
    private const int MaxUserAgentLength = 500;

    /// <inheritdoc />
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        RouteGroupBuilder group = app.MapApiVersionGroup(1)
            .MapGroup($"{ContentConstants.Public}/{InteractionsRouteConstants.Lyrics}")
            .WithTags($"{ContentConstants.Public}::{InteractionsRouteConstants.Lyrics}");

        group
            .MapPost(
                $"/{{id}}/{InteractionsRouteConstants.Views}",
                async (
                    string id,
                    PublicRecordLyricsViewRequest request,
                    HttpContext httpContext,
                    ClaimsPrincipal user,
                    IClaimsProvider claimsProvider,
                    IDispatcher dispatcher
                ) =>
                {
                    Guid lyricsId = Guid.Parse(id);
                    Guid? userId = null;

                    if (user.Identity?.IsAuthenticated == true)
                    {
                        userId = claimsProvider.GetUserIdFromClaims(user: user);
                    }

                    string? deviceId = httpContext.Request.Headers["X-Device-Id"].FirstOrDefault();
                    // UseForwardedHeaders already resolves the trusted client IP from
                    // X-Forwarded-For into RemoteIpAddress, so read it rather than re-parsing.
                    string? ipAddress = httpContext.Connection.RemoteIpAddress?.ToString();
                    string? userAgent = httpContext.Request.Headers.UserAgent.FirstOrDefault();

                    if (userAgent?.Length > MaxUserAgentLength)
                    {
                        userAgent = userAgent[..MaxUserAgentLength];
                    }

                    var command = new PublicRecordLyricsViewCommand(
                        LyricsId: lyricsId,
                        UserId: userId,
                        DeviceId: deviceId,
                        IpAddress: ipAddress,
                        UserAgent: userAgent,
                        DwellMs: request.DwellMs,
                        ScrollDepthRatio: request.ScrollDepthRatio
                    );

                    PublicRecordLyricsViewResult result = await dispatcher.Send(request: command);

                    var response = new PublicRecordLyricsViewResponse(
                        IsSuccess: result.IsSuccess,
                        IsCounted: result.IsCounted
                    );
                    return Results.Ok(response);
                }
            )
            .WithName(endpointName: PublicRecordLyricsViewMetaField.RecordLyricsView.Name)
            .WithSummary(summary: PublicRecordLyricsViewMetaField.RecordLyricsView.Summary)
            .WithDescription(description: PublicRecordLyricsViewMetaField.RecordLyricsView.Description)
            .AllowAnonymous()
            .RequireRateLimiting(policyName: RateLimitPolicies.ContentBrowsing)
            .Produces<PublicRecordLyricsViewResponse>(statusCode: StatusCodes.Status200OK)
            .ProducesProblem(statusCode: StatusCodes.Status400BadRequest)
            .ProducesProblem(statusCode: StatusCodes.Status404NotFound)
            .ProducesProblem(statusCode: StatusCodes.Status429TooManyRequests);
    }
}
