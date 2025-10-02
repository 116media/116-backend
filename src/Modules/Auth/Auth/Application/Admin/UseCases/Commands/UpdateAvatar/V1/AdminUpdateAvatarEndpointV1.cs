using _116.Auth.Application.Shared.Authorizations.Policies;
using _116.Auth.Application.Shared.Repositories;
using _116.Auth.Domain.Constants;
using _116.Auth.Domain.DTOs;
using _116.Shared.Application.Extensions;
using _116.Shared.Contracts.Application.CQRS;
using Carter;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using System.Security.Claims;
using _116.Auth.Application.Shared.Constants;

namespace _116.Auth.Application.Admin.UseCases.Commands.UpdateAvatar.V1;

/// <summary>
/// Request model for updating admin user avatar.
/// This endpoint requires admin authentication - only logged-in admin users can update their avatar.
/// </summary>
/// <param name="AvatarUrl">The new avatar URL to set for the admin user.</param>
public record AdminUpdateAvatarRequest(
    string AvatarUrl
);

/// <summary>
/// Response model for updating admin user avatar.
/// </summary>
/// <param name="User">The updated admin user information with the new avatar.</param>
public record AdminUpdateAvatarResponse(
    UserResponseDto User
);

/// <summary>
/// Defines the update avatar endpoint for authenticated admin users (V1).
/// This endpoint requires admin authentication - only logged-in admin users can update their avatar.
/// </summary>
public class AdminUpdateAvatarEndpointV1 : ICarterModule
{
    /// <summary>
    /// Configures the admin update avatar route within the API pipeline.
    /// Maps the <c>/api/v1/admin/profile/avatar</c> endpoint to handle admin avatar update requests.
    /// </summary>
    /// <param name="app">The route builder used to register API endpoints.</param>
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        RouteGroupBuilder group = app
            .MapApiVersionGroup(version: 1)
            .MapGroup($"{AuthConstants.Admin}/{AuthRouteConstants.Profile}")
            .WithTags($"{AuthConstants.Admin}::{AuthRouteConstants.Profile}");

        group.MapPatch(AuthRouteConstants.Avatar, async (
                AdminUpdateAvatarRequest request,
                ClaimsPrincipal user,
                IUserRepository userRepository,
                IDispatcher dispatcher
            ) =>
            {
                // Extract user ID from JWT token claims
                Guid userId = userRepository.GetUserIdFromClaims(user);

                // Send the command to update the avatar
                var command = new AdminUpdateAvatarCommand(userId, request.AvatarUrl);
                AdminUpdateAvatarResult result = await dispatcher.Send(command);

                // Adapt the result to the response type
                var response = new AdminUpdateAvatarResponse(result.User);

                return Results.Ok(response);
            })
            .WithName(AdminUpdateAvatarMetaField.UpdateAvatar.Name)
            .WithSummary(AdminUpdateAvatarMetaField.UpdateAvatar.Summary)
            .WithDescription(AdminUpdateAvatarMetaField.UpdateAvatar.Description)
            .RequireAuthorization(UserRolePolicies.RequireAdminOrSuperAdmin)
            .ProducesValidationProblem()
            .Produces<AdminUpdateAvatarResponse>()
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .ProducesProblem(StatusCodes.Status404NotFound);
    }
}
