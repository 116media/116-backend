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

namespace _116.Identity.Application.Auth.UseCases.Public.Commands.SetPassword.V1;

/// <summary>
/// Request model for the public user to set password.
/// </summary>
/// <param name="Password">The password to set for the user.</param>
public record PublicSetPasswordRequest(string Password);

/// <summary>
/// Response model for the public user to set password.
/// </summary>
/// <param name="IsSuccess">Indicates whether the password was set successfully.</param>
public record PublicSetPasswordResponse(bool IsSuccess);

/// <summary>
/// Defines the password set endpoint for authenticated users who used external authentication.
/// Handles password setting for Google/Facebook users to enable local authentication.
/// </summary>
public class PublicSetPasswordEndpointV1 : ICarterModule
{
    /// <summary>
    /// Configures the public password set route within the API pipeline.
    /// Maps the <c>/api/v1/public/auth/set-password</c> endpoint to handle password set requests.
    /// </summary>
    /// <param name="app">The route builder used to register API endpoints.</param>
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        RouteGroupBuilder group = app.MapApiVersionGroup(1)
            .MapGroup($"{IdentityConstants.Public}/{AuthRouteConstants.Endpoint}")
            .WithTags($"{IdentityConstants.Public}::{IdentityConstants.SchemaName}");

        group
            .MapPost(
                pattern: AuthRouteConstants.SetPassword,
                async (
                    PublicSetPasswordRequest request,
                    ClaimsPrincipal user,
                    IAuthRepository authRepository,
                    IDispatcher dispatcher
                ) =>
                {
                    Guid userId = authRepository.GetUserIdFromClaims(user: user);

                    var command = new PublicSetPasswordCommand(UserId: userId, Password: request.Password);
                    PublicSetPasswordResult result = await dispatcher.Send(request: command);

                    var response = new PublicSetPasswordResponse(IsSuccess: result.IsSuccess);

                    return Results.Ok(value: response);
                }
            )
            .WithName(endpointName: PublicSetPasswordMetaField.SetPassword.Name)
            .WithSummary(summary: PublicSetPasswordMetaField.SetPassword.Summary)
            .WithDescription(description: PublicSetPasswordMetaField.SetPassword.Description)
            .WithAuthorization(UserRolePolicies.RequireVisitorOnly)
            .RequireRateLimiting(policyName: RateLimitPolicies.PasswordManagement)
            .ProducesValidationProblem()
            .Produces<PublicSetPasswordResponse>()
            .ProducesProblem(statusCode: StatusCodes.Status400BadRequest)
            .ProducesProblem(statusCode: StatusCodes.Status401Unauthorized)
            .ProducesProblem(statusCode: StatusCodes.Status403Forbidden)
            .ProducesProblem(statusCode: StatusCodes.Status429TooManyRequests)
            .ProducesProblem(statusCode: StatusCodes.Status404NotFound);
    }
}
