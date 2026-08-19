using System.Security.Claims;
using _116.BuildingBlocks.Constants.Authorization.Policies;
using _116.BuildingBlocks.Constants.RateLimit;
using _116.Identity.Contracts.Application;
using _116.Mailer.Domain.Constants;
using _116.Shared.Application.Extensions;
using _116.Shared.Contracts.Application.CQRS;
using Carter;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace _116.Mailer.Application.Notifications.UseCases.Public.Queries.GetUnreadNotificationCount.V1;

/// <summary>
/// Response model for the unread notification count.
/// </summary>
/// <param name="Count">The number of unread notifications.</param>
public record PublicGetUnreadNotificationCountResponse(int Count);

/// <summary>
/// Defines the unread notification count endpoint for authenticated public users.
/// </summary>
public class PublicGetUnreadNotificationCountEndpointV1 : ICarterModule
{
    /// <summary>
    /// Configures the unread count route within the API pipeline.
    /// Maps the <c>GET /api/v1/public/notifications/unread-count</c> endpoint.
    /// </summary>
    /// <param name="app">The route builder used to register API endpoints.</param>
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        RouteGroupBuilder group = app.MapApiVersionGroup(1)
            .MapGroup($"{MailerConstants.Public}/{MailerConstants.NotificationsRoute}")
            .WithTags($"{MailerConstants.Public}::{MailerConstants.NotificationsRoute}");

        group
            .MapGet(
                pattern: "unread-count",
                async (ClaimsPrincipal user, IClaimsProvider claims, IDispatcher dispatcher) =>
                {
                    Guid userId = claims.GetUserIdFromClaims(user: user);

                    var query = new PublicGetUnreadNotificationCountQuery(UserId: userId);
                    PublicGetUnreadNotificationCountResult result = await dispatcher.Send(request: query);

                    var response = new PublicGetUnreadNotificationCountResponse(Count: result.Count);

                    return Results.Ok(value: response);
                }
            )
            .WithName(endpointName: PublicGetUnreadNotificationCountMetaField.GetUnreadNotificationCount.Name)
            .WithSummary(summary: PublicGetUnreadNotificationCountMetaField.GetUnreadNotificationCount.Summary)
            .WithDescription(
                description: PublicGetUnreadNotificationCountMetaField.GetUnreadNotificationCount.Description
            )
            .WithAuthorization(UserRolePolicies.RequireVisitorOnly)
            .RequireRateLimiting(policyName: RateLimitPolicies.ContentBrowsing)
            .Produces<PublicGetUnreadNotificationCountResponse>()
            .ProducesProblem(statusCode: StatusCodes.Status401Unauthorized)
            .ProducesProblem(statusCode: StatusCodes.Status403Forbidden)
            .ProducesProblem(statusCode: StatusCodes.Status429TooManyRequests);
    }
}
