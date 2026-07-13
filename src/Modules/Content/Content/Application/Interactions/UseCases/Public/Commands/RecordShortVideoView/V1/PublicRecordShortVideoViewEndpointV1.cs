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

namespace _116.Content.Application.Interactions.UseCases.Public.Commands.RecordShortVideoView.V1;

/// <summary>
/// Response model for a successful PublicRecordShortVideoView operation.
/// </summary>
/// <param name="IsSuccess">Indicates if the operation was successful.</param>
/// <param name="IsCounted">Whether this view incremented the displayed count.</param>
public record PublicRecordShortVideoViewResponse(bool IsSuccess, bool IsCounted);

/// <summary>
/// Defines the record short video view endpoint. Collects the caller's identity signals
/// (user id, X-Device-Id, IP, User-Agent) so views can be deduplicated per viewer.
/// </summary>
public class PublicRecordShortVideoViewEndpointV1 : ICarterModule
{
    /// <summary>
    /// Maximum stored User-Agent length; longer values are truncated.
    /// </summary>
    private const int MaxUserAgentLength = 500;

    /// <inheritdoc />
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        RouteGroupBuilder group = app.MapApiVersionGroup(1)
            .MapGroup($"{ContentConstants.Public}/{InteractionsRouteConstants.Shorts}")
            .WithTags($"{ContentConstants.Public}::{InteractionsRouteConstants.Shorts}");

        group
            .MapPost(
                $"/{{id}}/{InteractionsRouteConstants.Views}",
                async (
                    string id,
                    HttpContext httpContext,
                    ClaimsPrincipal user,
                    IClaimsProvider claimsProvider,
                    IDispatcher dispatcher
                ) =>
                {
                    Guid shortVideoId = Guid.Parse(id);
                    Guid? userId = null;

                    if (user.Identity?.IsAuthenticated == true)
                    {
                        userId = claimsProvider.GetUserIdFromClaims(user: user);
                    }

                    string? deviceId = httpContext.Request.Headers["X-Device-Id"].FirstOrDefault();
                    string? ipAddress = ResolveClientIp(httpContext: httpContext);
                    string? userAgent = httpContext.Request.Headers.UserAgent.FirstOrDefault();

                    if (userAgent?.Length > MaxUserAgentLength)
                    {
                        userAgent = userAgent[..MaxUserAgentLength];
                    }

                    var command = new PublicRecordShortVideoViewCommand(
                        ShortVideoId: shortVideoId,
                        UserId: userId,
                        DeviceId: deviceId,
                        IpAddress: ipAddress,
                        UserAgent: userAgent
                    );

                    PublicRecordShortVideoViewResult result = await dispatcher.Send(request: command);

                    var response = new PublicRecordShortVideoViewResponse(
                        IsSuccess: result.IsSuccess,
                        IsCounted: result.IsCounted
                    );
                    return Results.Ok(response);
                }
            )
            .WithName(endpointName: PublicRecordShortVideoViewMetaField.RecordShortVideoView.Name)
            .WithSummary(summary: PublicRecordShortVideoViewMetaField.RecordShortVideoView.Summary)
            .WithDescription(description: PublicRecordShortVideoViewMetaField.RecordShortVideoView.Description)
            .AllowAnonymous()
            .RequireRateLimiting(policyName: RateLimitPolicies.ContentBrowsing)
            .Produces<PublicRecordShortVideoViewResponse>(statusCode: StatusCodes.Status200OK)
            .ProducesProblem(statusCode: StatusCodes.Status404NotFound)
            .ProducesProblem(statusCode: StatusCodes.Status429TooManyRequests);
    }

    /// <summary>
    /// Resolves the client IP, preferring the first X-Forwarded-For hop when the API sits
    /// behind a reverse proxy, else the direct connection address.
    /// </summary>
    private static string? ResolveClientIp(HttpContext httpContext)
    {
        string? forwarded = httpContext.Request.Headers["X-Forwarded-For"].FirstOrDefault();

        if (!string.IsNullOrWhiteSpace(forwarded))
        {
            return forwarded.Split(',')[0].Trim();
        }

        return httpContext.Connection.RemoteIpAddress?.ToString();
    }
}
