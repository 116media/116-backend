using _116.Mailer.Application.Shared.Exceptions;
using _116.Mailer.Application.Shared.Repositories;
using _116.Mailer.Application.Shared.Services;
using _116.Mailer.Domain.Entities;
using _116.Mailer.Domain.Enums;
using _116.Mailer.Infrastructure.BackgroundJobs;
using _116.Mailer.Infrastructure.Persistence;
using AwesomeAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using Quartz;
using Xunit;

namespace _116.Unit.Tests.Modules.Mailer.Infrastructure.BackgroundJobs;

/// <summary>
/// Unit tests for <see cref="OutboxEmailDispatcherJob" /> covering the empty
/// run, the delivery outcomes, and the one-bad-message-never-stops-the-batch
/// guarantee.
/// </summary>
public class OutboxEmailDispatcherJobTests
{
    private readonly Mock<IOutboxEmailRepository> _repositoryMock = new();
    private readonly Mock<IEmailSender> _senderMock = new();
    private readonly Mock<IJobExecutionContext> _jobContextMock = new();
    private readonly OutboxEmailDispatcherJob _job;

    public OutboxEmailDispatcherJobTests()
    {
        // The InMemory provider has no real transactions; the ignored warning keeps
        // the job's transactional shape runnable without a relational database.
        DbContextOptions<MailerDbContext> options = new DbContextOptionsBuilder<MailerDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .ConfigureWarnings(w => w.Ignore(InMemoryEventId.TransactionIgnoredWarning))
            .Options;
        var dbContext = new MailerDbContext(options);

        Mock<IServiceScopeFactory> scopeFactoryMock = new();
        Mock<IServiceScope> scopeMock = new();
        Mock<IServiceProvider> serviceProviderMock = new();
        Mock<ILogger<OutboxEmailDispatcherJob>> loggerMock = new();

        scopeFactoryMock.Setup(x => x.CreateScope()).Returns(scopeMock.Object);
        scopeMock.Setup(x => x.ServiceProvider).Returns(serviceProviderMock.Object);

        serviceProviderMock.Setup(x => x.GetService(typeof(MailerDbContext))).Returns(dbContext);
        serviceProviderMock.Setup(x => x.GetService(typeof(IOutboxEmailRepository))).Returns(_repositoryMock.Object);
        serviceProviderMock.Setup(x => x.GetService(typeof(IEmailSender))).Returns(_senderMock.Object);

        _jobContextMock.Setup(x => x.CancellationToken).Returns(CancellationToken.None);

        _job = new OutboxEmailDispatcherJob(scopeFactoryMock.Object, loggerMock.Object);
    }

    /// <summary>
    /// Builds a pending outbox email due for delivery.
    /// </summary>
    /// <param name="address">The recipient address.</param>
    /// <returns>The pending email.</returns>
    private static OutboxEmailEntity PendingEmail(string address = "fan@example.com")
    {
        return OutboxEmailEntity.Enqueue(
            Guid.NewGuid(),
            recipientAddress: address,
            recipientName: null,
            subject: "subject",
            htmlBody: "<p>body</p>",
            textBody: "body",
            template: "NewsletterWelcome",
            now: DateTime.UtcNow
        );
    }

    /// <summary>
    /// Points the claimed batch at the supplied rows.
    /// </summary>
    /// <param name="batch">The rows the dispatcher claims.</param>
    private void SetupBatch(params OutboxEmailEntity[] batch)
    {
        _repositoryMock
            .Setup(r => r.ClaimDueBatchAsync(It.IsAny<int>(), It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(batch);
    }

    [Fact]
    public async Task Execute_WithNoDueEmails_ShouldNeverTouchTheSender()
    {
        // Arrange
        SetupBatch();

        // Act
        await _job.Execute(_jobContextMock.Object);

        // Assert
        _senderMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task Execute_WhenDeliverySucceeds_ShouldMarkTheEmailSent()
    {
        // Arrange
        OutboxEmailEntity email = PendingEmail();
        SetupBatch(email);

        // Act
        await _job.Execute(_jobContextMock.Object);

        // Assert
        email.Status.Should().Be(EnumOutboxEmailStatus.Sent);
        email.SentAt.Should().NotBeNull();
        _senderMock.Verify(
            s =>
                s.SendAsync(It.Is<EmailMessage>(m => m.To.Address == "fan@example.com"), It.IsAny<CancellationToken>()),
            Times.Once
        );
    }

    [Fact]
    public async Task Execute_OnATransientFailure_ShouldKeepTheEmailPendingForRetry()
    {
        // Arrange
        OutboxEmailEntity email = PendingEmail();
        SetupBatch(email);
        _senderMock
            .Setup(s => s.SendAsync(It.IsAny<EmailMessage>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new EmailDeliveryException("timeout", isTransient: true));

        // Act
        await _job.Execute(_jobContextMock.Object);

        // Assert
        email.Status.Should().Be(EnumOutboxEmailStatus.Pending);
        email.AttemptCount.Should().Be(1);
        email.LastError.Should().Be("timeout");
    }

    [Fact]
    public async Task Execute_OnAPermanentFailure_ShouldMarkTheEmailFailed()
    {
        // Arrange
        OutboxEmailEntity email = PendingEmail();
        SetupBatch(email);
        _senderMock
            .Setup(s => s.SendAsync(It.IsAny<EmailMessage>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new EmailDeliveryException("invalid recipient", isTransient: false));

        // Act
        await _job.Execute(_jobContextMock.Object);

        // Assert
        email.Status.Should().Be(EnumOutboxEmailStatus.Failed);
        email.AttemptCount.Should().Be(1);
    }

    [Fact]
    public async Task Execute_OnAnUnclassifiedError_ShouldTreatItAsTransient()
    {
        // Arrange
        OutboxEmailEntity email = PendingEmail();
        SetupBatch(email);
        _senderMock
            .Setup(s => s.SendAsync(It.IsAny<EmailMessage>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("provider hiccup"));

        // Act
        await _job.Execute(_jobContextMock.Object);

        // Assert
        email.Status.Should().Be(EnumOutboxEmailStatus.Pending);
        email.AttemptCount.Should().Be(1);
    }

    [Fact]
    public async Task Execute_WhenOneEmailFails_ShouldStillDeliverTheRestOfTheBatch()
    {
        // Arrange
        OutboxEmailEntity failing = PendingEmail("failing@example.com");
        OutboxEmailEntity healthy = PendingEmail("healthy@example.com");
        SetupBatch(failing, healthy);
        _senderMock
            .Setup(s =>
                s.SendAsync(
                    It.Is<EmailMessage>(m => m.To.Address == "failing@example.com"),
                    It.IsAny<CancellationToken>()
                )
            )
            .ThrowsAsync(new EmailDeliveryException("rejected", isTransient: false));

        // Act
        await _job.Execute(_jobContextMock.Object);

        // Assert
        failing.Status.Should().Be(EnumOutboxEmailStatus.Failed);
        healthy.Status.Should().Be(EnumOutboxEmailStatus.Sent);
    }
}
