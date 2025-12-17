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

namespace _116.Identity.Application.Auth.Public.UseCases.Commands.ChangePassword.V1;

/// <summary>
/// Request model for password change.
/// </summary>
/// <param name="OldPassword">The user's current password for verification.</param>
/// <param name="NewPassword">The new password to set for the user.</param>
public record PublicChangePasswordRequest(
    string OldPassword,
    string NewPassword
);
/// <summary>
/// Response model for password change.
/// </summary>
/// <param name="IsSuccess">Indicates whether the password change was successful.</param>
public record PublicChangePasswordResponse(
    bool IsSuccess
);
/// <summary>
/// Defines the password change endpoint for authenticated public users.
/// Handles password change using current password verification.
/// </summary>
public class PublicChangePasswordEndpointV1 : ICarterModule
{
    /// <summary>
    /// Configures the password change route within the API pipeline.
    /// Maps the <c>/api/v1/public/auth/change-password</c> endpoint to handle password change requests.
    /// </summary>
    /// <param name="app">The route builder used to register API endpoints.</param>
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        RouteGroupBuilder group = app
            .MapApiVersionGroup(version: 1)
            .MapGroup($"{IdentityConstants.Public}/{AuthRouteConstants.Endpoint}")
            .WithTags($"{IdentityConstants.Public}::{IdentityConstants.SchemaName}");
        group.MapPatch(AuthRouteConstants.ChangePassword, async (
                PublicChangePasswordRequest request,
                ClaimsPrincipal user,
                IUserRepository userRepository,
                IDispatcher dispatcher
            ) =>
            {
                // Extract user ID from JWT token claims
                Guid userId = userRepository.GetUserIdFromClaims(user);
                // Send the command to change the password
                var command = new PublicChangePasswordCommand(
                    userId,
                    request.OldPassword,
                    request.NewPassword
                );
                PublicChangePasswordResult result = await dispatcher.Send(command);
                // Adapt the result to the response type
                var response = new PublicChangePasswordResponse(
                    result.IsSuccess
                );
                return Results.Ok(response);
            })
            .WithName(PublicChangePasswordMetaField.ChangePassword.Name)
            .WithSummary(PublicChangePasswordMetaField.ChangePassword.Summary)
            .WithDescription(PublicChangePasswordMetaField.ChangePassword.Description)
            .RequireAuthorization(UserRolePolicies.RequireVisitorOnly)
            .ProducesValidationProblem()
            .Produces<PublicChangePasswordResponse>()
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status409Conflict);
    }
}
