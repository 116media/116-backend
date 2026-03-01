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

namespace _116.Identity.Application.User.UseCases.Public.Commands.UpdateOwnProfile.V1;

/// <summary>
/// Request model for updating own profile.
/// This endpoint requires user authentication - only logged-in users can update their own profile.
/// </summary>
/// <param name="Email">The new email address (optional).</param>
/// <param name="UserName">The new username (optional).</param>
/// <param name="CountryName">The new country name (optional).</param>
/// <param name="PartialPhoneNumber">The new partial phone number (optional).</param>
/// <param name="CountryIsoCode">The new country ISO code (optional).</param>
/// <param name="CountryDialCode">The new country dial code (optional).</param>
public record PublicUpdateOwnProfileRequest(
    string? Email,
    string? UserName,
    string? CountryName,
    string? PartialPhoneNumber,
    string? CountryIsoCode,
    string? CountryDialCode
);

/// <summary>
/// Response model for updating own profile.
/// </summary>
/// <param name="User">The updated user profile information.</param>
public record PublicUpdateOwnProfileResponse(UserResponseDto User);

/// <summary>
/// Defines the update own profile endpoint for authenticated public users.
/// This endpoint requires user authentication - only logged-in users can update their own profile.
/// </summary>
public class PublicUpdateOwnProfileEndpointV1 : ICarterModule
{
    /// <summary>
    /// Configures the update own profile route within the API pipeline.
    /// Maps the <c>/api/v1/public/user/profile</c> endpoint to handle profile update requests.
    /// </summary>
    /// <param name="app">The route builder used to register API endpoints.</param>
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        RouteGroupBuilder group = app.MapApiVersionGroup(1)
            .MapGroup($"{IdentityConstants.Public}/{UserRouteConstants.Endpoint}")
            .WithTags($"{IdentityConstants.Public}::{UserRouteConstants.Endpoint}");

        group
            .MapPatch(
                pattern: UserRouteConstants.Profile,
                async (
                    PublicUpdateOwnProfileRequest request,
                    ClaimsPrincipal user,
                    IAuthRepository authRepository,
                    IDispatcher dispatcher
                ) =>
                {
                    Guid userId = authRepository.GetUserIdFromClaims(user: user);
                    Guid sessionId = authRepository.GetSessionIdFromClaims(user: user);

                    var command = new PublicUpdateOwnProfileCommand(
                        UserId: userId,
                        SessionId: sessionId,
                        Email: request.Email,
                        UserName: request.UserName,
                        CountryName: request.CountryName,
                        CountryIsoCode: request.CountryIsoCode,
                        CountryDialCode: request.CountryDialCode,
                        PartialPhoneNumber: request.PartialPhoneNumber
                    );
                    PublicUpdateOwnProfileResult result = await dispatcher.Send(request: command);

                    var response = new PublicUpdateOwnProfileResponse(User: result.User);

                    return Results.Ok(value: response);
                }
            )
            .WithName(endpointName: PublicUpdateOwnProfileMetaField.UpdateOwnProfile.Name)
            .WithSummary(summary: PublicUpdateOwnProfileMetaField.UpdateOwnProfile.Summary)
            .WithDescription(description: PublicUpdateOwnProfileMetaField.UpdateOwnProfile.Description)
            .WithAuthorization(UserRolePolicies.RequireVisitorOnly)
            .RequireRateLimiting(policyName: RateLimitPolicies.UserProfile)
            .ProducesValidationProblem()
            .Produces<PublicUpdateOwnProfileResponse>()
            .ProducesProblem(statusCode: StatusCodes.Status400BadRequest)
            .ProducesProblem(statusCode: StatusCodes.Status401Unauthorized)
            .ProducesProblem(statusCode: StatusCodes.Status403Forbidden)
            .ProducesProblem(statusCode: StatusCodes.Status404NotFound)
            .ProducesProblem(statusCode: StatusCodes.Status409Conflict)
            .ProducesProblem(statusCode: StatusCodes.Status429TooManyRequests);
    }
}
