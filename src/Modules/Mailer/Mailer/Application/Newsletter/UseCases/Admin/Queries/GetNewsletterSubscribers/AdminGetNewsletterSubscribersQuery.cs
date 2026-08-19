using _116.Mailer.Application.Shared.DTOs;
using _116.Mailer.Domain.Enums;
using _116.Shared.Application.Pagination;
using _116.Shared.Contracts.Application.CQRS;

namespace _116.Mailer.Application.Newsletter.UseCases.Admin.Queries.GetNewsletterSubscribers;

/// <summary>
/// Query for the paginated admin list of newsletter subscribers.
/// </summary>
/// <param name="PageIndex">The zero-based page index.</param>
/// <param name="PageSize">The page size.</param>
/// <param name="Status">Optional status filter.</param>
public record AdminGetNewsletterSubscribersQuery(int PageIndex, int PageSize, EnumNewsletterStatus? Status)
    : IQuery<AdminGetNewsletterSubscribersResult>;

/// <summary>
/// Result of the <see cref="AdminGetNewsletterSubscribersQuery" />.
/// </summary>
/// <param name="Subscribers">The paginated subscribers, newest first.</param>
public record AdminGetNewsletterSubscribersResult(PaginatedResult<NewsletterSubscriberDto> Subscribers);
