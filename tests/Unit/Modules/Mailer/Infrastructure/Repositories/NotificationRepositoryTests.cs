using _116.Mailer.Contracts.Domain;
using _116.Mailer.Domain.Entities;
using _116.Mailer.Infrastructure.Persistence;
using _116.Mailer.Infrastructure.Repositories;
using _116.Shared.Application.Pagination;
using AwesomeAssertions;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace _116.Unit.Tests.Modules.Mailer.Infrastructure.Repositories;

/// <summary>
/// Unit tests for <see cref="NotificationRepository" /> covering user scoping,
/// the unread filter, and the unread count.
/// </summary>
public class NotificationRepositoryTests
{
    private readonly MailerDbContext _context;
    private readonly NotificationRepository _repository;
    private readonly Guid _userId = Guid.NewGuid();

    public NotificationRepositoryTests()
    {
        DbContextOptions<MailerDbContext> options = new DbContextOptionsBuilder<MailerDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        _context = new MailerDbContext(options);
        _repository = new NotificationRepository(_context);
    }

    /// <summary>
    /// Seeds a notification for a user and returns it.
    /// </summary>
    /// <param name="userId">The recipient user.</param>
    /// <param name="read">Whether the notification starts read.</param>
    /// <returns>The persisted notification.</returns>
    private NotificationEntity Seed(Guid userId, bool read = false)
    {
        var notification = NotificationEntity.Create(
            Guid.NewGuid(),
            userId,
            EnumNotificationType.PasswordChanged,
            title: "title",
            body: "body",
            linkPath: null
        );
        if (read)
        {
            notification.MarkRead(DateTime.UtcNow);
        }

        _context.Notifications.Add(notification);
        _context.SaveChanges();
        return notification;
    }

    [Fact]
    public async Task AddAsync_ShouldPersistTheNotification()
    {
        // Arrange
        var notification = NotificationEntity.Create(
            Guid.NewGuid(),
            _userId,
            EnumNotificationType.PasswordChanged,
            title: "title",
            body: "body",
            linkPath: null
        );

        // Act
        await _repository.AddAsync(notification, CancellationToken.None);
        await _context.SaveChangesAsync();

        // Assert
        (await _context.Notifications.FindAsync(notification.Id))
            .Should()
            .NotBeNull();
    }

    [Fact]
    public async Task GetPagedForUserAsync_ShouldOnlyListTheUsersOwnNotifications()
    {
        // Arrange
        Seed(_userId);
        Seed(Guid.NewGuid());

        // Act
        PaginatedResult<NotificationEntity> page = await _repository.GetPagedForUserAsync(
            _userId,
            pageIndex: 0,
            pageSize: 10,
            unreadOnly: false,
            cancellationToken: CancellationToken.None
        );

        // Assert
        page.Count.Should().Be(1);
        page.Items.Should().OnlyContain(n => n.UserId == _userId);
    }

    [Fact]
    public async Task GetPagedForUserAsync_WithUnreadOnly_ShouldExcludeReadNotifications()
    {
        // Arrange
        NotificationEntity unread = Seed(_userId);
        Seed(_userId, read: true);

        // Act
        PaginatedResult<NotificationEntity> page = await _repository.GetPagedForUserAsync(
            _userId,
            pageIndex: 0,
            pageSize: 10,
            unreadOnly: true,
            cancellationToken: CancellationToken.None
        );

        // Assert
        page.Count.Should().Be(1);
        page.Items.Should().ContainSingle(n => n.Id == unread.Id);
    }

    [Fact]
    public async Task CountUnreadAsync_ShouldCountOnlyTheUsersUnreadNotifications()
    {
        // Arrange
        Seed(_userId);
        Seed(_userId);
        Seed(_userId, read: true);
        Seed(Guid.NewGuid());

        // Act
        int count = await _repository.CountUnreadAsync(_userId, CancellationToken.None);

        // Assert
        count.Should().Be(2);
    }

    [Fact]
    public async Task GetForUserAsync_ShouldRefuseAnotherUsersNotification()
    {
        // Arrange — knowing an id must not be enough to read someone else's notification
        NotificationEntity seeded = Seed(_userId);

        // Act
        NotificationEntity? owned = await _repository.GetForUserAsync(seeded.Id, _userId, CancellationToken.None);
        NotificationEntity? foreign = await _repository.GetForUserAsync(
            seeded.Id,
            Guid.NewGuid(),
            CancellationToken.None
        );

        // Assert
        owned!.Id.Should().Be(seeded.Id);
        foreign.Should().BeNull();
    }

    [Fact]
    public async Task GetUnreadForUserAsync_ShouldReturnEveryUnreadNotificationForTheUser()
    {
        // Arrange
        NotificationEntity first = Seed(_userId);
        NotificationEntity second = Seed(_userId);
        Seed(_userId, read: true);
        Seed(Guid.NewGuid());

        // Act
        IReadOnlyList<NotificationEntity> unread = await _repository.GetUnreadForUserAsync(
            _userId,
            CancellationToken.None
        );

        // Assert
        unread.Should().HaveCount(2);
        unread.Select(n => n.Id).Should().BeEquivalentTo([first.Id, second.Id]);
    }
}
