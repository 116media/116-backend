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

namespace _116.Identity.Application.User.UseCases.Public.Commands.UpdateAvatar.V1;

/// <summary>
/// Response model for updating user avatar.
/// </summary>
/// <param name="User">The updated user information with the new avatar.</param>
public record PublicUpdateAvatarResponse(UserResponseDto User);

/// <summary>
/// Defines the update avatar endpoint for authenticated public users.
/// This endpoint accepts multipart/form-data file uploads.
/// </summary>
public class PublicUpdateAvatarEndpointV1 : ICarterModule
{
    /// <summary>
    /// Configures the update avatar route within the API pipeline.
    /// Maps the <c>/api/v1/public/me/avatar</c> endpoint to handle avatar update requests.
    /// </summary>
    /// <param name="app">The route builder used to register API endpoints.</param>
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        RouteGroupBuilder group = app.MapApiVersionGroup(1)
            .MapGroup($"{IdentityConstants.Public}/{IdentityConstants.Me}")
            .WithTags($"{IdentityConstants.Public}::{IdentityConstants.Me}");

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

                    var command = new PublicUpdateAvatarCommand(
                        UserId: userId,
                        SessionId: sessionId,
                        AvatarFile: avatarFile
                    );
                    PublicUpdateAvatarResult result = await dispatcher.Send(request: command);

                    var response = new PublicUpdateAvatarResponse(User: result.User);

                    return Results.Ok(value: response);
                }
            )
            .WithName(endpointName: PublicUpdateAvatarMetaField.UpdateAvatar.Name)
            .WithSummary(summary: PublicUpdateAvatarMetaField.UpdateAvatar.Summary)
            .WithDescription(description: PublicUpdateAvatarMetaField.UpdateAvatar.Description)
            .WithAuthorization(UserRolePolicies.RequireVisitorOnly)
            .RequireRateLimiting(policyName: RateLimitPolicies.FileUpload)
            .DisableAntiforgery()
            .ProducesValidationProblem()
            .Produces<PublicUpdateAvatarResponse>()
            .ProducesProblem(statusCode: StatusCodes.Status400BadRequest)
            .ProducesProblem(statusCode: StatusCodes.Status401Unauthorized)
            .ProducesProblem(statusCode: StatusCodes.Status403Forbidden)
            .ProducesProblem(statusCode: StatusCodes.Status404NotFound)
            .ProducesProblem(statusCode: StatusCodes.Status429TooManyRequests);
    }
}
