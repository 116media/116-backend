using System.Security.Claims;

using _116.Identity.Application.Shared.Authorizations.Policies;
using _116.Identity.Application.Shared.Constants;
using _116.Identity.Application.Shared.Repositories;
using _116.Identity.Domain.Constants;
using _116.Shared.Application.Extensions;
using _116.Shared.Contracts.Application.CQRS;

using Carter;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace _116.Identity.Application.Auth.Admin.UseCases.Commands.ChangePassword.V1;

/// <summary>
/// Request model for admin password change.
/// </summary>
/// <param name="OldPassword">The admin user's current password for verification.</param>
/// <param name="NewPassword">The new password to set for the admin user.</param>
public record AdminChangePasswordRequest(
    string OldPassword,
    string NewPassword
);
/// <summary>
/// Response model for admin password change.
/// </summary>
/// <param name="IsSuccess">Indicates whether the password change was successful.</param>
public record AdminChangePasswordResponse(
    bool IsSuccess
);
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
        RouteGroupBuilder group = app
            .MapApiVersionGroup(version: 1)
            .MapGroup($"{IdentityConstants.Admin}/{AuthRouteConstants.Endpoint}")
            .WithTags($"{IdentityConstants.Admin}::{IdentityConstants.SchemaName}");
        group.MapPatch(AuthRouteConstants.ChangePassword, async (
                AdminChangePasswordRequest request,
                ClaimsPrincipal user,
                IUserRepository userRepository,
                IDispatcher dispatcher
            ) =>
            {
                // Extract user ID from JWT token claims
                Guid userId = userRepository.GetUserIdFromClaims(user);
                // Send the command to change the password
                var command = new AdminChangePasswordCommand(
                    userId,
                    request.OldPassword,
                    request.NewPassword
                );
                AdminChangePasswordResult result = await dispatcher.Send(command);
                // Adapt the result to the response type
                var response = new AdminChangePasswordResponse(
                    result.IsSuccess
                );
                return Results.Ok(response);
            })
            .WithName(AdminChangePasswordMetaField.ChangePassword.Name)
            .WithSummary(AdminChangePasswordMetaField.ChangePassword.Summary)
            .WithDescription(AdminChangePasswordMetaField.ChangePassword.Description)
            .RequireAuthorization(UserRolePolicies.RequireAdminOrSuperAdmin)
            .ProducesValidationProblem()
            .Produces<AdminChangePasswordResponse>()
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status409Conflict);
    }
}
