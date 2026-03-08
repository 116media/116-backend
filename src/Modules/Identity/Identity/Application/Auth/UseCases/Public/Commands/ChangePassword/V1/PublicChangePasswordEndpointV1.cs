using System.Security.Claims;
using _116.BuildingBlocks.Constants.Authorization.Policies;
using _116.BuildingBlocks.Constants.RateLimit;
using _116.Identity.Application.Auth.Constants;
using _116.Identity.Application.Shared.Repositories;
using _116.Identity.Domain.Constants;
using _116.Shared.Application.Extensions;
using _116.Shared.Contracts.Application.CQRS;
using Carter;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace _116.Identity.Application.Auth.UseCases.Public.Commands.ChangePassword.V1;

/// <summary>
/// Request model for password change.
/// </summary>
/// <param name="OldPassword">The user's current password for verification.</param>
/// <param name="NewPassword">The new password to set for the user.</param>
public record PublicChangePasswordRequest(string OldPassword, string NewPassword);

/// <summary>
/// Response model for password change.
/// </summary>
/// <param name="IsSuccess">Indicates whether the password change was successful.</param>
public record PublicChangePasswordResponse(bool IsSuccess);

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
        RouteGroupBuilder group = app.MapApiVersionGroup(1)
            .MapGroup($"{IdentityConstants.Public}/{AuthRouteConstants.Endpoint}")
            .WithTags($"{IdentityConstants.Public}::{IdentityConstants.SchemaName}");

        group
            .MapPatch(
                pattern: AuthRouteConstants.ChangePassword,
                async (
                    PublicChangePasswordRequest request,
                    ClaimsPrincipal user,
                    IAuthRepository authRepository,
                    IDispatcher dispatcher
                ) =>
                {
                    Guid userId = authRepository.GetUserIdFromClaims(user: user);
                    Guid sessionId = authRepository.GetSessionIdFromClaims(user: user);

                    var command = new PublicChangePasswordCommand(
                        UserId: userId,
                        SessionId: sessionId,
                        OldPassword: request.OldPassword,
                        NewPassword: request.NewPassword
                    );
                    PublicChangePasswordResult result = await dispatcher.Send(request: command);

                    var response = new PublicChangePasswordResponse(IsSuccess: result.IsSuccess);

                    return Results.Ok(value: response);
                }
            )
            .WithName(endpointName: PublicChangePasswordMetaField.ChangePassword.Name)
            .WithSummary(summary: PublicChangePasswordMetaField.ChangePassword.Summary)
            .WithDescription(description: PublicChangePasswordMetaField.ChangePassword.Description)
            .WithAuthorization(UserRolePolicies.RequireVisitorOnly)
            .RequireRateLimiting(policyName: RateLimitPolicies.PasswordManagement)
            .ProducesValidationProblem()
            .Produces<PublicChangePasswordResponse>()
            .ProducesProblem(statusCode: StatusCodes.Status400BadRequest)
            .ProducesProblem(statusCode: StatusCodes.Status401Unauthorized)
            .ProducesProblem(statusCode: StatusCodes.Status403Forbidden)
            .ProducesProblem(statusCode: StatusCodes.Status404NotFound)
            .ProducesProblem(statusCode: StatusCodes.Status429TooManyRequests)
            .ProducesProblem(statusCode: StatusCodes.Status409Conflict);
    }
}
