using System.Security.Claims;
using _116.BuildingBlocks.Constants.Authorization.Policies;
using _116.BuildingBlocks.Constants.RateLimit;
using _116.Content.Application.Editorial.Constants;
using _116.Content.Domain.Constants;
using _116.Identity.Contracts.Application;
using _116.Shared.Application.Extensions;
using _116.Shared.Contracts.Application.CQRS;
using Carter;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace _116.Content.Application.Editorial.UseCases.Admin.Commands.DecideTranslationRevision.V1;

/// <summary>
/// Request model for a moderator decision on a translation revision.
/// </summary>
/// <param name="Accept"><c>true</c> to accept the revision, <c>false</c> to reject it.</param>
public record AdminDecideTranslationRevisionRequest(bool Accept);

/// <summary>
/// Response model for a successful DecideTranslationRevision operation.
/// </summary>
/// <param name="IsSuccess">Indicates if the operation was successful.</param>
public record AdminDecideTranslationRevisionResponse(bool IsSuccess);

/// <summary>
/// Defines the admin decide translation revision endpoint.
/// </summary>
public class AdminDecideTranslationRevisionEndpointV1 : ICarterModule
{
    /// <summary>
    /// Configures the moderator translation revision decision route within the API pipeline.
    /// Maps the <c>PUT /api/v1/admin/translations/revisions/{id}</c> endpoint.
    /// </summary>
    /// <param name="app">The route builder used to register API endpoints.</param>
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        RouteGroupBuilder group = app.MapApiVersionGroup(1)
            .MapGroup($"{ContentConstants.Admin}/{EditorialRouteConstants.Translations}")
            .WithTags($"{ContentConstants.Admin}::{EditorialRouteConstants.Translations}");

        group
            .MapPut(
                $"/{EditorialRouteConstants.Revisions}/{{id}}",
                async (
                    Guid id,
                    AdminDecideTranslationRevisionRequest request,
                    ClaimsPrincipal user,
                    IClaimsProvider claimsProvider,
                    IDispatcher dispatcher
                ) =>
                {
                    Guid adminUserId = claimsProvider.GetUserIdFromClaims(user: user);

                    var command = new AdminDecideTranslationRevisionCommand(
                        Id: id,
                        Accept: request.Accept,
                        DecidedByUserId: adminUserId
                    );
                    AdminDecideTranslationRevisionResult result = await dispatcher.Send(request: command);

                    var response = new AdminDecideTranslationRevisionResponse(IsSuccess: result.IsSuccess);
                    return Results.Ok(response);
                }
            )
            .WithName(endpointName: AdminDecideTranslationRevisionMetaField.DecideTranslationRevision.Name)
            .WithSummary(summary: AdminDecideTranslationRevisionMetaField.DecideTranslationRevision.Summary)
            .WithDescription(description: AdminDecideTranslationRevisionMetaField.DecideTranslationRevision.Description)
            .WithAuthorization(AccountStatusPolicies.RequireActiveUser)
            .WithAuthorization(UserRolePolicies.RequireAdminOrSuperAdmin)
            .RequireRateLimiting(policyName: RateLimitPolicies.ContentContribution)
            .Produces<AdminDecideTranslationRevisionResponse>(statusCode: StatusCodes.Status200OK)
            .ProducesProblem(statusCode: StatusCodes.Status401Unauthorized)
            .ProducesProblem(statusCode: StatusCodes.Status403Forbidden)
            .ProducesProblem(statusCode: StatusCodes.Status404NotFound)
            .ProducesProblem(statusCode: StatusCodes.Status429TooManyRequests);
    }
}
