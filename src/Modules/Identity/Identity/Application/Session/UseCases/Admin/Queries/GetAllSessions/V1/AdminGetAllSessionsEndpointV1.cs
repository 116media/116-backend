using _116.BuildingBlocks.Constants.Authorization.Policies;
using _116.BuildingBlocks.Constants.RateLimit;
using _116.Identity.Application.Session.Constants;
using _116.Identity.Application.Shared.DTOs;
using _116.Identity.Domain.Constants;
using _116.Shared.Application.Extensions;
using _116.Shared.Application.Pagination;
using _116.Shared.Contracts.Application.CQRS;
using Carter;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace _116.Identity.Application.Session.UseCases.Admin.Queries.GetAllSessions.V1;

/// <summary>
/// Response model for retrieving all sessions.
/// </summary>
/// <param name="Sessions">Paginated result containing session DTOs and pagination metadata.</param>
public record AdminGetAllSessionsResponse(PaginatedResult<SessionDto> Sessions);

/// <summary>
/// Defines the admin get all the sessions' endpoint.
/// Handles retrieval of all sessions with pagination and filtering.
/// </summary>
public class AdminGetAllSessionsEndpointV1 : ICarterModule
{
    /// <summary>
    /// Configures the admin get all the sessions route within the API pipeline.
    /// Maps the <c>/api/v1/admin/sessions</c> endpoint to handle session retrieval requests.
    /// </summary>
    /// <param name="app">The route builder used to register API endpoints.</param>
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        RouteGroupBuilder group = app.MapApiVersionGroup(1)
            .MapGroup($"{IdentityConstants.Admin}/{SessionRouteConstants.Endpoint}")
            .WithTags($"{IdentityConstants.Admin}::{SessionRouteConstants.Endpoint}");

        group
            .MapGet(
                "/",
                async (
                    IDispatcher dispatcher,
                    int pageIndex,
                    int pageSize,
                    string? status = null,
                    string? userId = null,
                    string? ipAddress = null,
                    DateTime? fromDate = null,
                    DateTime? toDate = null
                ) =>
                {
                    var paginatedRequest = new PaginatedRequest(pageIndex, pageSize);

                    var query = new AdminGetAllSessionsQuery(
                        PaginatedRequest: paginatedRequest,
                        Status: status,
                        UserId: userId,
                        IpAddress: ipAddress,
                        FromDate: fromDate,
                        ToDate: toDate
                    );

                    AdminGetAllSessionsResult result = await dispatcher.Send(request: query);

                    var response = new AdminGetAllSessionsResponse(Sessions: result.Sessions);
                    return Results.Ok(value: response);
                }
            )
            .WithName(endpointName: AdminGetAllSessionsMetaField.AdminGetAllSessions.Name)
            .WithSummary(summary: AdminGetAllSessionsMetaField.AdminGetAllSessions.Summary)
            .WithDescription(description: AdminGetAllSessionsMetaField.AdminGetAllSessions.Description)
            .WithAuthorization(UserRolePolicies.RequireAdminOrSuperAdmin)
            .RequireRateLimiting(policyName: RateLimitPolicies.SessionManagement)
            .ProducesValidationProblem()
            .Produces<AdminGetAllSessionsResponse>()
            .ProducesProblem(statusCode: StatusCodes.Status400BadRequest)
            .ProducesProblem(statusCode: StatusCodes.Status401Unauthorized)
            .ProducesProblem(statusCode: StatusCodes.Status403Forbidden)
            .ProducesProblem(statusCode: StatusCodes.Status429TooManyRequests);
    }
}
