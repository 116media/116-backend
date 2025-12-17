using System.Security.Claims;

using _116.Identity.Application.Shared.Authorizations.Policies;
using _116.Identity.Application.Shared.Constants;
using _116.Identity.Application.Shared.DTOs;
using _116.Identity.Application.Shared.Repositories;
using _116.Identity.Domain.Constants;
using _116.Shared.Application.Extensions;
using _116.Shared.Contracts.Application.CQRS;

using Carter;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace _116.Identity.Application.User.Admin.UseCases.Commands.UpdateOwnProfile.V1;

/// <summary>
/// Request model for updating admin own profile.
/// This endpoint requires admin user authentication - only logged-in admin users can update their own profile.
/// </summary>
/// <param name="UserName">The new username (optional).</param>
/// <param name="CountryName">The new country name (optional).</param>
/// <param name="CountryFlagUrl">The new country flag URL (optional).</param>
/// <param name="PartialPhoneNumber">The new partial phone number (optional).</param>
/// <param name="CountryIsoCode">The new country ISO code (optional).</param>
/// <param name="CountryDialCode">The new country dial code (optional).</param>
public record AdminUpdateOwnProfileRequest(
    string? UserName,
    string? CountryName,
    string? CountryFlagUrl,
    string? PartialPhoneNumber,
    string? CountryIsoCode,
    string? CountryDialCode
);
/// <summary>
/// Response model for updating admin own profile.
/// </summary>
/// <param name="User">The updated admin user profile information.</param>
public record AdminUpdateOwnProfileResponse(
    UserResponseDto User
);
/// <summary>
/// Defines the update own profile endpoint for authenticated admin users (V1).
/// This endpoint requires admin user authentication - only logged-in admin users can update their own profile.
/// </summary>
public class AdminUpdateOwnProfileEndpointV1 : ICarterModule
{
    /// <summary>
    /// Configures the admin update own profile route within the API pipeline.
    /// Maps the <c>/api/v1/admin/profile</c> endpoint to handle profile update requests.
    /// </summary>
    /// <param name="app">The route builder used to register API endpoints.</param>
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        RouteGroupBuilder group = app
            .MapApiVersionGroup(version: 1)
            .MapGroup($"{IdentityConstants.Admin}/{AuthRouteConstants.Profile}")
            .WithTags($"{IdentityConstants.Admin}::{AuthRouteConstants.Profile}");
        group.MapPatch("/", async (
                AdminUpdateOwnProfileRequest request,
                ClaimsPrincipal user,
                IUserRepository userRepository,
                IDispatcher dispatcher
            ) =>
            {
                // Extract user ID from JWT token claims
                Guid userId = userRepository.GetUserIdFromClaims(user);
                // Send the command to update the profile
                var command = new AdminUpdateOwnProfileCommand(
                    userId,
                    request.UserName,
                    request.CountryName,
                    request.CountryFlagUrl,
                    request.PartialPhoneNumber,
                    request.CountryIsoCode,
                    request.CountryDialCode
                );
                AdminUpdateOwnProfileResult result = await dispatcher.Send(command);
                // Adapt the result to the response type
                var response = new AdminUpdateOwnProfileResponse(
                    result.User
                );
                return Results.Ok(response);
            })
            .WithName(AdminUpdateOwnProfileMetaField.UpdateOwnProfile.Name)
            .WithSummary(AdminUpdateOwnProfileMetaField.UpdateOwnProfile.Summary)
            .WithDescription(AdminUpdateOwnProfileMetaField.UpdateOwnProfile.Description)
            .RequireAuthorization(UserRolePolicies.RequireAdminOrSuperAdmin)
            .ProducesValidationProblem()
            .Produces<AdminUpdateOwnProfileResponse>()
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status409Conflict);
    }
}
