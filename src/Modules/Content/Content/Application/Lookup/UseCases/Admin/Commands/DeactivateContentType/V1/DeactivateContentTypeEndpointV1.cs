using _116.BuildingBlocks.Constants.Authorization.Policies;
using _116.BuildingBlocks.Constants.RateLimit;
using _116.Content.Application.Lookup.Constants;
using _116.Content.Application.Shared.DTOs;
using _116.Content.Domain.Constants;
using _116.Shared.Application.Extensions;
using _116.Shared.Contracts.Application.CQRS;
using Carter;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace _116.Content.Application.Lookup.UseCases.Admin.Commands.DeactivateContentType.V1;

/// <summary>Response model for a successful content type deactivation.</summary>
/// <param name="ContentType">The updated content type information.</param>
public record DeactivateContentTypeResponse(ContentTypeDto ContentType);

/// <summary>
/// Defines the admin deactivate content type endpoint.
/// </summary>
public class DeactivateContentTypeEndpointV1 : ICarterModule
{
    /// <inheritdoc />
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        RouteGroupBuilder group = app.MapApiVersionGroup(1)
            .MapGroup($"{ContentConstants.Admin}/{LookupRouteConstants.ContentTypes}")
            .WithTags($"{ContentConstants.Admin}::{LookupRouteConstants.ContentTypes}");

        group
            .MapPatch(
                $"/{{id:guid}}/{LookupRouteConstants.Deactivate}",
                async (Guid id, IDispatcher dispatcher) =>
                {
                    var command = new DeactivateContentTypeCommand(Id: id);
                    DeactivateContentTypeResult result = await dispatcher.Send(request: command);
                    return Results.Ok(new DeactivateContentTypeResponse(ContentType: result.ContentType));
                }
            )
            .WithName(endpointName: DeactivateContentTypeMetaField.DeactivateContentType.Name)
            .WithSummary(summary: DeactivateContentTypeMetaField.DeactivateContentType.Summary)
            .WithDescription(description: DeactivateContentTypeMetaField.DeactivateContentType.Description)
            .RequireAuthorization(AccountStatusPolicies.RequireActiveUser)
            .RequireAuthorization(UserRolePolicies.RequireAdminOrSuperAdmin)
            .RequireRateLimiting(policyName: RateLimitPolicies.ContentBrowsing)
            .Produces<DeactivateContentTypeResponse>(statusCode: StatusCodes.Status200OK)
            .ProducesProblem(statusCode: StatusCodes.Status401Unauthorized)
            .ProducesProblem(statusCode: StatusCodes.Status403Forbidden)
            .ProducesProblem(statusCode: StatusCodes.Status404NotFound)
            .ProducesProblem(statusCode: StatusCodes.Status409Conflict)
            .ProducesProblem(statusCode: StatusCodes.Status429TooManyRequests);
    }
}
