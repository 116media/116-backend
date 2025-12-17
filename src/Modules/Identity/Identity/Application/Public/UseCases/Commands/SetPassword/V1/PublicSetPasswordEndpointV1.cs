using _116.Shared.Contracts.Application.CQRS;
using _116.Auth.Application.Shared.Repositories;
using _116.Auth.Application.Shared.Constants;
using _116.Auth.Domain.Constants;
using _116.Shared.Application.Extensions;
using Carter;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using System.Security.Claims;
using _116.Auth.Application.Shared.Authorizations.Policies;

namespace _116.Auth.Application.Public.UseCases.Commands.SetPassword.V1;

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
        RouteGroupBuilder group = app
            .MapApiVersionGroup(version: 1)
            .MapGroup($"{AuthConstants.Public}/{AuthRouteConstants.Endpoint}")
            .WithTags($"{AuthConstants.Public}::{AuthConstants.SchemaName}");

        group.MapPost(AuthRouteConstants.SetPassword, async (
                PublicSetPasswordRequest request,
                ClaimsPrincipal user,
                IUserRepository userRepository,
                IDispatcher dispatcher
            ) =>
            {
                // Extract user ID from JWT token claims
                Guid userId = userRepository.GetUserIdFromClaims(user);

                // Send the command to set the password
                var command = new PublicSetPasswordCommand(userId, request.Password);

                PublicSetPasswordResult result = await dispatcher.Send(command);

                // Adapt the result to the response type
                var response = new PublicSetPasswordResponse(result.IsSuccess);

                return Results.Ok(response);
            })
            .WithName(PublicSetPasswordMetaField.SetPassword.Name)
            .WithSummary(PublicSetPasswordMetaField.SetPassword.Summary)
            .WithDescription(PublicSetPasswordMetaField.SetPassword.Description)
            .RequireAuthorization(UserRolePolicies.RequireVisitorOnly)
            .ProducesValidationProblem()
            .Produces<PublicSetPasswordResponse>()
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .ProducesProblem(StatusCodes.Status404NotFound);
    }
}
