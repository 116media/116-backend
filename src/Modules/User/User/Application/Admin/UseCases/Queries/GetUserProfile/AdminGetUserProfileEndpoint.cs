using _116.BuildingBlocks.Constants;
using _116.Shared.Contracts.Application.CQRS;
using _116.User.Application.Shared.Authorizations.Policies;
using _116.User.Application.Shared.Errors;
using _116.User.Domain.DTOs;
using Carter;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using System.Security.Claims;

namespace _116.User.Application.Admin.UseCases.Queries.GetUserProfile;

/// <summary>
/// Response model for admin user profile.
/// </summary>
/// <param name="User">The complete admin user profile information including roles and permissions.</param>
public record AdminGetUserProfileResponse(
    UserResponseDto User
);

/// <summary>
/// Defines the admin user profile endpoint for authenticated admin users.
/// Handles retrieval of complete admin user profile information.
/// </summary>
public class AdminGetUserProfileEndpoint : ICarterModule
{
    /// <summary>
    /// Configures the admin user profile route within the API pipeline.
    /// Maps the <c>/api/v1/admin/profile</c> endpoint to handle admin profile retrieval requests.
    /// </summary>
    /// <param name="app">The route builder used to register API endpoints.</param>
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        RouteGroupBuilder group = app
            .MapGroup(RouteConstants.V1.Admin.Profile)
            .WithTags("Admin::authentication");

        group.MapGet("/", async (ClaimsPrincipal user, IDispatcher dispatcher) =>
            {
                // Extract user ID from JWT token claims
                string? userIdClaim = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out Guid userId))
                {
                    throw UserErrors.InvalidUserAuthentication();
                }

                // Send the query to get admin user profile
                var query = new AdminGetUserProfileQuery(userId);
                AdminGetUserProfileResult result = await dispatcher.Send(query);

                // Adapt the result to the response type
                var response = new AdminGetUserProfileResponse(
                    result.User
                );

                return Results.Ok(response);
            })
            .WithName(AdminGetUserProfileMetaField.GetUserProfile.Name)
            .WithSummary(AdminGetUserProfileMetaField.GetUserProfile.Summary)
            .WithDescription(AdminGetUserProfileMetaField.GetUserProfile.Description)
            .RequireAuthorization(UserRolePolicies.RequireAdminOrSuperAdmin)
            .RequireAuthorization(AccountStatusPolicies.RequireLoggedInUser)
            .Produces<AdminGetUserProfileResponse>()
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .ProducesProblem(StatusCodes.Status404NotFound);
    }
}
