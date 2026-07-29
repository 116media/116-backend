using _116.BuildingBlocks.Constants.RateLimit;
using _116.Mailer.Domain.Constants;
using _116.Shared.Application.Extensions;
using _116.Shared.Contracts.Application.CQRS;
using Carter;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace _116.Mailer.Application.Newsletter.UseCases.Public.Commands.SubscribeNewsletter.V1;

/// <summary>
/// Request model for the newsletter subscription use-case.
/// </summary>
/// <param name="Email">The email address to subscribe.</param>
public record PublicSubscribeNewsletterRequest(string Email);

/// <summary>
/// Response model for the newsletter subscription use-case.
/// </summary>
/// <param name="IsSuccess">Always true, to prevent subscriber enumeration.</param>
/// <param name="Email">The email address from the request for client reference.</param>
public record PublicSubscribeNewsletterResponse(bool IsSuccess, string Email);

/// <summary>
/// Defines the public newsletter subscription endpoint.
/// </summary>
public class PublicSubscribeNewsletterEndpointV1 : ICarterModule
{
    /// <summary>
    /// Configures the newsletter subscription route within the API pipeline.
    /// Maps the <c>POST /api/v1/public/newsletter/subscriptions</c> endpoint.
    /// </summary>
    /// <param name="app">The route builder used to register API endpoints.</param>
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        RouteGroupBuilder group = app.MapApiVersionGroup(1)
            .MapGroup($"{MailerConstants.Public}/{MailerConstants.NewsletterRoute}")
            .WithTags($"{MailerConstants.Public}::{MailerConstants.NewsletterRoute}");

        group
            .MapPost(
                pattern: "subscriptions",
                async (PublicSubscribeNewsletterRequest request, IDispatcher dispatcher) =>
                {
                    var command = new PublicSubscribeNewsletterCommand(Email: request.Email);
                    PublicSubscribeNewsletterResult result = await dispatcher.Send(request: command);

                    var response = new PublicSubscribeNewsletterResponse(
                        IsSuccess: result.IsSuccess,
                        Email: result.Email
                    );

                    return Results.Accepted(uri: null, value: response);
                }
            )
            .WithName(endpointName: PublicSubscribeNewsletterMetaField.SubscribeNewsletter.Name)
            .WithSummary(summary: PublicSubscribeNewsletterMetaField.SubscribeNewsletter.Summary)
            .WithDescription(description: PublicSubscribeNewsletterMetaField.SubscribeNewsletter.Description)
            .AllowAnonymous()
            .RequireRateLimiting(policyName: RateLimitPolicies.Otp)
            .ProducesValidationProblem()
            .Produces<PublicSubscribeNewsletterResponse>(statusCode: StatusCodes.Status202Accepted)
            .ProducesProblem(statusCode: StatusCodes.Status429TooManyRequests)
            .ProducesProblem(statusCode: StatusCodes.Status400BadRequest);
    }
}
