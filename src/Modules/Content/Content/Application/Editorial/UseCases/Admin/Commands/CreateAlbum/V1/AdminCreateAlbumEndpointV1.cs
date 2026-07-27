using _116.BuildingBlocks.Constants.Authorization.Policies;
using _116.BuildingBlocks.Constants.RateLimit;
using _116.BuildingBlocks.Utils;
using _116.Content.Application.Editorial.Constants;
using _116.Content.Application.Shared.DTOs;
using _116.Content.Domain.Constants;
using _116.Content.Domain.Enums;
using _116.Shared.Application.Extensions;
using _116.Shared.Contracts.Application.CQRS;
using Carter;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace _116.Content.Application.Editorial.UseCases.Admin.Commands.CreateAlbum.V1;

/// <summary>
/// Request model for creating an album.
/// </summary>
/// <param name="Name">The album's display name.</param>
/// <param name="ArtistId">Optional link to the claimed artist profile this album belongs to.</param>
/// <param name="ReleaseYear">The release year, if known.</param>
/// <param name="Label">The record label, if known.</param>
/// <param name="ReleaseType">What kind of release this is.</param>
public record AdminCreateAlbumRequest(
    string Name,
    Guid? ArtistId,
    short? ReleaseYear,
    string? Label,
    EnumReleaseType ReleaseType
);

/// <summary>
/// Response model for a successful album creation.
/// </summary>
/// <param name="Album">The created album information.</param>
public record AdminCreateAlbumResponse(AlbumDto Album);

/// <summary>
/// Defines the admin create album endpoint.
/// Handles creation of new albums.
/// </summary>
public class AdminCreateAlbumEndpointV1 : ICarterModule
{
    /// <summary>
    /// Configures the album creation route within the API pipeline.
    /// Maps the <c>POST /api/v1/admin/albums</c> endpoint.
    /// </summary>
    /// <param name="app">The route builder used to register API endpoints.</param>
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        RouteGroupBuilder group = app.MapApiVersionGroup(1)
            .MapGroup($"{ContentConstants.Admin}/{EditorialRouteConstants.Albums}")
            .WithTags($"{ContentConstants.Admin}::{EditorialRouteConstants.Albums}");

        group
            .MapPost(
                "/",
                async (AdminCreateAlbumRequest request, IDispatcher dispatcher, HttpContext httpContext) =>
                {
                    var command = new AdminCreateAlbumCommand(
                        Name: request.Name,
                        ArtistId: request.ArtistId,
                        ReleaseYear: request.ReleaseYear,
                        Label: request.Label,
                        ReleaseType: request.ReleaseType
                    );

                    AdminCreateAlbumResult result = await dispatcher.Send(request: command);

                    var response = new AdminCreateAlbumResponse(Album: result.Album);
                    Guid albumId = response.Album.Id;

                    string path = $"{ContentConstants.Admin}/{EditorialRouteConstants.Albums}/{albumId}";
                    string locationUrl = ApiVersionUrl.Build(context: httpContext, path: path);

                    return Results.Created(uri: locationUrl, value: response);
                }
            )
            .WithName(endpointName: AdminCreateAlbumMetaField.CreateAlbum.Name)
            .WithSummary(summary: AdminCreateAlbumMetaField.CreateAlbum.Summary)
            .WithDescription(description: AdminCreateAlbumMetaField.CreateAlbum.Description)
            .WithAuthorization(AccountStatusPolicies.RequireActiveUser)
            .WithAuthorization(UserRolePolicies.RequireSuperAdminOnly)
            .RequireRateLimiting(policyName: RateLimitPolicies.ContentBrowsing)
            .ProducesValidationProblem()
            .Produces<AdminCreateAlbumResponse>(statusCode: StatusCodes.Status201Created)
            .ProducesProblem(statusCode: StatusCodes.Status400BadRequest)
            .ProducesProblem(statusCode: StatusCodes.Status401Unauthorized)
            .ProducesProblem(statusCode: StatusCodes.Status403Forbidden)
            .ProducesProblem(statusCode: StatusCodes.Status404NotFound)
            .ProducesProblem(statusCode: StatusCodes.Status429TooManyRequests);
    }
}
