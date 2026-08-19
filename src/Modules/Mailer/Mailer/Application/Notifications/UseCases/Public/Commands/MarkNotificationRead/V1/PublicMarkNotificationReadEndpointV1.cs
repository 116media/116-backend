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

namespace _116.Mailer.Application.Notifications.UseCases.Public.Commands.MarkNotificationRead.V1;

/// <summary>
/// Response model for the mark-notification-read use-case.
/// </summary>
/// <param name="IsRead">Whether the notification is read after the call.</param>
public record PublicMarkNotificationReadResponse(bool IsRead);

/// <summary>
/// Defines the mark-notification-read endpoint for authenticated public users.
/// </summary>
public class PublicMarkNotificationReadEndpointV1 : ICarterModule
{
    /// <summary>
    /// Configures the mark-read route within the API pipeline.
    /// Maps the <c>PATCH /api/v1/public/notifications/{id}/read</c> endpoint.
    /// </summary>
    /// <param name="app">The route builder used to register API endpoints.</param>
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        RouteGroupBuilder group = app.MapApiVersionGroup(1)
            .MapGroup($"{MailerConstants.Public}/{MailerConstants.NotificationsRoute}")
            .WithTags($"{MailerConstants.Public}::{MailerConstants.NotificationsRoute}");

        group
            .MapPatch(
                pattern: "{id:guid}/read",
                async (Guid id, ClaimsPrincipal user, IClaimsProvider claims, IDispatcher dispatcher) =>
                {
                    Guid userId = claims.GetUserIdFromClaims(user: user);

                    var command = new PublicMarkNotificationReadCommand(UserId: userId, NotificationId: id);
                    PublicMarkNotificationReadResult result = await dispatcher.Send(request: command);

                    var response = new PublicMarkNotificationReadResponse(IsRead: result.IsRead);

                    return Results.Ok(value: response);
                }
            )
            .WithName(endpointName: PublicMarkNotificationReadMetaField.MarkNotificationRead.Name)
            .WithSummary(summary: PublicMarkNotificationReadMetaField.MarkNotificationRead.Summary)
            .WithDescription(description: PublicMarkNotificationReadMetaField.MarkNotificationRead.Description)
            .WithAuthorization(UserRolePolicies.RequireVisitorOnly)
            .RequireRateLimiting(policyName: RateLimitPolicies.UserProfile)
            .ProducesValidationProblem()
            .Produces<PublicMarkNotificationReadResponse>()
            .ProducesProblem(statusCode: StatusCodes.Status401Unauthorized)
            .ProducesProblem(statusCode: StatusCodes.Status403Forbidden)
            .ProducesProblem(statusCode: StatusCodes.Status404NotFound)
            .ProducesProblem(statusCode: StatusCodes.Status429TooManyRequests);
    }
}
