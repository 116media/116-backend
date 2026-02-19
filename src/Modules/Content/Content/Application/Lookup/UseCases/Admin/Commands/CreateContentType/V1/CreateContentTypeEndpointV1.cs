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
public record CreateContentTypeRequest(string Name);

/// <summary>
/// Response model for successful content type creation.
/// </summary>
/// <param name="ContentType">The created content type information.</param>
public record CreateContentTypeResponse(ContentTypeDto ContentType);

/// <summary>
/// Defines the admin create content type endpoint.
/// Handles creation of new content types (e.g., "Article", "Video").
/// </summary>
public class CreateContentTypeEndpointV1 : ICarterModule
{
    /// <inheritdoc />
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        RouteGroupBuilder group = app.MapApiVersionGroup(1)
            .MapGroup($"{ContentConstants.Admin}/{LookupRouteConstants.ContentTypes}")
            .WithTags($"{ContentConstants.Admin}::{LookupRouteConstants.ContentTypes}");

        group
            .MapPost(
                "/",
                async (CreateContentTypeRequest request, IDispatcher dispatcher, HttpContext httpContext) =>
                {
                    var command = new CreateContentTypeCommand(Name: request.Name);

                    CreateContentTypeResult result = await dispatcher.Send(request: command);

                    var response = new CreateContentTypeResponse(ContentType: result.ContentType);

                    string path =
                        $"{ContentConstants.Admin}/{LookupRouteConstants.ContentTypes}/{response.ContentType.Id}";
                    string locationUrl = ApiVersionUrl.Build(context: httpContext, path: path);

                    return Results.Created(uri: locationUrl, value: response);
                }
            )
            .WithName(endpointName: CreateContentTypeMetaField.CreateContentType.Name)
            .WithSummary(summary: CreateContentTypeMetaField.CreateContentType.Summary)
            .WithDescription(description: CreateContentTypeMetaField.CreateContentType.Description)
            .RequireAuthorization()
            .RequireRateLimiting(policyName: RateLimitPolicies.ContentBrowsing)
            .ProducesValidationProblem()
            .Produces<CreateContentTypeResponse>(statusCode: StatusCodes.Status201Created)
            .ProducesProblem(statusCode: StatusCodes.Status400BadRequest)
            .ProducesProblem(statusCode: StatusCodes.Status401Unauthorized)
            .ProducesProblem(statusCode: StatusCodes.Status403Forbidden)
            .ProducesProblem(statusCode: StatusCodes.Status409Conflict)
            .ProducesProblem(statusCode: StatusCodes.Status429TooManyRequests);
    }
}
