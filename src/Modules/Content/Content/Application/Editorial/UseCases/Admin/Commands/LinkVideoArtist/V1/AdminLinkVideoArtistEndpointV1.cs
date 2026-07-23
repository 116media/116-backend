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

namespace _116.Content.Application.Editorial.UseCases.Admin.Commands.LinkVideoArtist.V1;

/// <summary>
/// Request model for linking or unlinking a video's artist profile.
/// </summary>
/// <param name="ArtistId">The artist profile to link, or null to unlink.</param>
public record AdminLinkVideoArtistRequest(Guid? ArtistId);

/// <summary>
/// Response model for a successful LinkVideoArtist operation.
/// </summary>
/// <param name="IsSuccess">Indicates if the operation was successful.</param>
public record AdminLinkVideoArtistResponse(bool IsSuccess);

/// <summary>
/// Defines the admin link video artist endpoint.
/// Handles linking or unlinking a video's real, addressable artist profile.
/// </summary>
public class AdminLinkVideoArtistEndpointV1 : ICarterModule
{
    /// <summary>
    /// Configures the video artist link route within the API pipeline.
    /// Maps the <c>PUT /api/v1/admin/videos/{id}/artist</c> endpoint.
    /// </summary>
    /// <param name="app">The route builder used to register API endpoints.</param>
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        RouteGroupBuilder group = app.MapApiVersionGroup(1)
            .MapGroup($"{ContentConstants.Admin}/{EditorialRouteConstants.Videos}")
            .WithTags($"{ContentConstants.Admin}::{EditorialRouteConstants.Videos}");

        group
            .MapPut(
                $"/{{id}}/{EditorialRouteConstants.Artist}",
                async (Guid id, AdminLinkVideoArtistRequest request, IDispatcher dispatcher) =>
                {
                    var command = new AdminLinkVideoArtistCommand(VideoId: id, ArtistId: request.ArtistId);
                    AdminLinkVideoArtistResult result = await dispatcher.Send(request: command);

                    var response = new AdminLinkVideoArtistResponse(IsSuccess: result.IsSuccess);
                    return Results.Ok(response);
                }
            )
            .WithName(endpointName: AdminLinkVideoArtistMetaField.LinkVideoArtist.Name)
            .WithSummary(summary: AdminLinkVideoArtistMetaField.LinkVideoArtist.Summary)
            .WithDescription(description: AdminLinkVideoArtistMetaField.LinkVideoArtist.Description)
            .WithAuthorization(AccountStatusPolicies.RequireActiveUser)
            .WithAuthorization(UserRolePolicies.RequireAdminOrSuperAdmin)
            .RequireRateLimiting(policyName: RateLimitPolicies.ContentBrowsing)
            .Produces<AdminLinkVideoArtistResponse>(statusCode: StatusCodes.Status200OK)
            .ProducesProblem(statusCode: StatusCodes.Status401Unauthorized)
            .ProducesProblem(statusCode: StatusCodes.Status403Forbidden)
            .ProducesProblem(statusCode: StatusCodes.Status404NotFound)
            .ProducesProblem(statusCode: StatusCodes.Status429TooManyRequests);
    }
}
