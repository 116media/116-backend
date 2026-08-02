using _116.BuildingBlocks.Constants.Authorization.Policies;
using _116.BuildingBlocks.Constants.RateLimit;
using _116.Content.Application.Editorial.Constants;
using _116.Content.Domain.Constants;
using _116.Shared.Application.Extensions;
using _116.Shared.Contracts.Application.CQRS;
using Carter;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace _116.Content.Application.Editorial.UseCases.Admin.Commands.UploadArtistAvatar.V1;

/// <summary>
/// Response model for successful artist avatar image upload.
/// </summary>
/// <param name="AvatarUrl">The publicly accessible URL of the uploaded avatar image.</param>
/// <param name="AvatarStorageKey">The provider-agnostic storage key for the avatar asset.</param>
public record AdminUploadArtistAvatarResponse(string AvatarUrl, string AvatarStorageKey);

/// <summary>
/// Defines the admin upload artist avatar endpoint.
/// Handles uploading or replacing an artist profile's avatar image.
/// </summary>
public class AdminUploadArtistAvatarEndpointV1 : ICarterModule
{
    /// <summary>
    /// Configures the artist avatar upload route within the API pipeline.
    /// Maps the <c>POST /api/v1/admin/artists/{id}/avatar</c> endpoint.
    /// </summary>
    /// <param name="app">The route builder used to register API endpoints.</param>
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        RouteGroupBuilder group = app.MapApiVersionGroup(1)
            .MapGroup($"{ContentConstants.Admin}/{EditorialRouteConstants.Artists}")
            .WithTags($"{ContentConstants.Admin}::{EditorialRouteConstants.Artists}");

        group
            .MapPost(
                $"/{{id}}/{EditorialRouteConstants.Avatar}",
                async (Guid id, IFormFile file, IDispatcher dispatcher) =>
                {
                    var command = new AdminUploadArtistAvatarCommand(ArtistId: id, File: file);
                    AdminUploadArtistAvatarResult result = await dispatcher.Send(request: command);

                    var response = new AdminUploadArtistAvatarResponse(
                        AvatarUrl: result.AvatarUrl,
                        AvatarStorageKey: result.AvatarStorageKey
                    );

                    return Results.Ok(response);
                }
            )
            .DisableAntiforgery()
            .WithName(endpointName: AdminUploadArtistAvatarMetaField.UploadArtistAvatar.Name)
            .WithSummary(summary: AdminUploadArtistAvatarMetaField.UploadArtistAvatar.Summary)
            .WithDescription(description: AdminUploadArtistAvatarMetaField.UploadArtistAvatar.Description)
            .WithAuthorization(AccountStatusPolicies.RequireActiveUser)
            .WithAuthorization(UserRolePolicies.RequireAdminOrSuperAdmin)
            .RequireRateLimiting(policyName: RateLimitPolicies.FileUpload)
            .ProducesValidationProblem()
            .Produces<AdminUploadArtistAvatarResponse>(statusCode: StatusCodes.Status200OK)
            .ProducesProblem(statusCode: StatusCodes.Status400BadRequest)
            .ProducesProblem(statusCode: StatusCodes.Status401Unauthorized)
            .ProducesProblem(statusCode: StatusCodes.Status403Forbidden)
            .ProducesProblem(statusCode: StatusCodes.Status404NotFound)
            .ProducesProblem(statusCode: StatusCodes.Status429TooManyRequests);
    }
}
