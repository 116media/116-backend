using _116.BuildingBlocks.Constants.Authorization.Policies;
using _116.BuildingBlocks.Constants.RateLimit;
using _116.Identity.Domain.Constants;
using _116.Shared.Application.Extensions;
using _116.Shared.Contracts.Application.CQRS;
using Carter;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace _116.Identity.Application.User.UseCases.Admin.Commands.ActivateUser.V1;

/// <summary>
/// Response model for user activation.
/// </summary>
/// <param name="IsSuccess">Indicates whether the account is active after the call.</param>
public record AdminActivateUserResponse(bool IsSuccess);

/// <summary>
/// Defines the admin activate user endpoint.
/// Handles lifting a user account suspension.
/// </summary>
public class AdminActivateUserEndpointV1 : ICarterModule
{
    /// <summary>
    /// Configures the admin activate user route within the API pipeline.
    /// Maps the <c>/api/v1/admin/users/{id}/activate</c> endpoint (PATCH).
    /// </summary>
    /// <param name="app">The route builder used to register API endpoints.</param>
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        RouteGroupBuilder group = app.MapApiVersionGroup(1)
            .MapGroup($"{IdentityConstants.Admin}/users")
            .WithTags($"{IdentityConstants.Admin}::users");

        group
            .MapPatch(
                "{id}/activate",
                async (string id, IDispatcher dispatcher, CancellationToken cancellationToken) =>
                {
                    var command = new AdminActivateUserCommand(UserId: id);
                    AdminActivateUserResult result = await dispatcher.Send(
                        request: command,
                        cancellationToken: cancellationToken
                    );

                    var response = new AdminActivateUserResponse(IsSuccess: result.IsSuccess);
                    return Results.Ok(value: response);
                }
            )
            .WithName(endpointName: AdminActivateUserMetaField.ActivateUser.Name)
            .WithSummary(summary: AdminActivateUserMetaField.ActivateUser.Summary)
            .WithDescription(description: AdminActivateUserMetaField.ActivateUser.Description)
            .WithAuthorization(UserRolePolicies.RequireSuperAdminOnly)
            .RequireRateLimiting(policyName: RateLimitPolicies.SessionManagement)
            .ProducesValidationProblem()
            .Produces<AdminActivateUserResponse>()
            .ProducesProblem(statusCode: StatusCodes.Status400BadRequest)
            .ProducesProblem(statusCode: StatusCodes.Status401Unauthorized)
            .ProducesProblem(statusCode: StatusCodes.Status403Forbidden)
            .ProducesProblem(statusCode: StatusCodes.Status404NotFound)
            .ProducesProblem(statusCode: StatusCodes.Status429TooManyRequests);
    }
}
