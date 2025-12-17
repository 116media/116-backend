using _116.Identity.Application.Shared.Authorizations.Policies;
using _116.Identity.Application.Shared.Repositories;
using _116.Identity.Domain.Constants;
using _116.Identity.Domain.DTOs;
using _116.Shared.Application.Extensions;
using _116.Shared.Contracts.Application.CQRS;
using Carter;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using System.Security.Claims;
using _116.Identity.Application.Shared.Constants;

namespace _116.Identity.Application.Admin.UseCases.Commands.UpdateAvatar.V1;

/// <summary>
/// Response model for updating admin user avatar.
/// </summary>
/// <param name="User">The updated admin user information with the new avatar.</param>
public record AdminUpdateAvatarResponse(
    UserResponseDto User
);

/// <summary>
/// Defines the update avatar endpoint for authenticated admin users.
/// This endpoint accepts multipart/form-data file uploads.
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
                IFormFile avatarFile,
                ClaimsPrincipal user,
                IUserRepository userRepository,
                IDispatcher dispatcher
            ) =>
            {
                // Extract user ID from JWT token claims
                Guid userId = userRepository.GetUserIdFromClaims(user);

                // Send the command with the uploaded file (validation happens in validator)
                var command = new AdminUpdateAvatarCommand(userId, avatarFile);
                AdminUpdateAvatarResult result = await dispatcher.Send(command);

                // Return response
                var response = new AdminUpdateAvatarResponse(result.User);
                return Results.Ok(response);
            })
            .WithName(AdminUpdateAvatarMetaField.UpdateAvatar.Name)
            .WithSummary(AdminUpdateAvatarMetaField.UpdateAvatar.Summary)
            .WithDescription(AdminUpdateAvatarMetaField.UpdateAvatar.Description)
            .RequireAuthorization(UserRolePolicies.RequireAdminOrSuperAdmin)
            .DisableAntiforgery()
            .ProducesValidationProblem()
            .Produces<AdminUpdateAvatarResponse>()
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .ProducesProblem(StatusCodes.Status404NotFound);
    }
}
