using _116.Mailer.Application.Newsletter.UseCases.Admin.Queries.GetNewsletterSubscribers;
using _116.Mailer.Application.Shared.Repositories;
using _116.Mailer.Domain.Entities;
using _116.Mailer.Domain.Enums;
using _116.Shared.Application.Pagination;
using AwesomeAssertions;
using Moq;
using Xunit;

namespace _116.Unit.Tests.Modules.Mailer.Application.Newsletter;

/// <summary>
/// Unit tests for <see cref="AdminGetNewsletterSubscribersHandler" /> covering
/// the paging passthrough and the entity-to-DTO projection.
/// </summary>
public class AdminGetNewsletterSubscribersHandlerTests
{
    private readonly Mock<INewsletterRepository> _repository = new();

    private AdminGetNewsletterSubscribersHandler Handler => new(_repository.Object);

    [Fact]
    public async Task Handle_ShouldProjectThePageToDtosPreservingThePagingEnvelope()
    {
        // Arrange
        var subscriber = NewsletterSubscriberEntity.Subscribe(Guid.NewGuid(), "fan@example.com");
        var page = new PaginatedResult<NewsletterSubscriberEntity>(
            pageIndex: 2,
            pageSize: 10,
            count: 21,
            items: [subscriber]
        );
        _repository.Setup(r => r.GetPagedAsync(2, 10, null, It.IsAny<CancellationToken>())).ReturnsAsync(page);

        // Act
        AdminGetNewsletterSubscribersResult result = await Handler.Handle(
            new AdminGetNewsletterSubscribersQuery(PageIndex: 2, PageSize: 10, Status: null),
            CancellationToken.None
        );

        // Assert
        result.Subscribers.PageIndex.Should().Be(2);
        result.Subscribers.PageSize.Should().Be(10);
        result.Subscribers.Count.Should().Be(21);
        result.Subscribers.Items.Should().ContainSingle(dto => dto.Email == "fan@example.com");
    }

    [Fact]
    public async Task Handle_WithAStatusFilter_ShouldPassItThroughToTheRepository()
    {
        // Arrange
        var page = new PaginatedResult<NewsletterSubscriberEntity>(pageIndex: 0, pageSize: 20, count: 0, items: []);
        _repository
            .Setup(r => r.GetPagedAsync(0, 20, EnumNewsletterStatus.Unsubscribed, It.IsAny<CancellationToken>()))
            .ReturnsAsync(page);

        // Act
        AdminGetNewsletterSubscribersResult result = await Handler.Handle(
            new AdminGetNewsletterSubscribersQuery(
                PageIndex: 0,
                PageSize: 20,
                Status: EnumNewsletterStatus.Unsubscribed
            ),
            CancellationToken.None
        );

        // Assert
        result.Subscribers.Items.Should().BeEmpty();
        _repository.Verify(
            r => r.GetPagedAsync(0, 20, EnumNewsletterStatus.Unsubscribed, It.IsAny<CancellationToken>()),
            Times.Once
        );
    }
}
