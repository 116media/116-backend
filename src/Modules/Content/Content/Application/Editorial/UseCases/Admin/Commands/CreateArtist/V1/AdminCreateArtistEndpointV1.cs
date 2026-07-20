using _116.BuildingBlocks.Constants.Authorization.Policies;
using _116.BuildingBlocks.Constants.RateLimit;
using _116.BuildingBlocks.Utils;
using _116.Content.Application.Editorial.Constants;
using _116.Content.Application.Shared.DTOs;
using _116.Content.Domain.Constants;
using _116.Shared.Application.Extensions;
using _116.Shared.Contracts.Application.CQRS;
using Carter;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace _116.Content.Application.Editorial.UseCases.Admin.Commands.CreateArtist.V1;

/// <summary>
/// Request model for creating an artist profile.
/// </summary>
/// <param name="Name">The artist's display name.</param>
/// <param name="Slug">The URL-safe slug for the artist's public page.</param>
/// <param name="Bio">Optional free-text biography.</param>
public record AdminCreateArtistRequest(string Name, string Slug, string? Bio);

/// <summary>
/// Response model for a successful artist profile creation.
/// </summary>
/// <param name="Artist">The created artist profile information.</param>
public record AdminCreateArtistResponse(ArtistDto Artist);

/// <summary>
/// Defines the admin create artist profile endpoint.
/// Handles creation of new, unclaimed artist profiles.
/// </summary>
public class AdminCreateArtistEndpointV1 : ICarterModule
{
    /// <summary>
    /// Configures the artist profile creation route within the API pipeline.
    /// Maps the <c>POST /api/v1/admin/artists</c> endpoint.
    /// </summary>
    /// <param name="app">The route builder used to register API endpoints.</param>
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        RouteGroupBuilder group = app.MapApiVersionGroup(1)
            .MapGroup($"{ContentConstants.Admin}/{EditorialRouteConstants.Artists}")
            .WithTags($"{ContentConstants.Admin}::{EditorialRouteConstants.Artists}");

        group
            .MapPost(
                "/",
                async (AdminCreateArtistRequest request, IDispatcher dispatcher, HttpContext httpContext) =>
                {
                    var command = new AdminCreateArtistCommand(
                        Name: request.Name,
                        Slug: request.Slug,
                        Bio: request.Bio
                    );

                    AdminCreateArtistResult result = await dispatcher.Send(request: command);

                    var response = new AdminCreateArtistResponse(Artist: result.Artist);
                    Guid artistId = response.Artist.Id;

                    string path = $"{ContentConstants.Admin}/{EditorialRouteConstants.Artists}/{artistId}";
                    string locationUrl = ApiVersionUrl.Build(context: httpContext, path: path);

                    return Results.Created(uri: locationUrl, value: response);
                }
            )
            .WithName(endpointName: AdminCreateArtistMetaField.CreateArtist.Name)
            .WithSummary(summary: AdminCreateArtistMetaField.CreateArtist.Summary)
            .WithDescription(description: AdminCreateArtistMetaField.CreateArtist.Description)
            .WithAuthorization(AccountStatusPolicies.RequireActiveUser)
            .WithAuthorization(UserRolePolicies.RequireSuperAdminOnly)
            .RequireRateLimiting(policyName: RateLimitPolicies.ContentBrowsing)
            .ProducesValidationProblem()
            .Produces<AdminCreateArtistResponse>(statusCode: StatusCodes.Status201Created)
            .ProducesProblem(statusCode: StatusCodes.Status400BadRequest)
            .ProducesProblem(statusCode: StatusCodes.Status401Unauthorized)
            .ProducesProblem(statusCode: StatusCodes.Status403Forbidden)
            .ProducesProblem(statusCode: StatusCodes.Status409Conflict)
            .ProducesProblem(statusCode: StatusCodes.Status429TooManyRequests);
    }
}
