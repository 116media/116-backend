using _116.Mailer.Application.Shared.DTOs;
using _116.Mailer.Application.Shared.Mappers;
using _116.Mailer.Application.Shared.Repositories;
using _116.Mailer.Domain.Entities;
using _116.Shared.Application.Pagination;
using _116.Shared.Contracts.Application.CQRS;

namespace _116.Mailer.Application.Newsletter.UseCases.Admin.Queries.GetNewsletterSubscribers;

/// <summary>
/// Handles the <see cref="AdminGetNewsletterSubscribersQuery" /> by paging the
/// subscriber table, newest first, with an optional status filter.
/// </summary>
/// <param name="newsletterRepository">Repository for subscriber persistence.</param>
public class AdminGetNewsletterSubscribersHandler(INewsletterRepository newsletterRepository)
    : IQueryHandler<AdminGetNewsletterSubscribersQuery, AdminGetNewsletterSubscribersResult>
{
    /// <summary>
    /// Handles the paginated subscriber listing.
    /// </summary>
    public async Task<AdminGetNewsletterSubscribersResult> Handle(
        AdminGetNewsletterSubscribersQuery query,
        CancellationToken cancellationToken
    )
    {
        PaginatedResult<NewsletterSubscriberEntity> page = await newsletterRepository.GetPagedAsync(
            pageIndex: query.PageIndex,
            pageSize: query.PageSize,
            status: query.Status,
            cancellationToken: cancellationToken
        );

        var subscribers = new PaginatedResult<NewsletterSubscriberDto>(
            pageIndex: page.PageIndex,
            pageSize: page.PageSize,
            count: page.Count,
            items: page.Items.ToNewsletterSubscriberDtoList()
        );

        return new AdminGetNewsletterSubscribersResult(Subscribers: subscribers);
    }
}
