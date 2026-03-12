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

namespace _116.Content.Application.Lookup.UseCases.Admin.Commands.UpdateContentType.V1;

/// <summary>
/// Request model for updating a content type.
/// </summary>
/// <param name="Name">The new name for the content type.</param>
public record UpdateContentTypeRequest(string Name);

/// <summary>
/// Response model for a successful content type update.
/// </summary>
/// <param name="ContentType">The updated content type information.</param>
public record UpdateContentTypeResponse(ContentTypeDto ContentType);

/// <summary>
/// Defines the admin update content type endpoint.
/// Handles renaming an existing content type.
/// </summary>
public class UpdateContentTypeEndpointV1 : ICarterModule
{
    /// <summary>
    /// Configures the content type update route within the API pipeline.
    /// Maps the <c>PUT /api/v1/admin/content-types/{id:guid}</c> endpoint to handle content type update requests.
    /// </summary>
    /// <param name="app">The route builder used to register API endpoints.</param>
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        RouteGroupBuilder group = app.MapApiVersionGroup(1)
            .MapGroup($"{ContentConstants.Admin}/{LookupRouteConstants.ContentTypes}")
            .WithTags($"{ContentConstants.Admin}::{LookupRouteConstants.ContentTypes}");

        group
            .MapPut(
                "/{id}",
                async (string id, UpdateContentTypeRequest request, IDispatcher dispatcher) =>
                {
                    var command = new UpdateContentTypeCommand(Id: id, Name: request.Name);

                    UpdateContentTypeResult result = await dispatcher.Send(request: command);

                    var response = new UpdateContentTypeResponse(ContentType: result.ContentType);
                    return Results.Ok(response);
                }
            )
            .WithName(endpointName: UpdateContentTypeMetaField.UpdateContentType.Name)
            .WithSummary(summary: UpdateContentTypeMetaField.UpdateContentType.Summary)
            .WithDescription(description: UpdateContentTypeMetaField.UpdateContentType.Description)
            .WithAuthorization(AccountStatusPolicies.RequireActiveUser)
            .WithAuthorization(UserRolePolicies.RequireSuperAdminOnly)
            .RequireRateLimiting(policyName: RateLimitPolicies.ContentBrowsing)
            .ProducesValidationProblem()
            .Produces<UpdateContentTypeResponse>(statusCode: StatusCodes.Status200OK)
            .ProducesProblem(statusCode: StatusCodes.Status400BadRequest)
            .ProducesProblem(statusCode: StatusCodes.Status401Unauthorized)
            .ProducesProblem(statusCode: StatusCodes.Status403Forbidden)
            .ProducesProblem(statusCode: StatusCodes.Status404NotFound)
            .ProducesProblem(statusCode: StatusCodes.Status409Conflict)
            .ProducesProblem(statusCode: StatusCodes.Status429TooManyRequests);
    }
}
