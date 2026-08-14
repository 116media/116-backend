using _116.BuildingBlocks.Constants.Authorization.Policies;
using _116.BuildingBlocks.Constants.RateLimit;
using _116.Content.Application.Editorial.Constants;
using _116.Content.Domain.Constants;
using _116.Content.Domain.Enums;
using _116.Shared.Application.Extensions;
using _116.Shared.Contracts.Application.CQRS;
using Carter;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace _116.Content.Application.Editorial.UseCases.Admin.Commands.RemoveArtistSocialLink.V1;

/// <summary>
/// Response model for a successful RemoveArtistSocialLink operation.
/// </summary>
/// <param name="IsSuccess">Whether the link was removed.</param>
public record AdminRemoveArtistSocialLinkResponse(bool IsSuccess);

/// <summary>
/// Defines the admin remove artist social link endpoint.
/// Handles removing an artist's social link for a single platform.
/// </summary>
public class AdminRemoveArtistSocialLinkEndpointV1 : ICarterModule
{
    /// <summary>
    /// Configures the artist social link removal route within the API pipeline.
    /// Maps the <c>DELETE /api/v1/admin/artists/{id}/social-links/{platform}</c> endpoint.
    /// </summary>
    /// <param name="app">The route builder used to register API endpoints.</param>
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        RouteGroupBuilder group = app.MapApiVersionGroup(1)
            .MapGroup($"{ContentConstants.Admin}/{EditorialRouteConstants.Artists}")
            .WithTags($"{ContentConstants.Admin}::{EditorialRouteConstants.Artists}");

        group
            .MapDelete(
                $"/{{id}}/{EditorialRouteConstants.SocialLinks}/{{platform}}",
                async (Guid id, EnumSocialPlatform platform, IDispatcher dispatcher) =>
                {
                    var command = new AdminRemoveArtistSocialLinkCommand(ArtistId: id, Platform: platform);

                    AdminRemoveArtistSocialLinkResult result = await dispatcher.Send(request: command);

                    var response = new AdminRemoveArtistSocialLinkResponse(IsSuccess: result.IsSuccess);
                    return Results.Ok(response);
                }
            )
            .WithName(endpointName: AdminRemoveArtistSocialLinkMetaField.RemoveArtistSocialLink.Name)
            .WithSummary(summary: AdminRemoveArtistSocialLinkMetaField.RemoveArtistSocialLink.Summary)
            .WithDescription(description: AdminRemoveArtistSocialLinkMetaField.RemoveArtistSocialLink.Description)
            .WithAuthorization(AccountStatusPolicies.RequireActiveUser)
            .WithAuthorization(UserRolePolicies.RequireAdminOrSuperAdmin)
            .RequireRateLimiting(policyName: RateLimitPolicies.ContentBrowsing)
            .Produces<AdminRemoveArtistSocialLinkResponse>(statusCode: StatusCodes.Status200OK)
            .ProducesProblem(statusCode: StatusCodes.Status401Unauthorized)
            .ProducesProblem(statusCode: StatusCodes.Status403Forbidden)
            .ProducesProblem(statusCode: StatusCodes.Status404NotFound)
            .ProducesProblem(statusCode: StatusCodes.Status429TooManyRequests);
    }
}
