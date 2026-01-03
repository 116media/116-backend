using _116.Identity.Application.Session.Constants;
using _116.Identity.Application.Shared.Authorizations.Policies;
using _116.Identity.Domain.Constants;
using _116.Shared.Application.Extensions;
using _116.Shared.Contracts.Application.CQRS;

using Carter;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace _116.Identity.Application.Session.UseCases.Admin.Commands.CleanupExpiredSessions.V1;

/// <summary>
/// Response model for cleanup expired sessions.
/// </summary>
/// <param name="DeletedCount">Number of expired sessions that were deleted.</param>
public record AdminCleanupExpiredSessionsResponse(int DeletedCount);

/// <summary>
/// Defines the admin cleanup expired sessions endpoint.
/// Handles cleanup of expired sessions from the system.
/// </summary>
public class AdminCleanupExpiredSessionsEndpointV1 : ICarterModule
{
    /// <summary>
    /// Configures the admin cleanup expired sessions route within the API pipeline.
    /// Maps the <c>/api/v1/admin/sessions/cleanup</c> endpoint to handle cleanup requests.
    /// </summary>
    /// <param name="app">The route builder used to register API endpoints.</param>
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        RouteGroupBuilder group = app
            .MapApiVersionGroup(1)
            .MapGroup($"{IdentityConstants.Admin}/{SessionRouteConstants.Endpoint}")
            .WithTags($"{IdentityConstants.Admin}::{SessionRouteConstants.Endpoint}");

        group.MapPost(pattern: SessionRouteConstants.Cleanup, async (IDispatcher dispatcher) =>
            {
                var command = new AdminCleanupExpiredSessionsCommand();
                AdminCleanupExpiredSessionsResult result = await dispatcher.Send(request: command);

                var response = new AdminCleanupExpiredSessionsResponse(DeletedCount: result.DeletedCount);
                return Results.Ok(value: response);
            })
            .WithName(endpointName: AdminCleanupExpiredSessionsMetaField.AdminCleanupExpiredSessions.Name)
            .WithSummary(summary: AdminCleanupExpiredSessionsMetaField.AdminCleanupExpiredSessions.Summary)
            .WithDescription(description: AdminCleanupExpiredSessionsMetaField.AdminCleanupExpiredSessions.Description)
            .RequireAuthorization(UserRolePolicies.RequireSuperAdminOnly)
            .ProducesValidationProblem()
            .Produces<AdminCleanupExpiredSessionsResponse>()
            .ProducesProblem(statusCode: StatusCodes.Status401Unauthorized)
            .ProducesProblem(statusCode: StatusCodes.Status403Forbidden);
    }
}
