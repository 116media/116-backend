using _116.BuildingBlocks.Constants.Authorization.Policies;
using _116.BuildingBlocks.Constants.RateLimit;
using _116.BuildingBlocks.Utils;
using _116.Content.Application.Lookup.Constants;
using _116.Content.Application.Shared.DTOs;
using _116.Content.Domain.Constants;
using _116.Shared.Application.Extensions;
using _116.Shared.Contracts.Application.CQRS;
using Carter;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace _116.Content.Application.Lookup.UseCases.Admin.Commands.CreateContentType.V1;

/// <summary>
/// Request model for creating a content type.
/// </summary>
/// <param name="Name">The display name of the content type.</param>
public record AdminCreateContentTypeRequest(string Name);

/// <summary>
/// Response model for successful content type creation.
/// </summary>
/// <param name="ContentType">The created content type information.</param>
public record AdminCreateContentTypeResponse(ContentTypeDto ContentType);

/// <summary>
/// Defines the admin create content type endpoint.
/// Handles creation of new content types (e.g., "Article", "Video").
/// </summary>
public class AdminCreateContentTypeEndpointV1 : ICarterModule
{
    /// <summary>
    /// Configures the content type creation route within the API pipeline.
    /// Maps the <c>POST /api/v1/admin/content-types</c> endpoint to handle content type creation requests.
    /// </summary>
    /// <param name="app">The route builder used to register API endpoints.</param>
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        RouteGroupBuilder group = app.MapApiVersionGroup(1)
            .MapGroup($"{ContentConstants.Admin}/{LookupRouteConstants.ContentTypes}")
            .WithTags($"{ContentConstants.Admin}::{LookupRouteConstants.ContentTypes}");

        group
            .MapPost(
                "/",
                async (AdminCreateContentTypeRequest request, IDispatcher dispatcher, HttpContext httpContext) =>
                {
                    var command = new AdminCreateContentTypeCommand(Name: request.Name);

                    AdminCreateContentTypeResult result = await dispatcher.Send(request: command);

                    var response = new AdminCreateContentTypeResponse(ContentType: result.ContentType);
                    Guid contentTypeId = response.ContentType.Id;

                    string path = $"{ContentConstants.Admin}/{LookupRouteConstants.ContentTypes}/{contentTypeId}";
                    string locationUrl = ApiVersionUrl.Build(context: httpContext, path: path);

                    return Results.Created(uri: locationUrl, value: response);
                }
            )
            .WithName(endpointName: AdminCreateContentTypeMetaField.CreateContentType.Name)
            .WithSummary(summary: AdminCreateContentTypeMetaField.CreateContentType.Summary)
            .WithDescription(description: AdminCreateContentTypeMetaField.CreateContentType.Description)
            .WithAuthorization(AccountStatusPolicies.RequireActiveUser)
            .WithAuthorization(UserRolePolicies.RequireAdminOrSuperAdmin)
            .RequireRateLimiting(policyName: RateLimitPolicies.ContentBrowsing)
            .ProducesValidationProblem()
            .Produces<AdminCreateContentTypeResponse>(statusCode: StatusCodes.Status201Created)
            .ProducesProblem(statusCode: StatusCodes.Status400BadRequest)
            .ProducesProblem(statusCode: StatusCodes.Status401Unauthorized)
            .ProducesProblem(statusCode: StatusCodes.Status403Forbidden)
            .ProducesProblem(statusCode: StatusCodes.Status409Conflict)
            .ProducesProblem(statusCode: StatusCodes.Status429TooManyRequests);
    }
}
