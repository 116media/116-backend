using _116.BuildingBlocks.Constants;
using _116.Shared.Contracts.Application.CQRS;
using _116.User.Application.Shared.Authorizations.Policies;
using _116.User.Application.Shared.Repositories;
using _116.User.Domain.DTOs;
using Carter;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using System.Security.Claims;

namespace _116.User.Application.Admin.UseCases.Commands.UpdateAvatar;

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
/// Defines the update avatar endpoint for authenticated admin users.
/// This endpoint requires admin authentication - only logged-in admin users can update their avatar.
/// </summary>
public class AdminUpdateAvatarEndpoint : ICarterModule
{
    /// <summary>
    /// Configures the admin update avatar route within the API pipeline.
    /// Maps the <c>/api/v1/admin/profile/avatar</c> endpoint to handle admin avatar update requests.
    /// </summary>
    /// <param name="app">The route builder used to register API endpoints.</param>
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        RouteGroupBuilder group = app
            .MapGroup(RouteConstants.V1.Admin.Profile)
            .WithTags("Admin::profile");

        group.MapPatch("/avatar", async (
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
