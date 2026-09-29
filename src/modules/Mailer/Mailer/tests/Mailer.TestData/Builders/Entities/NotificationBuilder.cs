using _116.Mailer.Contracts.Domain.Enums;
using _116.Mailer.Domain.Entities;
using _116.Tests.TestData.Helpers;
using Bogus;

namespace _116.Mailer.TestData.Builders.Entities;

/// <summary>
/// Fluent builder for creating <see cref="NotificationEntity" /> instances in tests.
/// Drives the real domain transitions, so every state it produces is one the application can reach.
/// </summary>
public class NotificationBuilder
{
    private readonly Faker _faker = TestFaker.Create();

    private Guid _id = Guid.NewGuid();
    private Guid _userId = Guid.NewGuid();
    private EnumNotificationType _type = EnumNotificationType.PasswordChanged;
    private string _title;
    private string _body;
    private string? _linkPath;
    private DateTime? _readAt;

    /// <summary>
    /// Initializes a new instance of the <see cref="NotificationBuilder" /> class with random default values.
    /// </summary>
    public NotificationBuilder()
    {
        _title = _faker.Lorem.Sentence(wordCount: 3);
        _body = _faker.Lorem.Sentence();
    }

    /// <summary>
    /// Sets the notification ID.
    /// </summary>
    /// <param name="id">The notification identifier.</param>
    /// <returns>The builder instance for chaining.</returns>
    public NotificationBuilder WithId(Guid id)
    {
        _id = id;
        return this;
    }

    /// <summary>
    /// Sets the recipient.
    /// </summary>
    /// <param name="userId">The identity user UUID the notification belongs to.</param>
    /// <returns>The builder instance for chaining.</returns>
    public NotificationBuilder WithUserId(Guid userId)
    {
        _userId = userId;
        return this;
    }

    /// <summary>
    /// Sets the notification type.
    /// </summary>
    /// <param name="type">The notification type.</param>
    /// <returns>The builder instance for chaining.</returns>
    public NotificationBuilder WithType(EnumNotificationType type)
    {
        _type = type;
        return this;
    }

    /// <summary>
    /// Sets the rendered title and body.
    /// </summary>
    /// <param name="title">The notification title.</param>
    /// <param name="body">The notification body.</param>
    /// <returns>The builder instance for chaining.</returns>
    public NotificationBuilder WithContent(string title, string body)
    {
        _title = title;
        _body = body;
        return this;
    }

    /// <summary>
    /// Sets the in-app link the notification points at.
    /// </summary>
    /// <param name="linkPath">The relative link path, or null.</param>
    /// <returns>The builder instance for chaining.</returns>
    public NotificationBuilder WithLinkPath(string? linkPath)
    {
        _linkPath = linkPath;
        return this;
    }

    /// <summary>
    /// Marks the notification read at the supplied instant.
    /// </summary>
    /// <param name="readAt">When the recipient read it; defaults to now.</param>
    /// <returns>The builder instance for chaining.</returns>
    public NotificationBuilder AsRead(DateTime? readAt = null)
    {
        _readAt = readAt ?? DateTime.UtcNow;
        return this;
    }

    /// <summary>
    /// Builds the <see cref="NotificationEntity" /> instance.
    /// </summary>
    /// <returns>A configured NotificationEntity instance.</returns>
    public NotificationEntity Build()
    {
        NotificationEntity notification = NotificationEntity.Create(
            id: _id,
            userId: _userId,
            type: _type,
            title: _title,
            body: _body,
            linkPath: _linkPath
        );

        if (_readAt is not null)
        {
            notification.MarkRead(now: _readAt.Value);
        }

        return notification;
    }
}
