using _116.Mailer.Domain.Constants;
using _116.Mailer.Domain.Entities;
using _116.Mailer.Infrastructure.Persistence;
using AwesomeAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Xunit;

namespace _116.Unit.Tests.Modules.Mailer.Infrastructure.Persistence;

/// <summary>
/// Unit tests for all Mailer entity configurations.
/// Validates the table mapping, column constraints, enum-to-string conversions
/// and the indexes each configuration declares for its read path.
/// </summary>
public class EntityConfigurationsTests
{
    private static DbContextOptions<MailerDbContext> CreateOptions() =>
        new DbContextOptionsBuilder<MailerDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

    /// <summary>
    /// Resolves an entity type from the design-time model, which keeps the
    /// configuration annotations — value converters and index sort orders —
    /// that the read-optimized runtime model drops.
    /// </summary>
    private static IEntityType GetEntityType<TEntity>(MailerDbContext context)
        where TEntity : class
    {
        IModel model = context.GetService<IDesignTimeModel>().Model;
        IEntityType? entityType = model.FindEntityType(typeof(TEntity));
        entityType.Should().NotBeNull();

        return entityType;
    }

    #region NewsletterSubscriberConfiguration Tests

    [Fact]
    public void NewsletterSubscriberConfiguration_ShouldMapToNewsletterSubscribersTable()
    {
        // Arrange
        using var context = new MailerDbContext(CreateOptions());

        // Act
        IEntityType entityType = GetEntityType<NewsletterSubscriberEntity>(context);

        // Assert
        entityType.GetTableName().Should().Be("newsletter_subscribers");
    }

    [Fact]
    public void NewsletterSubscriberConfiguration_ShouldHavePrimaryKey()
    {
        // Arrange
        using var context = new MailerDbContext(CreateOptions());

        // Act
        IKey? primaryKey = GetEntityType<NewsletterSubscriberEntity>(context).FindPrimaryKey();

        // Assert
        primaryKey.Should().NotBeNull();
        primaryKey.Properties.Should().ContainSingle();
        primaryKey.Properties.First().Name.Should().Be("Id");
    }

    [Fact]
    public void NewsletterSubscriberConfiguration_EmailProperty_ShouldBeRequiredWithAddressLength()
    {
        // Arrange
        using var context = new MailerDbContext(CreateOptions());

        // Act
        IProperty? emailProperty = GetEntityType<NewsletterSubscriberEntity>(context).FindProperty("Email");

        // Assert
        emailProperty.Should().NotBeNull();
        emailProperty.IsNullable.Should().BeFalse();
        emailProperty.GetMaxLength().Should().Be(320);
    }

    [Fact]
    public void NewsletterSubscriberConfiguration_StatusProperty_ShouldBeStoredAsAString()
    {
        // Arrange
        using var context = new MailerDbContext(CreateOptions());

        // Act
        IProperty? statusProperty = GetEntityType<NewsletterSubscriberEntity>(context).FindProperty("Status");

        // Assert
        statusProperty.Should().NotBeNull();
        statusProperty.IsNullable.Should().BeFalse();
        statusProperty.GetMaxLength().Should().Be(30);
        statusProperty.GetProviderClrType().Should().Be<string>();
    }

    [Fact]
    public void NewsletterSubscriberConfiguration_TokenProperties_ShouldBeRequiredWithTokenLength()
    {
        // Arrange
        using var context = new MailerDbContext(CreateOptions());
        IEntityType entityType = GetEntityType<NewsletterSubscriberEntity>(context);

        // Act
        IProperty? confirmationToken = entityType.FindProperty("ConfirmationToken");
        IProperty? unsubscribeToken = entityType.FindProperty("UnsubscribeToken");

        // Assert
        confirmationToken.Should().NotBeNull();
        confirmationToken.IsNullable.Should().BeFalse();
        confirmationToken.GetMaxLength().Should().Be(64);

        unsubscribeToken.Should().NotBeNull();
        unsubscribeToken.IsNullable.Should().BeFalse();
        unsubscribeToken.GetMaxLength().Should().Be(64);
    }

    [Fact]
    public void NewsletterSubscriberConfiguration_ShouldHaveUniqueEmailAndTokenLookups()
    {
        // Arrange
        using var context = new MailerDbContext(CreateOptions());

        // Act
        List<IIndex> indexes = [.. GetEntityType<NewsletterSubscriberEntity>(context).GetIndexes()];

        // Assert
        indexes
            .Should()
            .ContainSingle(index => index.Properties.Single().Name == "Email" && index.IsUnique)
            .And.ContainSingle(index => index.Properties.Single().Name == "ConfirmationToken" && index.IsUnique)
            .And.ContainSingle(index => index.Properties.Single().Name == "UnsubscribeToken" && index.IsUnique);
    }

