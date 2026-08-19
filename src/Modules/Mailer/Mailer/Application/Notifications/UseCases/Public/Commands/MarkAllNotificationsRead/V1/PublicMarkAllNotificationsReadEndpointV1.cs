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

namespace _116.Mailer.Application.Notifications.UseCases.Public.Commands.MarkAllNotificationsRead.V1;

/// <summary>
/// Response model for the mark-all-notifications-read use-case.
/// </summary>
/// <param name="MarkedCount">The number of notifications transitioned to read by this call.</param>
public record PublicMarkAllNotificationsReadResponse(int MarkedCount);

/// <summary>
/// Defines the mark-all-notifications-read endpoint for authenticated public users.
/// </summary>
public class PublicMarkAllNotificationsReadEndpointV1 : ICarterModule
{
    /// <summary>
    /// Configures the read-all route within the API pipeline.
    /// Maps the <c>PATCH /api/v1/public/notifications/read-all</c> endpoint.
    /// </summary>
    /// <param name="app">The route builder used to register API endpoints.</param>
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        RouteGroupBuilder group = app.MapApiVersionGroup(1)
            .MapGroup($"{MailerConstants.Public}/{MailerConstants.NotificationsRoute}")
            .WithTags($"{MailerConstants.Public}::{MailerConstants.NotificationsRoute}");

        group
            .MapPatch(
                pattern: "read-all",
                async (ClaimsPrincipal user, IClaimsProvider claims, IDispatcher dispatcher) =>
                {
                    Guid userId = claims.GetUserIdFromClaims(user: user);

                    var command = new PublicMarkAllNotificationsReadCommand(UserId: userId);
                    PublicMarkAllNotificationsReadResult result = await dispatcher.Send(request: command);

                    var response = new PublicMarkAllNotificationsReadResponse(MarkedCount: result.MarkedCount);

                    return Results.Ok(value: response);
                }
            )
            .WithName(endpointName: PublicMarkAllNotificationsReadMetaField.MarkAllNotificationsRead.Name)
            .WithSummary(summary: PublicMarkAllNotificationsReadMetaField.MarkAllNotificationsRead.Summary)
            .WithDescription(description: PublicMarkAllNotificationsReadMetaField.MarkAllNotificationsRead.Description)
            .WithAuthorization(UserRolePolicies.RequireVisitorOnly)
            .RequireRateLimiting(policyName: RateLimitPolicies.UserProfile)
            .Produces<PublicMarkAllNotificationsReadResponse>()
            .ProducesProblem(statusCode: StatusCodes.Status401Unauthorized)
            .ProducesProblem(statusCode: StatusCodes.Status403Forbidden)
            .ProducesProblem(statusCode: StatusCodes.Status429TooManyRequests);
    }
}
