using _116.Mailer.Domain.Entities;
using _116.Mailer.Infrastructure.Persistence;
using AwesomeAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.InMemory.Internal;
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

    #region ExecuteInTransactionAsync

    /// <summary>
    /// Builds options that let the in-memory provider accept the transaction the seam opens.
    /// The provider ignores it, which is enough to drive the surrounding orchestration.
    /// </summary>
    /// <returns>Options with the transaction warning suppressed.</returns>
    private static DbContextOptions<MailerDbContext> CreateTransactionalOptions() =>
        new DbContextOptionsBuilder<MailerDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .ConfigureWarnings(warnings => warnings.Ignore(InMemoryEventId.TransactionIgnoredWarning))
            .Options;

    [Fact]
    public async Task ExecuteInTransactionAsync_ShouldRunTheOperation()
    {
        // Arrange
        await using var context = new MailerDbContext(CreateTransactionalOptions());
        var unitOfWork = new MailerUnitOfWork(context);
        var ran = false;

        // Act
        await unitOfWork.ExecuteInTransactionAsync(_ =>
        {
            ran = true;
            return Task.CompletedTask;
        });

        // Assert
        ran.Should().BeTrue();
    }

    [Fact]
    public async Task ExecuteInTransactionAsync_ShouldSaveWhatTheOperationTracked()
    {
        // Arrange
        await using var context = new MailerDbContext(CreateTransactionalOptions());
        var unitOfWork = new MailerUnitOfWork(context);

        // Act
        await unitOfWork.ExecuteInTransactionAsync(_ =>
        {
            context.NewsletterSubscribers.Add(
                NewsletterSubscriberEntity.Subscribe(Guid.NewGuid(), $"fan-{Guid.NewGuid():N}@test.com")
            );
            return Task.CompletedTask;
        });

        // Assert
        context.NewsletterSubscribers.Local.Should().ContainSingle();
        context.ChangeTracker.HasChanges().Should().BeFalse();
    }

    [Fact]
    public async Task ExecuteInTransactionAsync_WhenTheOperationThrows_ShouldSurfaceTheFailure()
    {
        // Arrange
        await using var context = new MailerDbContext(CreateTransactionalOptions());
        var unitOfWork = new MailerUnitOfWork(context);
        var failure = new InvalidOperationException("operation failed");

        // Act
        Func<Task> act = async () => await unitOfWork.ExecuteInTransactionAsync(_ => throw failure);

        // Assert
        (await act.Should().ThrowAsync<InvalidOperationException>())
            .Which.Should()
            .BeSameAs(failure);
    }

    [Fact]
    public async Task ExecuteInTransactionAsync_ShouldPassTheTokenToTheOperation()
    {
        // Arrange
        await using var context = new MailerDbContext(CreateTransactionalOptions());
        var unitOfWork = new MailerUnitOfWork(context);
        using var cts = new CancellationTokenSource();
        CancellationToken observed = default;

        // Act
        await unitOfWork.ExecuteInTransactionAsync(
            token =>
            {
                observed = token;
                return Task.CompletedTask;
            },
            cts.Token
        );

        // Assert
        observed.Should().Be(cts.Token);
    }

    #endregion
}
