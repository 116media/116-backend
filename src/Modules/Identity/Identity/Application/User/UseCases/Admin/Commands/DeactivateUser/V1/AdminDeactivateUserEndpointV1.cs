using _116.BuildingBlocks.Constants.Authorization.Policies;
using _116.BuildingBlocks.Constants.RateLimit;
using _116.Identity.Domain.Constants;
using _116.Shared.Application.Extensions;
using _116.Shared.Contracts.Application.CQRS;
using Carter;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace _116.Identity.Application.User.UseCases.Admin.Commands.DeactivateUser.V1;

/// <summary>
/// Response model for user deactivation.
/// </summary>
/// <param name="IsSuccess">Indicates whether the account is deactivated after the call.</param>
public record AdminDeactivateUserResponse(bool IsSuccess);

/// <summary>
/// Defines the admin deactivate user endpoint.
/// Handles suspending a user account and terminating its access.
/// </summary>
public class AdminDeactivateUserEndpointV1 : ICarterModule
{
    /// <summary>
    /// Configures the admin deactivate user route within the API pipeline.
    /// Maps the <c>/api/v1/admin/users/{id}/deactivate</c> endpoint (PATCH).
    /// </summary>
    /// <param name="app">The route builder used to register API endpoints.</param>
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        RouteGroupBuilder group = app.MapApiVersionGroup(1)
            .MapGroup($"{IdentityConstants.Admin}/users")
            .WithTags($"{IdentityConstants.Admin}::users");

        group
            .MapPatch(
                "{id}/deactivate",
                async (string id, IDispatcher dispatcher, CancellationToken cancellationToken) =>
                {
                    var command = new AdminDeactivateUserCommand(UserId: id);
                    AdminDeactivateUserResult result = await dispatcher.Send(
                        request: command,
                        cancellationToken: cancellationToken
                    );

                    var response = new AdminDeactivateUserResponse(IsSuccess: result.IsSuccess);
                    return Results.Ok(value: response);
                }
            )
            .WithName(endpointName: AdminDeactivateUserMetaField.DeactivateUser.Name)
            .WithSummary(summary: AdminDeactivateUserMetaField.DeactivateUser.Summary)
            .WithDescription(description: AdminDeactivateUserMetaField.DeactivateUser.Description)
            .WithAuthorization(UserRolePolicies.RequireSuperAdminOnly)
            .RequireRateLimiting(policyName: RateLimitPolicies.SessionManagement)
            .ProducesValidationProblem()
            .Produces<AdminDeactivateUserResponse>()
            .ProducesProblem(statusCode: StatusCodes.Status400BadRequest)
            .ProducesProblem(statusCode: StatusCodes.Status401Unauthorized)
            .ProducesProblem(statusCode: StatusCodes.Status403Forbidden)
            .ProducesProblem(statusCode: StatusCodes.Status404NotFound)
            .ProducesProblem(statusCode: StatusCodes.Status429TooManyRequests);
    }
}
