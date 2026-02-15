using _116.Identity.Application.Session.Repositories;
using _116.Identity.Application.Shared.DTOs;
using _116.Identity.Application.Shared.Mappers;
using _116.Shared.Application.Pagination;
using _116.Shared.Contracts.Application.CQRS;
using MapsterMapper;

namespace _116.Identity.Application.Session.UseCases.Admin.Queries.GetAllSessions;

/// <summary>
/// Handles the <see cref="AdminGetAllSessionsQuery" /> to retrieve all sessions with pagination and filtering.
/// </summary>
/// <param name="sessionRepository">Repository for session data access operations.</param>
/// <param name="mapper">Mapster mapper for entity-to-DTO transformations.</param>
public class AdminGetAllSessionsHandler(ISessionRepository sessionRepository, IMapper mapper)
    : IQueryHandler<AdminGetAllSessionsQuery, AdminGetAllSessionsResult>
{
    /// <summary>
    /// Handles the query by fetching sessions with pagination and optional filters.
    /// </summary>
    /// <param name="query">The query containing pagination and filter parameters.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>A <see cref="AdminGetAllSessionsResult" /> containing paginated session DTOs.</returns>
    public async Task<AdminGetAllSessionsResult> Handle(
        AdminGetAllSessionsQuery query,
        CancellationToken cancellationToken
    )
    {
        int pageSize = query.PaginatedRequest.PageSize;
        int pageIndex = query.PaginatedRequest.PageIndex;

        Guid? userIdFilter = null;
        if (
            !string.IsNullOrWhiteSpace(value: query.UserId) && Guid.TryParse(input: query.UserId, out Guid parsedUserId)
        )
        {
            userIdFilter = parsedUserId;
        }

        var (sessions, totalCount) = await sessionRepository.GetAllWithPaginationAsync(
            pageIndex + 1,
            pageSize: pageSize,
            status: query.Status,
            userId: userIdFilter,
            ipAddress: query.IpAddress,
            fromDate: query.FromDate,
            toDate: query.ToDate,
            cancellationToken: cancellationToken
        );

        List<SessionDto> sessionDtos = sessions.Select(s => s.ToSessionDto(mapper)).ToList();

        var paginatedResult = new PaginatedResult<SessionDto>(
            pageIndex: pageIndex,
            pageSize: pageSize,
            count: totalCount,
            items: sessionDtos
        );

        return new AdminGetAllSessionsResult(Sessions: paginatedResult);
    }
}
