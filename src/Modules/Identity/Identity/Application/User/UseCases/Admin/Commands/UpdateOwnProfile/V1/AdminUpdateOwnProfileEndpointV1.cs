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

namespace _116.Identity.Application.User.UseCases.Admin.Commands.UpdateOwnProfile.V1;

/// <summary>
/// Request model for updating admin own profile.
/// This endpoint requires admin user authentication - only logged-in admin users can update their own profile.
/// </summary>
/// <param name="UserName">The new username (optional).</param>
/// <param name="CountryName">The new country name (optional).</param>
/// <param name="PartialPhoneNumber">The new partial phone number (optional).</param>
/// <param name="CountryIsoCode">The new country ISO code (optional).</param>
/// <param name="CountryDialCode">The new country dial code (optional).</param>
public record AdminUpdateOwnProfileRequest(
    string? UserName,
    string? CountryName,
    string? PartialPhoneNumber,
    string? CountryIsoCode,
    string? CountryDialCode
);

/// <summary>
/// Response model for updating admin own profile.
/// </summary>
/// <param name="User">The updated admin user profile information.</param>
public record AdminUpdateOwnProfileResponse(UserResponseDto User);

/// <summary>
/// Defines the update own profile endpoint for authenticated admin users (V1).
/// This endpoint requires admin user authentication - only logged-in admin users can update their own profile.
/// </summary>
public class AdminUpdateOwnProfileEndpointV1 : ICarterModule
{
    /// <summary>
    /// Configures the admin update own profile route within the API pipeline.
    /// Maps the <c>/api/v1/admin/user/profile</c> endpoint to handle profile update requests.
    /// </summary>
    /// <param name="app">The route builder used to register API endpoints.</param>
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        RouteGroupBuilder group = app.MapApiVersionGroup(1)
            .MapGroup($"{IdentityConstants.Admin}/{UserRouteConstants.Endpoint}")
            .WithTags($"{IdentityConstants.Admin}::{UserRouteConstants.Endpoint}");

        group
            .MapPatch(
                pattern: UserRouteConstants.Profile,
                async (
                    AdminUpdateOwnProfileRequest request,
                    ClaimsPrincipal user,
                    IAuthRepository authRepository,
                    IDispatcher dispatcher
                ) =>
                {
                    Guid userId = authRepository.GetUserIdFromClaims(user: user);
                    Guid sessionId = authRepository.GetSessionIdFromClaims(user: user);

                    var command = new AdminUpdateOwnProfileCommand(
                        UserId: userId,
                        SessionId: sessionId,
                        UserName: request.UserName,
                        CountryName: request.CountryName,
                        CountryIsoCode: request.CountryIsoCode,
                        CountryDialCode: request.CountryDialCode,
                        PartialPhoneNumber: request.PartialPhoneNumber
                    );
                    AdminUpdateOwnProfileResult result = await dispatcher.Send(request: command);

                    var response = new AdminUpdateOwnProfileResponse(User: result.User);

                    return Results.Ok(value: response);
                }
            )
            .WithName(endpointName: AdminUpdateOwnProfileMetaField.UpdateOwnProfile.Name)
            .WithSummary(summary: AdminUpdateOwnProfileMetaField.UpdateOwnProfile.Summary)
            .WithDescription(description: AdminUpdateOwnProfileMetaField.UpdateOwnProfile.Description)
            .WithAuthorization(UserRolePolicies.RequireAdminOrSuperAdmin)
            .RequireRateLimiting(policyName: RateLimitPolicies.UserProfile)
            .ProducesValidationProblem()
            .Produces<AdminUpdateOwnProfileResponse>()
            .ProducesProblem(statusCode: StatusCodes.Status400BadRequest)
            .ProducesProblem(statusCode: StatusCodes.Status401Unauthorized)
            .ProducesProblem(statusCode: StatusCodes.Status403Forbidden)
            .ProducesProblem(statusCode: StatusCodes.Status404NotFound)
            .ProducesProblem(statusCode: StatusCodes.Status409Conflict)
            .ProducesProblem(statusCode: StatusCodes.Status429TooManyRequests);
    }
}
