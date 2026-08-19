using _116.BuildingBlocks.Constants.Authorization.Policies;
using _116.BuildingBlocks.Constants.RateLimit;
using _116.Mailer.Application.Shared.DTOs;
using _116.Mailer.Domain.Constants;
using _116.Mailer.Domain.Enums;
using _116.Shared.Application.Extensions;
using _116.Shared.Application.Pagination;
using _116.Shared.Contracts.Application.CQRS;
using Carter;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace _116.Mailer.Application.Newsletter.UseCases.Admin.Queries.GetNewsletterSubscribers.V1;

/// <summary>
/// Response model for the admin newsletter subscribers listing.
/// </summary>
/// <param name="Subscribers">The paginated subscribers, newest first.</param>
public record AdminGetNewsletterSubscribersResponse(PaginatedResult<NewsletterSubscriberDto> Subscribers);

/// <summary>
/// Defines the admin newsletter subscribers listing endpoint.
/// </summary>
public class AdminGetNewsletterSubscribersEndpointV1 : ICarterModule
{
    /// <summary>
    /// Configures the admin subscribers route within the API pipeline.
    /// Maps the <c>GET /api/v1/admin/newsletter/subscribers</c> endpoint.
    /// </summary>
    /// <param name="app">The route builder used to register API endpoints.</param>
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        RouteGroupBuilder group = app.MapApiVersionGroup(1)
            .MapGroup($"{MailerConstants.Admin}/{MailerConstants.NewsletterRoute}")
            .WithTags($"{MailerConstants.Admin}::{MailerConstants.NewsletterRoute}");

        group
            .MapGet(
                pattern: "subscribers",
                async (
                    IDispatcher dispatcher,
                    int pageIndex = 0,
                    int pageSize = 20,
                    EnumNewsletterStatus? status = null
                ) =>
                {
                    var query = new AdminGetNewsletterSubscribersQuery(
                        PageIndex: int.Max(pageIndex, 0),
                        PageSize: int.Clamp(pageSize, 1, 100),
                        Status: status
                    );

                    AdminGetNewsletterSubscribersResult result = await dispatcher.Send(request: query);

                    var response = new AdminGetNewsletterSubscribersResponse(Subscribers: result.Subscribers);

                    return Results.Ok(value: response);
                }
            )
            .WithName(endpointName: AdminGetNewsletterSubscribersMetaField.GetNewsletterSubscribers.Name)
            .WithSummary(summary: AdminGetNewsletterSubscribersMetaField.GetNewsletterSubscribers.Summary)
            .WithDescription(description: AdminGetNewsletterSubscribersMetaField.GetNewsletterSubscribers.Description)
            .WithAuthorization(AccountStatusPolicies.RequireActiveUser)
            .WithAuthorization(UserRolePolicies.RequireAdminOrSuperAdmin)
            .RequireRateLimiting(policyName: RateLimitPolicies.AdminMetrics)
            .Produces<AdminGetNewsletterSubscribersResponse>()
            .ProducesProblem(statusCode: StatusCodes.Status401Unauthorized)
            .ProducesProblem(statusCode: StatusCodes.Status403Forbidden);
    }
}
