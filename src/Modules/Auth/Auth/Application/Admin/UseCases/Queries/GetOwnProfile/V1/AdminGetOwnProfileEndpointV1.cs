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

namespace _116.Auth.Application.Admin.UseCases.Queries.GetOwnProfile.V1;

/// <summary>
/// Response model for admin user profile.
/// </summary>
/// <param name="User">The complete admin user profile information including roles and permissions.</param>
public record AdminGetOwnProfileResponse(
    UserResponseDto User
);

/// <summary>
/// Defines the admin user profile endpoint for authenticated admin users.
/// Handles retrieval of complete admin user profile information.
/// </summary>
public class AdminGetOwnProfileEndpointV1 : ICarterModule
{
    /// <summary>
    /// Configures the admin user profile route within the API pipeline.
    /// Maps the <c>/api/v1/admin/profile</c> endpoint to handle admin profile retrieval requests.
    /// </summary>
    /// <param name="app">The route builder used to register API endpoints.</param>
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        RouteGroupBuilder group = app
            .MapApiVersionGroup(version: 1)
            .MapGroup($"{AuthConstants.Admin}/{AuthRouteConstants.Profile}")
            .WithTags($"{AuthConstants.Admin}::{AuthRouteConstants.Profile}");

        group.MapGet("/", async (
                ClaimsPrincipal user,
                IUserRepository userRepository,
                IDispatcher dispatcher
            ) =>
            {
                // Extract user ID from JWT token claims
                Guid userId = userRepository.GetUserIdFromClaims(user);

                // Send the query to get admin user profile
                var query = new AdminGetOwnProfileQuery(userId);
                AdminGetOwnProfileResult result = await dispatcher.Send(query);

                // Adapt the result to the response type
                var response = new AdminGetOwnProfileResponse(
                    result.User
                );

                return Results.Ok(response);
            })
            .WithName(AdminGetOwnProfileMetaField.GetOwnProfile.Name)
            .WithSummary(AdminGetOwnProfileMetaField.GetOwnProfile.Summary)
            .WithDescription(AdminGetOwnProfileMetaField.GetOwnProfile.Description)
            .RequireAuthorization(UserRolePolicies.RequireAdminOrSuperAdmin)
            .RequireAuthorization(AccountStatusPolicies.RequireLoggedInUser)
            .Produces<AdminGetOwnProfileResponse>()
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .ProducesProblem(StatusCodes.Status404NotFound);
    }
}
