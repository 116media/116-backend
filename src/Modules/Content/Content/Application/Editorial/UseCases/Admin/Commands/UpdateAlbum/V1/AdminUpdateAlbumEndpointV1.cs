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

namespace _116.Content.Application.Editorial.UseCases.Admin.Commands.UpdateAlbum.V1;

/// <summary>
/// Request model for updating an album.
/// </summary>
/// <param name="Name">The album's display name.</param>
/// <param name="ReleaseYear">The release year, or null to clear it.</param>
/// <param name="Label">The record label, or null to clear it.</param>
public record AdminUpdateAlbumRequest(string Name, short? ReleaseYear, string? Label);

/// <summary>
/// Response model for a successful album update.
/// </summary>
/// <param name="Album">The updated album information.</param>
public record AdminUpdateAlbumResponse(AlbumDto Album);

/// <summary>
/// Defines the admin update album endpoint.
/// Handles updating an existing album's editable fields.
/// </summary>
public class AdminUpdateAlbumEndpointV1 : ICarterModule
{
    /// <summary>
    /// Configures the album update route within the API pipeline.
    /// Maps the <c>PUT /api/v1/admin/albums/{id}</c> endpoint.
    /// </summary>
    /// <param name="app">The route builder used to register API endpoints.</param>
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        RouteGroupBuilder group = app.MapApiVersionGroup(1)
            .MapGroup($"{ContentConstants.Admin}/{EditorialRouteConstants.Albums}")
            .WithTags($"{ContentConstants.Admin}::{EditorialRouteConstants.Albums}");

        group
            .MapPut(
                "/{id}",
                async (Guid id, AdminUpdateAlbumRequest request, IDispatcher dispatcher) =>
                {
                    var command = new AdminUpdateAlbumCommand(
                        Id: id,
                        Name: request.Name,
                        ReleaseYear: request.ReleaseYear,
                        Label: request.Label
                    );

                    AdminUpdateAlbumResult result = await dispatcher.Send(request: command);

                    var response = new AdminUpdateAlbumResponse(Album: result.Album);
                    return Results.Ok(response);
                }
            )
            .WithName(endpointName: AdminUpdateAlbumMetaField.UpdateAlbum.Name)
            .WithSummary(summary: AdminUpdateAlbumMetaField.UpdateAlbum.Summary)
            .WithDescription(description: AdminUpdateAlbumMetaField.UpdateAlbum.Description)
            .WithAuthorization(AccountStatusPolicies.RequireActiveUser)
            .WithAuthorization(UserRolePolicies.RequireSuperAdminOnly)
            .RequireRateLimiting(policyName: RateLimitPolicies.ContentBrowsing)
            .ProducesValidationProblem()
            .Produces<AdminUpdateAlbumResponse>(statusCode: StatusCodes.Status200OK)
            .ProducesProblem(statusCode: StatusCodes.Status400BadRequest)
            .ProducesProblem(statusCode: StatusCodes.Status401Unauthorized)
            .ProducesProblem(statusCode: StatusCodes.Status403Forbidden)
            .ProducesProblem(statusCode: StatusCodes.Status404NotFound)
            .ProducesProblem(statusCode: StatusCodes.Status429TooManyRequests);
    }
}
