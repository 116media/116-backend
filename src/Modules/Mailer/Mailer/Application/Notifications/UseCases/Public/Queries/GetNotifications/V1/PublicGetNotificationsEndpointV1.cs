using System.Security.Claims;
using _116.BuildingBlocks.Constants.Authorization.Policies;
using _116.BuildingBlocks.Constants.RateLimit;
using _116.Identity.Contracts.Application;
using _116.Mailer.Application.Shared.DTOs;
using _116.Mailer.Domain.Constants;
using _116.Shared.Application.Extensions;
using _116.Shared.Application.Pagination;
using _116.Shared.Contracts.Application.CQRS;
using Carter;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace _116.Mailer.Application.Notifications.UseCases.Public.Queries.GetNotifications.V1;

/// <summary>
/// Response model for the notification feed.
/// </summary>
/// <param name="Notifications">The paginated notifications, newest first.</param>
public record PublicGetNotificationsResponse(PaginatedResult<NotificationDto> Notifications);

/// <summary>
/// Defines the notification feed endpoint for authenticated public users.
/// </summary>
public class PublicGetNotificationsEndpointV1 : ICarterModule
{
    /// <summary>
    /// Configures the notification feed route within the API pipeline.
    /// Maps the <c>GET /api/v1/public/notifications</c> endpoint.
    /// </summary>
    /// <param name="app">The route builder used to register API endpoints.</param>
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        RouteGroupBuilder group = app.MapApiVersionGroup(1)
            .MapGroup($"{MailerConstants.Public}/{MailerConstants.NotificationsRoute}")
            .WithTags($"{MailerConstants.Public}::{MailerConstants.NotificationsRoute}");

        group
            .MapGet(
                pattern: "/",
                async (
                    ClaimsPrincipal user,
                    IClaimsProvider claims,
                    IDispatcher dispatcher,
                    int pageIndex = 0,
                    int pageSize = 20,
                    bool unreadOnly = false
                ) =>
                {
                    Guid userId = claims.GetUserIdFromClaims(user: user);

                    var query = new PublicGetNotificationsQuery(
                        UserId: userId,
                        PageIndex: int.Max(pageIndex, 0),
                        PageSize: int.Clamp(pageSize, 1, 100),
                        UnreadOnly: unreadOnly
                    );

                    PublicGetNotificationsResult result = await dispatcher.Send(request: query);

                    var response = new PublicGetNotificationsResponse(Notifications: result.Notifications);

                    return Results.Ok(value: response);
                }
            )
            .WithName(endpointName: PublicGetNotificationsMetaField.GetNotifications.Name)
            .WithSummary(summary: PublicGetNotificationsMetaField.GetNotifications.Summary)
            .WithDescription(description: PublicGetNotificationsMetaField.GetNotifications.Description)
            .WithAuthorization(UserRolePolicies.RequireVisitorOnly)
            .RequireRateLimiting(policyName: RateLimitPolicies.ContentBrowsing)
            .Produces<PublicGetNotificationsResponse>()
            .ProducesProblem(statusCode: StatusCodes.Status401Unauthorized)
            .ProducesProblem(statusCode: StatusCodes.Status403Forbidden)
            .ProducesProblem(statusCode: StatusCodes.Status429TooManyRequests);
    }
}
