using _116.BuildingBlocks.Constants.RateLimit;
using _116.Content.Application.Interactions.Constants;
using _116.Content.Domain.Constants;
using _116.Shared.Application.Extensions;
using _116.Shared.Contracts.Application.CQRS;
using Carter;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace _116.Content.Application.Interactions.UseCases.Public.Commands.RecordShortVideoView.V1;

/// <summary>
/// Response model for a successful PublicRecordShortVideoView operation.
/// </summary>
/// <param name="IsSuccess">Indicates if the operation was successful.</param>
public record PublicRecordShortVideoViewResponse(bool IsSuccess);

/// <summary>
/// Defines the record short video view endpoint.
/// </summary>
public class PublicRecordShortVideoViewEndpointV1 : ICarterModule
{
    /// <inheritdoc />
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        RouteGroupBuilder group = app.MapApiVersionGroup(1)
            .MapGroup($"{ContentConstants.Public}/{InteractionsRouteConstants.Shorts}")
            .WithTags($"{ContentConstants.Public}::{InteractionsRouteConstants.Shorts}");

        group
            .MapPost(
                $"/{{id}}/{InteractionsRouteConstants.Views}",
                async (string id, IDispatcher dispatcher) =>
                {
                    Guid shortVideoId = Guid.Parse(id);
                    var command = new PublicRecordShortVideoViewCommand(ShortVideoId: shortVideoId);

                    PublicRecordShortVideoViewResult result = await dispatcher.Send(request: command);

                    var response = new PublicRecordShortVideoViewResponse(IsSuccess: result.IsSuccess);
                    return Results.Ok(response);
                }
            )
            .WithName(endpointName: PublicRecordShortVideoViewMetaField.PublicRecordShortVideoView.Name)
            .WithSummary(summary: PublicRecordShortVideoViewMetaField.PublicRecordShortVideoView.Summary)
            .WithDescription(description: PublicRecordShortVideoViewMetaField.PublicRecordShortVideoView.Description)
            .AllowAnonymous()
            .RequireRateLimiting(policyName: RateLimitPolicies.ContentBrowsing)
            .Produces<PublicRecordShortVideoViewResponse>(statusCode: StatusCodes.Status200OK)
            .ProducesProblem(statusCode: StatusCodes.Status404NotFound)
            .ProducesProblem(statusCode: StatusCodes.Status429TooManyRequests);
    }
}
