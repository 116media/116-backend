using System.Security.Claims;
using _116.BuildingBlocks.Constants.RateLimit;
using _116.Identity.Application.Auth.Constants;
using _116.Identity.Application.Shared.Authorizations.Policies;
using _116.Identity.Application.Shared.Repositories;
using _116.Identity.Domain.Constants;
using _116.Shared.Application.Extensions;
using _116.Shared.Contracts.Application.CQRS;
using Carter;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace _116.Identity.Application.Auth.UseCases.Admin.Commands.ChangePassword.V1;

/// <summary>
/// Request model for admin password change.
/// </summary>
/// <param name="OldPassword">The admin user's current password for verification.</param>
/// <param name="NewPassword">The new password to set for the admin user.</param>
public record AdminChangePasswordRequest(string OldPassword, string NewPassword);

/// <summary>
/// Response model for admin password change.
/// </summary>
/// <param name="IsSuccess">Indicates whether the password change was successful.</param>
public record AdminChangePasswordResponse(bool IsSuccess);

/// <summary>
/// Defines the password change endpoint for authenticated admin users.
/// Handles password change using current password verification.
/// </summary>
public class AdminChangePasswordEndpointV1 : ICarterModule
{
    /// <summary>
    /// Configures the admin password change route within the API pipeline.
    /// Maps the <c>/api/v1/admin/auth/change-password</c> endpoint to handle password change requests.
    /// </summary>
    /// <param name="app">The route builder used to register API endpoints.</param>
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        RouteGroupBuilder group = app.MapApiVersionGroup(1)
            .MapGroup($"{IdentityConstants.Admin}/{AuthRouteConstants.Endpoint}")
            .WithTags($"{IdentityConstants.Admin}::{IdentityConstants.SchemaName}");

        group
            .MapPatch(
                pattern: AuthRouteConstants.ChangePassword,
                async (
                    AdminChangePasswordRequest request,
                    ClaimsPrincipal user,
                    IAuthRepository authRepository,
                    IDispatcher dispatcher
                ) =>
                {
                    Guid userId = authRepository.GetUserIdFromClaims(user: user);

                    var command = new AdminChangePasswordCommand(
                        UserId: userId,
                        OldPassword: request.OldPassword,
                        NewPassword: request.NewPassword
                    );
                    AdminChangePasswordResult result = await dispatcher.Send(request: command);

                    var response = new AdminChangePasswordResponse(IsSuccess: result.IsSuccess);

                    return Results.Ok(value: response);
                }
            )
            .WithName(endpointName: AdminChangePasswordMetaField.ChangePassword.Name)
            .WithSummary(summary: AdminChangePasswordMetaField.ChangePassword.Summary)
            .WithDescription(description: AdminChangePasswordMetaField.ChangePassword.Description)
            .RequireAuthorization(UserRolePolicies.RequireAdminOrSuperAdmin)
            .RequireRateLimiting(policyName: RateLimitPolicies.PasswordManagement)
            .ProducesValidationProblem()
            .Produces<AdminChangePasswordResponse>()
            .ProducesProblem(statusCode: StatusCodes.Status400BadRequest)
            .ProducesProblem(statusCode: StatusCodes.Status401Unauthorized)
            .ProducesProblem(statusCode: StatusCodes.Status403Forbidden)
            .ProducesProblem(statusCode: StatusCodes.Status404NotFound)
            .ProducesProblem(statusCode: StatusCodes.Status429TooManyRequests)
            .ProducesProblem(statusCode: StatusCodes.Status409Conflict);
    }
}
