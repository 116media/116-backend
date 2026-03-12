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

namespace _116.Content.Application.Lookup.UseCases.Admin.Commands.ActivateContentType.V1;

/// <summary>
/// Response model for a successful content type activation.
/// </summary>
/// <param name="ContentType">The updated content type information.</param>
public record ActivateContentTypeResponse(ContentTypeDto ContentType);

/// <summary>
/// Defines the admin activate content type endpoint.
/// </summary>
public class ActivateContentTypeEndpointV1 : ICarterModule
{
    /// <summary>
    /// Configures the content type activation route within the API pipeline.
    /// Maps the <c>PATCH /api/v1/admin/content-types/{id:guid}/activate</c> endpoint to handle content type activation requests.
    /// </summary>
    /// <param name="app">The route builder used to register API endpoints.</param>
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        RouteGroupBuilder group = app.MapApiVersionGroup(1)
            .MapGroup($"{ContentConstants.Admin}/{LookupRouteConstants.ContentTypes}")
            .WithTags($"{ContentConstants.Admin}::{LookupRouteConstants.ContentTypes}");

        group
            .MapPatch(
                $"/{{id}}/{LookupRouteConstants.Activate}",
                async (string id, IDispatcher dispatcher) =>
                {
                    var command = new ActivateContentTypeCommand(Id: id);
                    ActivateContentTypeResult result = await dispatcher.Send(request: command);
                    return Results.Ok(new ActivateContentTypeResponse(ContentType: result.ContentType));
                }
            )
            .WithName(endpointName: ActivateContentTypeMetaField.ActivateContentType.Name)
            .WithSummary(summary: ActivateContentTypeMetaField.ActivateContentType.Summary)
            .WithDescription(description: ActivateContentTypeMetaField.ActivateContentType.Description)
            .WithAuthorization(AccountStatusPolicies.RequireActiveUser)
            .WithAuthorization(UserRolePolicies.RequireAdminOrSuperAdmin)
            .RequireRateLimiting(policyName: RateLimitPolicies.ContentBrowsing)
            .Produces<ActivateContentTypeResponse>(statusCode: StatusCodes.Status200OK)
            .ProducesProblem(statusCode: StatusCodes.Status401Unauthorized)
            .ProducesProblem(statusCode: StatusCodes.Status403Forbidden)
            .ProducesProblem(statusCode: StatusCodes.Status404NotFound)
            .ProducesProblem(statusCode: StatusCodes.Status409Conflict)
            .ProducesProblem(statusCode: StatusCodes.Status429TooManyRequests);
    }
}
