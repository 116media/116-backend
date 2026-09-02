using _116.Mailer.Domain.Entities;
using _116.Mailer.Domain.Enums;
using _116.Mailer.Infrastructure.Persistence;
using _116.Mailer.Infrastructure.Repositories;
using _116.Shared.Application.Pagination;
using AwesomeAssertions;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace _116.Unit.Tests.Modules.Mailer.Infrastructure.Repositories;

/// <summary>
/// Unit tests for <see cref="NewsletterRepository" /> covering the token and
/// email lookups and the paged listing with its status filter.
/// </summary>
public class NewsletterRepositoryTests
{
    private readonly MailerDbContext _context;
    private readonly NewsletterRepository _repository;

    public NewsletterRepositoryTests()
    {
        DbContextOptions<MailerDbContext> options = new DbContextOptionsBuilder<MailerDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        _context = new MailerDbContext(options);
        _repository = new NewsletterRepository(_context);
    }

    /// <summary>
    /// Seeds a subscriber and returns it.
    /// </summary>
    /// <param name="email">The subscriber email address.</param>
    /// <returns>The persisted subscriber.</returns>
    private NewsletterSubscriberEntity Seed(string email)
    {
        var subscriber = NewsletterSubscriberEntity.Subscribe(Guid.NewGuid(), email);
        _context.NewsletterSubscribers.Add(subscriber);
        _context.SaveChanges();
        return subscriber;
    }

    [Fact]
    public async Task AddAsync_ShouldPersistTheSubscriber()
    {
        // Arrange
        var subscriber = NewsletterSubscriberEntity.Subscribe(Guid.NewGuid(), "fan@example.com");

        // Act
        await _repository.AddAsync(subscriber, CancellationToken.None);
        await _context.SaveChangesAsync();

        // Assert
        (await _context.NewsletterSubscribers.FindAsync(subscriber.Id))
            .Should()
            .NotBeNull();
    }

    [Fact]
    public async Task GetByEmailAsync_ShouldNormalizeCaseAndWhitespaceBeforeMatching()
    {
        // Arrange
        NewsletterSubscriberEntity seeded = Seed("fan@example.com");

        // Act
        NewsletterSubscriberEntity? found = await _repository.GetByEmailAsync(
            "  FAN@Example.COM ",
            CancellationToken.None
        );

        // Assert
        found.Should().NotBeNull();
        found!.Id.Should().Be(seeded.Id);
    }

    [Fact]
    public async Task GetByEmailAsync_UnknownAddress_ShouldReturnNull()
    {
        // Arrange
        Seed("fan@example.com");

        // Act
        NewsletterSubscriberEntity? found = await _repository.GetByEmailAsync(
            "other@example.com",
            CancellationToken.None
        );

        // Assert
        found.Should().BeNull();
    }

    [Fact]
    public async Task GetByConfirmationTokenAsync_ShouldMatchOnlyItsOwnToken()
    {
        // Arrange
        NewsletterSubscriberEntity seeded = Seed("fan@example.com");

        // Act
        NewsletterSubscriberEntity? byToken = await _repository.GetByConfirmationTokenAsync(
            seeded.ConfirmationToken,
            CancellationToken.None
        );
        NewsletterSubscriberEntity? byWrongToken = await _repository.GetByConfirmationTokenAsync(
            "not-a-token",
            CancellationToken.None
        );

        // Assert
        byToken!.Id.Should().Be(seeded.Id);
        byWrongToken.Should().BeNull();
    }

    [Fact]
    public async Task GetByUnsubscribeTokenAsync_ShouldMatchOnlyItsOwnToken()
    {
        // Arrange
        NewsletterSubscriberEntity seeded = Seed("fan@example.com");

        // Act
        NewsletterSubscriberEntity? byToken = await _repository.GetByUnsubscribeTokenAsync(
            seeded.UnsubscribeToken,
            CancellationToken.None
        );
        NewsletterSubscriberEntity? byWrongToken = await _repository.GetByUnsubscribeTokenAsync(
            "not-a-token",
            CancellationToken.None
        );

        // Assert
        byToken!.Id.Should().Be(seeded.Id);
        byWrongToken.Should().BeNull();
    }

    [Fact]
    public async Task GetPagedAsync_ShouldFilterByStatusAndCountTheFilteredSet()
    {
        // Arrange
        Seed("pending@example.com");
        NewsletterSubscriberEntity confirmed = Seed("subscribed@example.com");
        confirmed.Confirm(DateTime.UtcNow);
        _context.SaveChanges();

        // Act
        PaginatedResult<NewsletterSubscriberEntity> page = await _repository.GetPagedAsync(
            pageIndex: 0,
            pageSize: 10,
            status: EnumNewsletterStatus.Subscribed,
            cancellationToken: CancellationToken.None
        );

        // Assert
        page.Count.Should().Be(1);
        page.Items.Should().ContainSingle(s => s.Email == "subscribed@example.com");
    }

    [Fact]
    public async Task GetPagedAsync_ShouldListNewestFirstAndHonourThePageWindow()
    {
        // Arrange — creation stamps are set explicitly, so the expected order is not a save-order accident
        NewsletterSubscriberEntity older = Seed("older@example.com");
        NewsletterSubscriberEntity newer = Seed("newer@example.com");
        older.CreatedAt = DateTime.UtcNow.AddMinutes(-10);
        newer.CreatedAt = DateTime.UtcNow;
        _context.SaveChanges();

        // Act
        PaginatedResult<NewsletterSubscriberEntity> firstPage = await _repository.GetPagedAsync(
            pageIndex: 0,
            pageSize: 1,
            status: null,
            cancellationToken: CancellationToken.None
        );
        PaginatedResult<NewsletterSubscriberEntity> secondPage = await _repository.GetPagedAsync(
            pageIndex: 1,
            pageSize: 1,
            status: null,
            cancellationToken: CancellationToken.None
        );

        // Assert
        firstPage.Count.Should().Be(2);
        firstPage.Items.Should().ContainSingle(s => s.Email == "newer@example.com");
        secondPage.Items.Should().ContainSingle(s => s.Email == "older@example.com");
    }
}