    #endregion

    #region NotificationConfiguration Tests

    [Fact]
    public void NotificationConfiguration_ShouldMapToNotificationsTable()
    {
        // Arrange
        using var context = new MailerDbContext(CreateOptions());

        // Act
        IEntityType entityType = GetEntityType<NotificationEntity>(context);

        // Assert
        entityType.GetTableName().Should().Be("notifications");
    }

    [Fact]
    public void NotificationConfiguration_ShouldHavePrimaryKey()
    {
        // Arrange
        using var context = new MailerDbContext(CreateOptions());

        // Act
        IKey? primaryKey = GetEntityType<NotificationEntity>(context).FindPrimaryKey();

        // Assert
        primaryKey.Should().NotBeNull();
        primaryKey.Properties.Should().ContainSingle();
        primaryKey.Properties.First().Name.Should().Be("Id");
    }

    [Fact]
    public void NotificationConfiguration_UserIdProperty_ShouldBeRequired()
    {
        // Arrange
        using var context = new MailerDbContext(CreateOptions());

        // Act
        IProperty? userIdProperty = GetEntityType<NotificationEntity>(context).FindProperty("UserId");

        // Assert
        userIdProperty.Should().NotBeNull();
        userIdProperty.IsNullable.Should().BeFalse();
    }

    [Fact]
    public void NotificationConfiguration_TypeProperty_ShouldBeStoredAsAString()
    {
        // Arrange
        using var context = new MailerDbContext(CreateOptions());

        // Act
        IProperty? typeProperty = GetEntityType<NotificationEntity>(context).FindProperty("Type");

        // Assert
        typeProperty.Should().NotBeNull();
        typeProperty.IsNullable.Should().BeFalse();
        typeProperty.GetMaxLength().Should().Be(50);
        typeProperty.GetProviderClrType().Should().Be<string>();
    }

    [Fact]
    public void NotificationConfiguration_BodyProperties_ShouldCarryTheRenderedCopyConstraints()
    {
        // Arrange
        using var context = new MailerDbContext(CreateOptions());
        IEntityType entityType = GetEntityType<NotificationEntity>(context);

        // Act
        IProperty? titleProperty = entityType.FindProperty("Title");
        IProperty? bodyProperty = entityType.FindProperty("Body");
        IProperty? linkPathProperty = entityType.FindProperty("LinkPath");

        // Assert
        titleProperty.Should().NotBeNull();
        titleProperty.IsNullable.Should().BeFalse();
        titleProperty.GetMaxLength().Should().Be(200);

        bodyProperty.Should().NotBeNull();
        bodyProperty.IsNullable.Should().BeFalse();
        bodyProperty.GetMaxLength().Should().Be(500);

        linkPathProperty.Should().NotBeNull();
        linkPathProperty.IsNullable.Should().BeTrue();
        linkPathProperty.GetMaxLength().Should().Be(300);
    }

    [Fact]
    public void NotificationConfiguration_ShouldHaveTheUnreadCountIndex()
    {
        // Arrange
        using var context = new MailerDbContext(CreateOptions());

        // Act
        IIndex? unreadCountIndex = GetEntityType<NotificationEntity>(context)
            .GetIndexes()
            .SingleOrDefault(index =>
                index.Properties.Select(property => property.Name).SequenceEqual(new[] { "UserId", "ReadAt" })
            );

        // Assert
        unreadCountIndex.Should().NotBeNull();
        unreadCountIndex.IsUnique.Should().BeFalse();
    }

    [Fact]
    public void NotificationConfiguration_ShouldHaveTheNewestFirstFeedIndex()
    {
        // Arrange
        using var context = new MailerDbContext(CreateOptions());

        // Act
        IIndex? feedIndex = GetEntityType<NotificationEntity>(context)
            .GetIndexes()
            .SingleOrDefault(index =>
                index.Properties.Select(property => property.Name).SequenceEqual(new[] { "UserId", "CreatedAt" })
            );

        // Assert
        feedIndex.Should().NotBeNull();
        feedIndex.IsDescending.Should().Equal(false, true);
    }

    #endregion

    #region OutboxEmailConfiguration Tests

    [Fact]
    public void OutboxEmailConfiguration_ShouldMapToOutboxEmailsTable()
    {
        // Arrange
        using var context = new MailerDbContext(CreateOptions());

        // Act
        IEntityType entityType = GetEntityType<OutboxEmailEntity>(context);

        // Assert
        entityType.GetTableName().Should().Be("outbox_emails");
    }

