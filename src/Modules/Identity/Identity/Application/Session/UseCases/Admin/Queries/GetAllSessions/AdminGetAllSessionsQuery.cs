using _116.Identity.Application.Shared.DTOs;
using _116.Shared.Application.Pagination;
using _116.Shared.Contracts.Application.CQRS;

namespace _116.Identity.Application.Session.UseCases.Admin.Queries.GetAllSessions;

/// <summary>
/// Query used to retrieve all sessions with pagination and filtering (admin only).
/// </summary>
public record AdminGetAllSessionsQuery(
    PaginatedRequest PaginatedRequest,
    string? Status = null,
    string? UserId = null,
    string? IpAddress = null,
    DateTime? FromDate = null,
    DateTime? ToDate = null
) : IQuery<AdminGetAllSessionsResult>;

/// <summary>
/// The result of executing an <see cref="AdminGetAllSessionsQuery" />.
/// </summary>
/// <param name="Sessions">Paginated result containing session DTOs.</param>
public record AdminGetAllSessionsResult(PaginatedResult<SessionDto> Sessions);
