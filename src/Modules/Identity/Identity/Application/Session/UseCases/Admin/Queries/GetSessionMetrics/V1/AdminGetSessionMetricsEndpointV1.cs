using _116.Identity.Application.Session.Constants;
using _116.Identity.Application.Shared.Authorizations.Policies;
using _116.Identity.Domain.Constants;
using _116.Shared.Application.Extensions;
using _116.Shared.Contracts.Application.CQRS;

using Carter;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace _116.Identity.Application.Session.UseCases.Admin.Queries.GetSessionMetrics.V1;

/// <summary>
/// Response model for session metrics.
/// </summary>
/// <param name="ClientPlatforms">Session counts grouped by client platform.</param>
/// <param name="DeviceTypes">Session counts grouped by device type.</param>
/// <param name="TotalActiveSessions">Total number of active sessions.</param>
/// <param name="TotalActiveUsers">Total number of unique active users.</param>
public record AdminGetSessionMetricsResponse(
    ClientPlatformMetrics ClientPlatforms,
    DeviceTypeMetrics DeviceTypes,
    int TotalActiveSessions,
    int TotalActiveUsers
);

/// <summary>
/// Defines the admin get session metrics endpoint.
/// Handles retrieval of session analytics and statistics.
/// </summary>
public class AdminGetSessionMetricsEndpointV1 : ICarterModule
{
    /// <summary>
    /// Configures the admin to get session metrics route within the API pipeline.
    /// Maps the <c>/api/v1/admin/sessions/metrics</c> endpoint to handle metrics retrieval requests.
    /// </summary>
    /// <param name="app">The route builder used to register API endpoints.</param>
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        RouteGroupBuilder group = app
            .MapApiVersionGroup(1)
            .MapGroup($"{IdentityConstants.Admin}/{SessionRouteConstants.Endpoint}")
            .WithTags($"{IdentityConstants.Admin}::{SessionRouteConstants.Endpoint}");

        group.MapGet(SessionRouteConstants.Metrics, async (IDispatcher dispatcher) =>
            {
                var query = new AdminGetSessionMetricsQuery();
                AdminGetSessionMetricsResult result = await dispatcher.Send(request: query);

                var response = new AdminGetSessionMetricsResponse(
                    DeviceTypes: result.DeviceTypes,
                    ClientPlatforms: result.ClientPlatforms,
                    TotalActiveUsers: result.TotalActiveUsers,
                    TotalActiveSessions: result.TotalActiveSessions
                );
                return Results.Ok(value: response);
            })
            .WithName(endpointName: AdminGetSessionMetricsMetaField.AdminGetSessionMetrics.Name)
            .WithSummary(summary: AdminGetSessionMetricsMetaField.AdminGetSessionMetrics.Summary)
            .WithDescription(description: AdminGetSessionMetricsMetaField.AdminGetSessionMetrics.Description)
            .RequireAuthorization(UserRolePolicies.RequireAdminOrSuperAdmin)
            .Produces<AdminGetSessionMetricsResponse>()
            .ProducesProblem(statusCode: StatusCodes.Status401Unauthorized)
            .ProducesProblem(statusCode: StatusCodes.Status403Forbidden);
    }
}
