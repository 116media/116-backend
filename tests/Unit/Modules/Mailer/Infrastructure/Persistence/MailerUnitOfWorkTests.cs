using _116.Mailer.Domain.Entities;
using _116.Mailer.Infrastructure.Persistence;
using AwesomeAssertions;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace _116.Unit.Tests.Modules.Mailer.Infrastructure.Persistence;

/// <summary>
/// Unit tests for <see cref="MailerUnitOfWork" />.
/// </summary>
public class MailerUnitOfWorkTests
{
    private static DbContextOptions<MailerDbContext> CreateOptions() =>
        new DbContextOptionsBuilder<MailerDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

    [Fact]
    public async Task CommitAsync_WithoutChanges_ShouldReturnZero()
    {
        // Arrange
        await using var context = new MailerDbContext(CreateOptions());
        var unitOfWork = new MailerUnitOfWork(context);

        // Act
        int result = await unitOfWork.CommitAsync();

        // Assert
        result.Should().Be(0);
    }

    [Fact]
    public async Task CommitAsync_WithATrackedChange_ShouldPersistItAndReportTheRow()
    {
        // Arrange
        await using var context = new MailerDbContext(CreateOptions());
        var unitOfWork = new MailerUnitOfWork(context);
        var subscriber = NewsletterSubscriberEntity.Subscribe(Guid.NewGuid(), "fan@example.com");
        context.NewsletterSubscribers.Add(subscriber);

        // Act
        int result = await unitOfWork.CommitAsync(CancellationToken.None);

        // Assert
        result.Should().Be(1);
        (await context.NewsletterSubscribers.FindAsync(subscriber.Id)).Should().NotBeNull();
    }
}
