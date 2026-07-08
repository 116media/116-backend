using _116.BuildingBlocks.Constants.Authorization.Policies;
using _116.BuildingBlocks.Constants.RateLimit;
using _116.Content.Application.Commerce.Constants;
using _116.Content.Application.Shared.DTOs;
using _116.Content.Domain.Constants;
using _116.Content.Domain.Enums;
using _116.Shared.Application.Extensions;
using _116.Shared.Application.Pagination;
using _116.Shared.Contracts.Application.CQRS;
using Carter;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace _116.Content.Application.Commerce.UseCases.Admin.Queries.GetAllPayments.V1;

/// <summary>
/// Response model for listing all payments.
/// </summary>
/// <param name="Payments">Paginated result containing payment summary DTOs and pagination metadata.</param>
public record AdminGetAllPaymentsResponse(PaginatedResult<PaymentSummaryDto> Payments);

/// <summary>
/// Defines the admin get all payments endpoint.
/// </summary>
public class AdminGetAllPaymentsEndpointV1 : ICarterModule
{
    /// <inheritdoc />
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        RouteGroupBuilder group = app.MapApiVersionGroup(1)
            .MapGroup($"{ContentConstants.Admin}/{CommerceRouteConstants.Payments}")
            .WithTags($"{ContentConstants.Admin}::{CommerceRouteConstants.Payments}");

        group
            .MapGet(
                "/",
                async (
                    IDispatcher dispatcher,
                    int pageIndex = 0,
                    int pageSize = 10,
                    EnumPaymentStatus? status = null,
                    EnumPaymentMethod? method = null,
                    string? search = null
                ) =>
                {
                    var paginatedRequest = new PaginatedRequest(pageIndex, pageSize);
                    var query = new AdminGetAllPaymentsQuery(
                        PaginatedRequest: paginatedRequest,
                        Status: status,
                        Method: method,
                        Search: search
                    );

                    AdminGetAllPaymentsResult result = await dispatcher.Send(request: query);

                    var response = new AdminGetAllPaymentsResponse(Payments: result.Payments);
                    return Results.Ok(response);
                }
            )
            .WithName(endpointName: AdminGetAllPaymentsMetaField.GetAllPayments.Name)
            .WithSummary(summary: AdminGetAllPaymentsMetaField.GetAllPayments.Summary)
            .WithDescription(description: AdminGetAllPaymentsMetaField.GetAllPayments.Description)
            .WithAuthorization(AccountStatusPolicies.RequireActiveUser)
            .WithAuthorization(UserRolePolicies.RequireAdminOrSuperAdmin)
            .RequireRateLimiting(policyName: RateLimitPolicies.ContentBrowsing)
            .Produces<AdminGetAllPaymentsResponse>(statusCode: StatusCodes.Status200OK)
            .ProducesProblem(statusCode: StatusCodes.Status401Unauthorized)
            .ProducesProblem(statusCode: StatusCodes.Status403Forbidden)
            .ProducesProblem(statusCode: StatusCodes.Status429TooManyRequests);
    }
}
