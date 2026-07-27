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

namespace _116.Content.Application.Editorial.UseCases.Admin.Commands.UpdateArtist.V1;

/// <summary>
/// Request model for updating an artist profile.
/// </summary>
/// <param name="Name">The artist's display name.</param>
/// <param name="Bio">Optional free-text biography, or null to clear it.</param>
/// <param name="RealName">The artist's legal or birth name, or null to clear it.</param>
/// <param name="Aliases">Alternate names the artist is known by, or null to clear them.</param>
/// <param name="Birthdate">The artist's date of birth, or null to clear it.</param>
/// <param name="Hometown">Where the artist is from, or null to clear it.</param>
public record AdminUpdateArtistRequest(
    string Name,
    string? Bio,
    string? RealName,
    IReadOnlyList<string>? Aliases,
    DateOnly? Birthdate,
    string? Hometown
);

/// <summary>
/// Response model for a successful artist profile update.
/// </summary>
/// <param name="Artist">The updated artist profile information.</param>
public record AdminUpdateArtistResponse(ArtistDto Artist);

/// <summary>
/// Defines the admin update artist profile endpoint.
/// Handles updating an existing artist profile's editable fields.
/// </summary>
public class AdminUpdateArtistEndpointV1 : ICarterModule
{
    /// <summary>
    /// Configures the artist profile update route within the API pipeline.
    /// Maps the <c>PUT /api/v1/admin/artists/{id}</c> endpoint.
    /// </summary>
    /// <param name="app">The route builder used to register API endpoints.</param>
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        RouteGroupBuilder group = app.MapApiVersionGroup(1)
            .MapGroup($"{ContentConstants.Admin}/{EditorialRouteConstants.Artists}")
            .WithTags($"{ContentConstants.Admin}::{EditorialRouteConstants.Artists}");

        group
            .MapPut(
                "/{id}",
                async (Guid id, AdminUpdateArtistRequest request, IDispatcher dispatcher) =>
                {
                    var command = new AdminUpdateArtistCommand(
                        Id: id,
                        Name: request.Name,
                        Bio: request.Bio,
                        RealName: request.RealName,
                        Aliases: request.Aliases,
                        Birthdate: request.Birthdate,
                        Hometown: request.Hometown
                    );

                    AdminUpdateArtistResult result = await dispatcher.Send(request: command);

                    var response = new AdminUpdateArtistResponse(Artist: result.Artist);
                    return Results.Ok(response);
                }
            )
            .WithName(endpointName: AdminUpdateArtistMetaField.UpdateArtist.Name)
            .WithSummary(summary: AdminUpdateArtistMetaField.UpdateArtist.Summary)
            .WithDescription(description: AdminUpdateArtistMetaField.UpdateArtist.Description)
            .WithAuthorization(AccountStatusPolicies.RequireActiveUser)
            .WithAuthorization(UserRolePolicies.RequireSuperAdminOnly)
            .RequireRateLimiting(policyName: RateLimitPolicies.ContentBrowsing)
            .ProducesValidationProblem()
            .Produces<AdminUpdateArtistResponse>(statusCode: StatusCodes.Status200OK)
            .ProducesProblem(statusCode: StatusCodes.Status400BadRequest)
            .ProducesProblem(statusCode: StatusCodes.Status401Unauthorized)
            .ProducesProblem(statusCode: StatusCodes.Status403Forbidden)
            .ProducesProblem(statusCode: StatusCodes.Status404NotFound)
            .ProducesProblem(statusCode: StatusCodes.Status429TooManyRequests);
    }
}
