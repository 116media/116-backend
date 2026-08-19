using _116.Mailer.Domain.Entities;
using _116.Mailer.Infrastructure.Persistence;
using AwesomeAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Xunit;

namespace _116.Unit.Tests.Modules.Mailer.Infrastructure.Persistence;

/// <summary>
/// Unit tests for <see cref="MailerDbContext" />.
/// Covers the exposed sets and the model the context builds: the "mailer"
/// default schema and the configurations discovered from the module assembly.
/// </summary>
public class MailerDbContextTests
{
    private static DbContextOptions<MailerDbContext> CreateOptions() =>
        new DbContextOptionsBuilder<MailerDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

    #region DbSet Properties

    [Fact]
    public void OutboxEmails_ShouldReturnDbSet()
    {
        using var context = new MailerDbContext(CreateOptions());
        DbSet<OutboxEmailEntity> result = context.OutboxEmails;
        result.Should().NotBeNull();
    }

    [Fact]
    public void NewsletterSubscribers_ShouldReturnDbSet()
    {
        using var context = new MailerDbContext(CreateOptions());
        DbSet<NewsletterSubscriberEntity> result = context.NewsletterSubscribers;
        result.Should().NotBeNull();
    }

    [Fact]
    public void Notifications_ShouldReturnDbSet()
    {
        using var context = new MailerDbContext(CreateOptions());
        DbSet<NotificationEntity> result = context.Notifications;
        result.Should().NotBeNull();
    }

    #endregion

    #region Schema and Configuration

    [Fact]
    public void OnModelCreating_ShouldApplyConfigurationsFromAssembly()
    {
        using var context = new MailerDbContext(CreateOptions());
        IModel model = context.Model;
        IEntityType? outboxEmailEntityType = model.FindEntityType(typeof(OutboxEmailEntity));

        outboxEmailEntityType.Should().NotBeNull();
        outboxEmailEntityType.GetTableName().Should().Be("outbox_emails");
    }

    [Fact]
    public void Context_ShouldSetDefaultSchemaToMailer()
    {
        using var context = new MailerDbContext(CreateOptions());
        IModel model = context.Model;

        model.FindEntityType(typeof(OutboxEmailEntity))?.GetSchema().Should().Be("mailer");
        model.FindEntityType(typeof(NewsletterSubscriberEntity))?.GetSchema().Should().Be("mailer");
        model.FindEntityType(typeof(NotificationEntity))?.GetSchema().Should().Be("mailer");
    }

    [Fact]
    public void Context_ShouldHaveAllEntityTypesConfigured()
    {
        using var context = new MailerDbContext(CreateOptions());
        IModel model = context.Model;

        model.FindEntityType(typeof(OutboxEmailEntity)).Should().NotBeNull();
        model.FindEntityType(typeof(NewsletterSubscriberEntity)).Should().NotBeNull();
        model.FindEntityType(typeof(NotificationEntity)).Should().NotBeNull();
    }

    #endregion
}
