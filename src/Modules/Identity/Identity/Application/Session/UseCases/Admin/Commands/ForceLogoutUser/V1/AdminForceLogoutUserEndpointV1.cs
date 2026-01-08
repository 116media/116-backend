using _116.BuildingBlocks.Constants;
using _116.BuildingBlocks.Constants.RateLimit;
using _116.Identity.Application.Session.Constants;
using _116.Identity.Application.Shared.Authorizations.Policies;
using _116.Identity.Domain.Constants;
using _116.Shared.Application.Extensions;
using _116.Shared.Contracts.Application.CQRS;
using Carter;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace _116.Identity.Application.Session.UseCases.Admin.Commands.ForceLogoutUser.V1;

/// <summary>
/// Response model for force logout user.
/// </summary>
/// <param name="IsSuccess">Indicates whether the user was successfully logged out from all devices.</param>
public record AdminForceLogoutUserResponse(bool IsSuccess);

/// <summary>
/// Defines the admin force logout user endpoint.
/// Handles forcing a user to log out from all their sessions.
/// </summary>
public class AdminForceLogoutUserEndpointV1 : ICarterModule
{
    /// <summary>
    /// Configures the admin force logout user route within the API pipeline.
    /// Maps the <c>/api/v1/admin/sessions/force-logout/{id:guid}</c> endpoint to handle force logout requests.
    /// </summary>
    /// <param name="app">The route builder used to register API endpoints.</param>
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        RouteGroupBuilder group = app.MapApiVersionGroup(1)
            .MapGroup($"{IdentityConstants.Admin}/{SessionRouteConstants.Endpoint}")
            .WithTags($"{IdentityConstants.Admin}::/{SessionRouteConstants.Endpoint}");

        group
            .MapPost(
                $"{SessionRouteConstants.ForceLogout}/{{id}}",
                async (string id, IDispatcher dispatcher) =>
                {
                    var command = new AdminForceLogoutUserCommand(UserId: id);
                    AdminForceLogoutUserResult result = await dispatcher.Send(request: command);

                    var response = new AdminForceLogoutUserResponse(IsSuccess: result.IsSuccess);
                    return Results.Ok(value: response);
                }
            )
            .WithName(endpointName: AdminForceLogoutUserMetaField.AdminForceLogoutUser.Name)
            .WithSummary(summary: AdminForceLogoutUserMetaField.AdminForceLogoutUser.Summary)
            .WithDescription(description: AdminForceLogoutUserMetaField.AdminForceLogoutUser.Description)
            .RequireAuthorization(UserRolePolicies.RequireAdminOrSuperAdmin)
            .RequireRateLimiting(policyName: RateLimitPolicies.SessionManagement)
            .ProducesValidationProblem()
            .Produces<AdminForceLogoutUserResponse>()
            .ProducesProblem(statusCode: StatusCodes.Status400BadRequest)
            .ProducesProblem(statusCode: StatusCodes.Status401Unauthorized)
            .ProducesProblem(statusCode: StatusCodes.Status403Forbidden)
            .ProducesProblem(statusCode: StatusCodes.Status429TooManyRequests);
    }
}
