using System.Security.Claims;
using _116.Identity.Application.Auth.Constants;
using _116.Identity.Application.Shared.Authorizations.Policies;
using _116.Identity.Application.Shared.Repositories;
using _116.Identity.Domain.Constants;
using _116.Shared.Application.Extensions;
using _116.Shared.Contracts.Application.CQRS;
using Carter;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace _116.Identity.Application.Auth.UseCases.Public.Commands.SignOut.V1;

/// <summary>
/// Request model for sign-out (RFC 7009 compliant).
/// </summary>
/// <param name="RefreshToken">The refresh token to revoke.</param>
public record PublicSignOutRequest(string RefreshToken);

/// <summary>
/// Response model for sign-out.
/// </summary>
/// <param name="IsSuccess">Indicates if the sign-out operation was successful.</param>
public record PublicSignOutResponse(bool IsSuccess);

/// <summary>
/// Defines the sign-out endpoint for authenticated public users.
/// </summary>
public class PublicSignOutEndpointV1 : ICarterModule
{
    /// <summary>
    /// Configures the sign-out route within the API pipeline.
    /// </summary>
    /// <param name="app">The route builder used to register API endpoints.</param>
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        RouteGroupBuilder group = app.MapApiVersionGroup(1)
            .MapGroup($"{IdentityConstants.Public}/{AuthRouteConstants.Endpoint}")
            .WithTags($"{IdentityConstants.Public}::{IdentityConstants.SchemaName}");

        group
            .MapPost(
                pattern: AuthRouteConstants.SignOut,
                async (
                    PublicSignOutRequest request,
                    ClaimsPrincipal user,
                    IAuthRepository authRepository,
                    IDispatcher dispatcher
                ) =>
                {
                    Guid userId = authRepository.GetUserIdFromClaims(user: user);

                    var command = new PublicSignOutCommand(UserId: userId, RefreshToken: request.RefreshToken);
                    PublicSignOutResult result = await dispatcher.Send(request: command);

                    var response = new PublicSignOutResponse(IsSuccess: result.IsSuccess);

                    return Results.Ok(value: response);
                }
            )
            .WithName(endpointName: PublicSignOutMetaField.SignOut.Name)
            .WithSummary(summary: PublicSignOutMetaField.SignOut.Summary)
            .WithDescription(description: PublicSignOutMetaField.SignOut.Description)
            .RequireAuthorization(UserRolePolicies.RequireVisitorOnly)
            .Produces<PublicSignOutResponse>()
            .ProducesProblem(statusCode: StatusCodes.Status401Unauthorized)
            .ProducesProblem(statusCode: StatusCodes.Status403Forbidden);
    }
}
