using _116.Identity.Application.Shared.Constants;
using _116.Identity.Domain.Constants;
using _116.Shared.Application.Extensions;
using _116.Shared.Contracts.Application.CQRS;

using Carter;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace _116.Identity.Application.Auth.Admin.UseCases.Commands.ForgotPassword.V1;

/// <summary>
/// Request model for admin forgot password.
/// </summary>
/// <param name="Email">The admin user's registered email address.</param>
public record AdminForgotPasswordRequest(
    string Email
);
/// <summary>
/// Response model for admin forgot password.
/// </summary>
/// <param name="IsSuccess">Always true for security reasons to prevent user enumeration.</param>
/// <param name="Email">The email address from the request for client reference.</param>
public record AdminForgotPasswordResponse(
    bool IsSuccess,
    string Email
);
/// <summary>
/// Defines the forgot password endpoint for initiating admin password reset.
/// </summary>
public class AdminForgotPasswordEndpointV1 : ICarterModule
{
    /// <summary>
    /// Configures the admin forgot password route within the API pipeline.
    /// </summary>
    /// <param name="app">The route builder used to register API endpoints.</param>
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        RouteGroupBuilder group = app
            .MapApiVersionGroup(version: 1)
            .MapGroup($"{IdentityConstants.Admin}/{AuthRouteConstants.Endpoint}")
            .WithTags($"{IdentityConstants.Admin}::{IdentityConstants.SchemaName}");
        group.MapPost(AuthRouteConstants.ForgotPassword, async (
                AdminForgotPasswordRequest request,
                IDispatcher dispatcher
            ) =>
            {
                var command = new AdminForgotPasswordCommand(request.Email);
                AdminForgotPasswordResult result = await dispatcher.Send(command);
                var response = new AdminForgotPasswordResponse(result.IsSuccess, request.Email);
                return Results.Ok(response);
            })
            .WithName(AdminForgotPasswordMetaField.ForgotPassword.Name)
            .WithSummary(AdminForgotPasswordMetaField.ForgotPassword.Summary)
            .WithDescription(AdminForgotPasswordMetaField.ForgotPassword.Description)
            .AllowAnonymous()
            .ProducesValidationProblem()
            .Produces<AdminForgotPasswordResponse>()
            .ProducesProblem(StatusCodes.Status400BadRequest);
    }
}
