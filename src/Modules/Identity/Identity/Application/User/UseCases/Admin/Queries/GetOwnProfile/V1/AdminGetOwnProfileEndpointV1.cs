using System.Security.Claims;
using _116.BuildingBlocks.Constants.RateLimit;
using _116.Identity.Application.Shared.Authorizations.Policies;
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

namespace _116.Identity.Application.User.UseCases.Admin.Queries.GetOwnProfile.V1;

/// <summary>
/// Response model for admin user profile.
/// </summary>
/// <param name="User">The complete admin user profile information including roles and permissions.</param>
public record AdminGetOwnProfileResponse(UserResponseDto User);

/// <summary>
/// Defines the admin user profile endpoint for authenticated admin users.
/// Handles retrieval of complete admin user profile information.
/// </summary>
public class AdminGetOwnProfileEndpointV1 : ICarterModule
{
    /// <summary>
    /// Configures the admin user profile route within the API pipeline.
    /// Maps the <c>/api/v1/admin/user/profile</c> endpoint to handle admin profile retrieval requests.
    /// </summary>
    /// <param name="app">The route builder used to register API endpoints.</param>
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        RouteGroupBuilder group = app.MapApiVersionGroup(1)
            .MapGroup($"{IdentityConstants.Admin}/{UserRouteConstants.Endpoint}")
            .WithTags($"{IdentityConstants.Admin}::{UserRouteConstants.Endpoint}");

        group
            .MapGet(
                pattern: UserRouteConstants.Profile,
                async (ClaimsPrincipal user, IAuthRepository authRepository, IDispatcher dispatcher) =>
                {
                    Guid userId = authRepository.GetUserIdFromClaims(user: user);

                    var query = new AdminGetOwnProfileQuery(UserId: userId);
                    AdminGetOwnProfileResult result = await dispatcher.Send(request: query);

                    var response = new AdminGetOwnProfileResponse(User: result.User);

                    return Results.Ok(value: response);
                }
            )
            .WithName(endpointName: AdminGetOwnProfileMetaField.GetOwnProfile.Name)
            .WithSummary(summary: AdminGetOwnProfileMetaField.GetOwnProfile.Summary)
            .WithDescription(description: AdminGetOwnProfileMetaField.GetOwnProfile.Description)
            .RequireAuthorization(UserRolePolicies.RequireAdminOrSuperAdmin)
            .RequireRateLimiting(policyName: RateLimitPolicies.UserProfile)
            .Produces<AdminGetOwnProfileResponse>()
            .ProducesProblem(statusCode: StatusCodes.Status401Unauthorized)
            .ProducesProblem(statusCode: StatusCodes.Status403Forbidden)
            .ProducesProblem(statusCode: StatusCodes.Status404NotFound)
            .ProducesProblem(statusCode: StatusCodes.Status429TooManyRequests);
    }
}
