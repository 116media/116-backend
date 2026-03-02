using System.Security.Claims;
using _116.BuildingBlocks.Constants.Authorization.Policies;
using _116.BuildingBlocks.Constants.RateLimit;
using _116.Identity.Application.Shared.DTOs;
using _116.Identity.Application.Shared.Repositories;
using _116.Identity.Application.User.Constants;
using _116.Identity.Domain.Constants;
using _116.Shared.Application.Extensions;
using _116.Shared.Contracts.Application.CQRS;
using Carter;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace _116.Identity.Application.User.UseCases.Admin.Commands.UpdateAvatar.V1;

/// <summary>
/// Response model for updating admin user avatar.
/// </summary>
/// <param name="User">The updated admin user information with the new avatar.</param>
public record AdminUpdateAvatarResponse(UserResponseDto User);

/// <summary>
/// Defines the update avatar endpoint for authenticated admin users.
/// This endpoint accepts multipart/form-data file uploads.
/// </summary>
public class AdminUpdateAvatarEndpointV1 : ICarterModule
{
    /// <summary>
    /// Configures the admin update avatar route within the API pipeline.
    /// Maps the <c>/api/v1/admin/me/avatar</c> endpoint to handle admin avatar update requests.
    /// </summary>
    /// <param name="app">The route builder used to register API endpoints.</param>
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        RouteGroupBuilder group = app.MapApiVersionGroup(1)
            .MapGroup($"{IdentityConstants.Admin}/{IdentityConstants.Me}")
            .WithTags($"{IdentityConstants.Admin}::{IdentityConstants.Me}");

        group
            .MapPatch(
                pattern: UserRouteConstants.Avatar,
                async (
                    IFormFile avatarFile,
                    ClaimsPrincipal user,
                    IAuthRepository authRepository,
                    IDispatcher dispatcher
                ) =>
                {
                    Guid userId = authRepository.GetUserIdFromClaims(user: user);
                    Guid sessionId = authRepository.GetSessionIdFromClaims(user: user);

                    var command = new AdminUpdateAvatarCommand(
                        UserId: userId,
                        SessionId: sessionId,
                        AvatarFile: avatarFile
                    );
                    AdminUpdateAvatarResult result = await dispatcher.Send(request: command);

                    var response = new AdminUpdateAvatarResponse(User: result.User);
                    return Results.Ok(value: response);
                }
            )
            .WithName(endpointName: AdminUpdateAvatarMetaField.UpdateAvatar.Name)
            .WithSummary(summary: AdminUpdateAvatarMetaField.UpdateAvatar.Summary)
            .WithDescription(description: AdminUpdateAvatarMetaField.UpdateAvatar.Description)
            .WithAuthorization(UserRolePolicies.RequireAdminOrSuperAdmin)
            .RequireRateLimiting(policyName: RateLimitPolicies.FileUpload)
            .DisableAntiforgery()
            .ProducesValidationProblem()
            .Produces<AdminUpdateAvatarResponse>()
            .ProducesProblem(statusCode: StatusCodes.Status400BadRequest)
            .ProducesProblem(statusCode: StatusCodes.Status401Unauthorized)
            .ProducesProblem(statusCode: StatusCodes.Status403Forbidden)
            .ProducesProblem(statusCode: StatusCodes.Status404NotFound)
            .ProducesProblem(statusCode: StatusCodes.Status429TooManyRequests);
    }
}
