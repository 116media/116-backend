using _116.BuildingBlocks.Constants.Authorization.Policies;
using _116.BuildingBlocks.Constants.RateLimit;
using _116.Content.Application.Editorial.Constants;
using _116.Content.Application.Shared.DTOs;
using _116.Content.Domain.Constants;
using _116.Shared.Application.Extensions;
using _116.Shared.Contracts.Application.CQRS;
using Carter;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace _116.Content.Application.Editorial.UseCases.Admin.Commands.VerifyArtistOwner.V1;

/// <summary>
/// Request model for verifying an artist profile's owner.
/// </summary>
/// <param name="UserId">The identity user UUID confirmed as the profile's owner.</param>
public record AdminVerifyArtistOwnerRequest(Guid UserId);

/// <summary>
/// Response model for a successful artist owner verification.
/// </summary>
/// <param name="Artist">The claimed artist profile information.</param>
public record AdminVerifyArtistOwnerResponse(ArtistDto Artist);

/// <summary>
/// Defines the admin verify artist owner endpoint.
/// Handles confirming and finalizing an artist profile's ownership claim.
/// </summary>
public class AdminVerifyArtistOwnerEndpointV1 : ICarterModule
{
    /// <summary>
    /// Configures the artist owner verification route within the API pipeline.
    /// Maps the <c>POST /api/v1/admin/artists/{id}/verify-owner</c> endpoint.
    /// </summary>
    /// <param name="app">The route builder used to register API endpoints.</param>
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        RouteGroupBuilder group = app.MapApiVersionGroup(1)
            .MapGroup($"{ContentConstants.Admin}/{EditorialRouteConstants.Artists}")
            .WithTags($"{ContentConstants.Admin}::{EditorialRouteConstants.Artists}");

        group
            .MapPost(
                $"/{{id}}/{EditorialRouteConstants.VerifyOwner}",
                async (Guid id, AdminVerifyArtistOwnerRequest request, IDispatcher dispatcher) =>
                {
                    var command = new AdminVerifyArtistOwnerCommand(ArtistId: id, UserId: request.UserId);
                    AdminVerifyArtistOwnerResult result = await dispatcher.Send(request: command);

                    var response = new AdminVerifyArtistOwnerResponse(Artist: result.Artist);
                    return Results.Ok(response);
                }
            )
            .WithName(endpointName: AdminVerifyArtistOwnerMetaField.VerifyArtistOwner.Name)
            .WithSummary(summary: AdminVerifyArtistOwnerMetaField.VerifyArtistOwner.Summary)
            .WithDescription(description: AdminVerifyArtistOwnerMetaField.VerifyArtistOwner.Description)
            .WithAuthorization(AccountStatusPolicies.RequireActiveUser)
            .WithAuthorization(UserRolePolicies.RequireAdminOrSuperAdmin)
            .RequireRateLimiting(policyName: RateLimitPolicies.ContentBrowsing)
            .ProducesValidationProblem()
            .Produces<AdminVerifyArtistOwnerResponse>(statusCode: StatusCodes.Status200OK)
            .ProducesProblem(statusCode: StatusCodes.Status401Unauthorized)
            .ProducesProblem(statusCode: StatusCodes.Status403Forbidden)
            .ProducesProblem(statusCode: StatusCodes.Status404NotFound)
            .ProducesProblem(statusCode: StatusCodes.Status409Conflict)
            .ProducesProblem(statusCode: StatusCodes.Status429TooManyRequests);
    }
}
