using _116.BuildingBlocks.Constants.Authorization.Policies;
using _116.BuildingBlocks.Constants.RateLimit;
using _116.Content.Application.Lookup.Constants;
using _116.Content.Domain.Constants;
using _116.Shared.Application.Extensions;
using _116.Shared.Contracts.Application.CQRS;
using Carter;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace _116.Content.Application.Lookup.UseCases.Admin.Commands.DeleteTag.V1;

/// <summary>
/// Defines the admin delete tag endpoint.
/// Handles permanent hard deletion of content discovery tags.
/// </summary>
public class AdminDeleteTagEndpointV1 : ICarterModule
{
    /// <summary>
    /// Configures the tag deletion route within the API pipeline.
    /// Maps the <c>DELETE /api/v1/admin/tags/{id}</c> endpoint.
    /// </summary>
    /// <param name="app">The route builder used to register API endpoints.</param>
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        RouteGroupBuilder group = app.MapApiVersionGroup(1)
            .MapGroup($"{ContentConstants.Admin}/{LookupRouteConstants.Tags}")
            .WithTags($"{ContentConstants.Admin}::{LookupRouteConstants.Tags}");

        group
            .MapDelete(
                "/{id}",
                async (string id, IDispatcher dispatcher, CancellationToken cancellationToken) =>
                {
                    var command = new AdminDeleteTagCommand(Id: id);
                    await dispatcher.Send(request: command, cancellationToken: cancellationToken);
                    return Results.NoContent();
                }
            )
            .WithName(endpointName: AdminDeleteTagMetaField.DeleteTag.Name)
            .WithSummary(summary: AdminDeleteTagMetaField.DeleteTag.Summary)
            .WithDescription(description: AdminDeleteTagMetaField.DeleteTag.Description)
            .WithAuthorization(AccountStatusPolicies.RequireActiveUser)
            .WithAuthorization(UserRolePolicies.RequireSuperAdminOnly)
            .RequireRateLimiting(policyName: RateLimitPolicies.ContentManagement)
            .Produces(statusCode: StatusCodes.Status204NoContent)
            .ProducesProblem(statusCode: StatusCodes.Status400BadRequest)
            .ProducesProblem(statusCode: StatusCodes.Status401Unauthorized)
            .ProducesProblem(statusCode: StatusCodes.Status403Forbidden)
            .ProducesProblem(statusCode: StatusCodes.Status404NotFound)
            .ProducesProblem(statusCode: StatusCodes.Status429TooManyRequests);
    }
}