    [Fact]
    public void OutboxEmailConfiguration_ShouldHavePrimaryKey()
    {
        // Arrange
        using var context = new MailerDbContext(CreateOptions());

        // Act
        IKey? primaryKey = GetEntityType<OutboxEmailEntity>(context).FindPrimaryKey();

        // Assert
        primaryKey.Should().NotBeNull();
        primaryKey.Properties.Should().ContainSingle();
        primaryKey.Properties.First().Name.Should().Be("Id");
    }

    [Fact]
    public void OutboxEmailConfiguration_RecipientProperties_ShouldRequireOnlyTheAddress()
    {
        // Arrange
        using var context = new MailerDbContext(CreateOptions());
        IEntityType entityType = GetEntityType<OutboxEmailEntity>(context);

        // Act
        IProperty? recipientAddress = entityType.FindProperty("RecipientAddress");
        IProperty? recipientName = entityType.FindProperty("RecipientName");

        // Assert
        recipientAddress.Should().NotBeNull();
        recipientAddress.IsNullable.Should().BeFalse();
        recipientAddress.GetMaxLength().Should().Be(320);

        recipientName.Should().NotBeNull();
        recipientName.IsNullable.Should().BeTrue();
        recipientName.GetMaxLength().Should().Be(200);
    }

    [Fact]
    public void OutboxEmailConfiguration_BodyProperties_ShouldBeRequired()
    {
        // Arrange
        using var context = new MailerDbContext(CreateOptions());
        IEntityType entityType = GetEntityType<OutboxEmailEntity>(context);

        // Act
        IProperty? subjectProperty = entityType.FindProperty("Subject");
        IProperty? htmlBodyProperty = entityType.FindProperty("HtmlBody");
        IProperty? textBodyProperty = entityType.FindProperty("TextBody");
        IProperty? templateProperty = entityType.FindProperty("Template");

        // Assert
        subjectProperty.Should().NotBeNull();
        subjectProperty.IsNullable.Should().BeFalse();
        subjectProperty.GetMaxLength().Should().Be(500);

        htmlBodyProperty.Should().NotBeNull();
        htmlBodyProperty.IsNullable.Should().BeFalse();
        htmlBodyProperty.GetMaxLength().Should().BeNull();

        textBodyProperty.Should().NotBeNull();
        textBodyProperty.IsNullable.Should().BeFalse();
        textBodyProperty.GetMaxLength().Should().BeNull();

        templateProperty.Should().NotBeNull();
        templateProperty.IsNullable.Should().BeFalse();
        templateProperty.GetMaxLength().Should().Be(100);
    }

    [Fact]
    public void OutboxEmailConfiguration_StatusProperty_ShouldBeStoredAsAString()
    {
        // Arrange
        using var context = new MailerDbContext(CreateOptions());

        // Act
        IProperty? statusProperty = GetEntityType<OutboxEmailEntity>(context).FindProperty("Status");

        // Assert
        statusProperty.Should().NotBeNull();
        statusProperty.IsNullable.Should().BeFalse();
        statusProperty.GetMaxLength().Should().Be(20);
        statusProperty.GetProviderClrType().Should().Be<string>();
    }

    [Fact]
    public void OutboxEmailConfiguration_LastErrorProperty_ShouldBeCappedAtTheConfiguredLength()
    {
        // Arrange
        using var context = new MailerDbContext(CreateOptions());

        // Act
        IProperty? lastErrorProperty = GetEntityType<OutboxEmailEntity>(context).FindProperty("LastError");

        // Assert
        lastErrorProperty.Should().NotBeNull();
        lastErrorProperty.IsNullable.Should().BeTrue();
        lastErrorProperty.GetMaxLength().Should().Be(MailerConstants.MaxLastErrorLength);
    }

    [Fact]
    public void OutboxEmailConfiguration_ShouldHaveTheDispatcherScanIndex()
    {
        // Arrange
        using var context = new MailerDbContext(CreateOptions());

        // Act
        IIndex? scanIndex = GetEntityType<OutboxEmailEntity>(context)
            .GetIndexes()
            .SingleOrDefault(index =>
                index.Properties.Select(property => property.Name).SequenceEqual(new[] { "Status", "NextAttemptAt" })
            );

        // Assert
        scanIndex.Should().NotBeNull();
        scanIndex.IsUnique.Should().BeFalse();
    }

    #endregion

    #region Schema Tests

    [Fact]
    public void AllEntityConfigurations_ShouldUseMailerSchema()
    {
        // Arrange
        using var context = new MailerDbContext(CreateOptions());

        // Act & Assert
        GetEntityType<NewsletterSubscriberEntity>(context).GetSchema().Should().Be(MailerConstants.SchemaName);
        GetEntityType<NotificationEntity>(context).GetSchema().Should().Be(MailerConstants.SchemaName);
        GetEntityType<OutboxEmailEntity>(context).GetSchema().Should().Be(MailerConstants.SchemaName);
    }

    #endregion
}
