using _116.Shared.Contracts.Application.CQRS;
using _116.Auth.Application.Shared.Authorizations.Policies;
using _116.Auth.Application.Shared.Repositories;
using _116.Auth.Domain.DTOs;
using Carter;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using System.Security.Claims;
using _116.Auth.Application.Shared.Constants;
using _116.Auth.Domain.Constants;
using _116.Shared.Application.Extensions;

namespace _116.Auth.Application.Public.UseCases.Commands.UpdateAvatar.V1;

/// <summary>
/// Request model for updating user avatar.
/// This endpoint requires user authentication - only logged-in verified users can update their avatar.
/// </summary>
/// <param name="AvatarUrl">The new avatar URL to set for the user.</param>
public record PublicUpdateAvatarRequest(
    string AvatarUrl
);

/// <summary>
/// Response model for updating user avatar.
/// </summary>
/// <param name="User">The updated user information with the new avatar.</param>
public record PublicUpdateAvatarResponse(
    UserResponseDto User
);

/// <summary>
/// Defines the update avatar endpoint for authenticated public users.
/// This endpoint requires user authentication - only logged-in verified users can update their avatar.
/// </summary>
public class PublicUpdateAvatarEndpointV1 : ICarterModule
{
    /// <summary>
    /// Configures the update avatar route within the API pipeline.
    /// Maps the <c>/api/v1/public/profile/avatar</c> endpoint to handle avatar update requests.
    /// </summary>
    /// <param name="app">The route builder used to register API endpoints.</param>
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        RouteGroupBuilder group = app
            .MapApiVersionGroup(version: 1)
            .MapGroup($"{AuthConstants.Public}/{AuthRouteConstants.Profile}")
            .WithTags($"{AuthConstants.Public}::{AuthRouteConstants.Profile}");

        group.MapPatch(AuthRouteConstants.Avatar, async (
                PublicUpdateAvatarRequest request,
                ClaimsPrincipal user,
                IUserRepository userRepository,
                IDispatcher dispatcher
            ) =>
            {
                // Extract user ID from JWT token claims
                Guid userId = userRepository.GetUserIdFromClaims(user);

                // Send the command to update the avatar
                var command = new PublicUpdateAvatarCommand(userId, request.AvatarUrl);
                PublicUpdateAvatarResult result = await dispatcher.Send(command);

                // Adapt the result to the response type
                var response = new PublicUpdateAvatarResponse(result.User);

                return Results.Ok(response);
            })
            .WithName(PublicUpdateAvatarMetaField.UpdateAvatar.Name)
            .WithSummary(PublicUpdateAvatarMetaField.UpdateAvatar.Summary)
            .WithDescription(PublicUpdateAvatarMetaField.UpdateAvatar.Description)
            .RequireAuthorization(UserRolePolicies.RequireVisitorOnly)
            .ProducesValidationProblem()
            .Produces<PublicUpdateAvatarResponse>()
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .ProducesProblem(StatusCodes.Status404NotFound);
    }
}
