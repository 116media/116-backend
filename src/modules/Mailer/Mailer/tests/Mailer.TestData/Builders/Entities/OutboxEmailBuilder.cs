using _116.Mailer.Domain.Entities;
using _116.Tests.TestData.Helpers;
using Bogus;

namespace _116.Mailer.TestData.Builders.Entities;

/// <summary>
/// Fluent builder for creating <see cref="OutboxEmailEntity" /> instances in tests.
/// Enqueued by default; the state helpers drive the same send and failure transitions the dispatcher does.
/// </summary>
public class OutboxEmailBuilder
{
    private readonly Faker _faker = TestFaker.Create();

    private Guid _id = Guid.NewGuid();
    private string _recipientAddress;
    private string? _recipientName;
    private string _subject;
    private string _htmlBody;
    private string _textBody;
    private string _template = "welcome";
    private DateTime _now = DateTime.UtcNow;
    private bool _isSent;
    private (string Error, bool IsTransient)? _failure;

    /// <summary>
    /// Initializes a new instance of the <see cref="OutboxEmailBuilder" /> class with random default values.
    /// </summary>
    public OutboxEmailBuilder()
    {
        _recipientAddress = _faker.Internet.Email();
        _recipientName = _faker.Name.FullName();
        _subject = _faker.Lorem.Sentence(wordCount: 4);
        _htmlBody = $"<p>{_faker.Lorem.Sentence()}</p>";
        _textBody = _faker.Lorem.Sentence();
    }

    /// <summary>
    /// Sets the outbox row ID.
    /// </summary>
    /// <param name="id">The outbox email identifier.</param>
    /// <returns>The builder instance for chaining.</returns>
    public OutboxEmailBuilder WithId(Guid id)
    {
        _id = id;
        return this;
    }

    /// <summary>
    /// Sets the recipient address and display name.
    /// </summary>
    /// <param name="recipientAddress">The recipient email address.</param>
    /// <param name="recipientName">The recipient display name, or null.</param>
    /// <returns>The builder instance for chaining.</returns>
    public OutboxEmailBuilder WithRecipient(string recipientAddress, string? recipientName = null)
    {
        _recipientAddress = recipientAddress;
        _recipientName = recipientName;
        return this;
    }

    /// <summary>
    /// Sets the rendered subject and bodies.
    /// </summary>
    /// <param name="subject">The email subject.</param>
    /// <param name="htmlBody">The HTML body.</param>
    /// <param name="textBody">The plain-text body.</param>
    /// <returns>The builder instance for chaining.</returns>
    public OutboxEmailBuilder WithContent(string subject, string htmlBody, string textBody)
    {
        _subject = subject;
        _htmlBody = htmlBody;
        _textBody = textBody;
        return this;
    }

    /// <summary>
    /// Sets the template the email was rendered from.
    /// </summary>
    /// <param name="template">The template name.</param>
    /// <returns>The builder instance for chaining.</returns>
    public OutboxEmailBuilder WithTemplate(string template)
    {
        _template = template;
        return this;
    }

    /// <summary>
    /// Sets the instant the transitions are stamped with.
    /// </summary>
    /// <param name="now">The instant to stamp.</param>
    /// <returns>The builder instance for chaining.</returns>
    public OutboxEmailBuilder At(DateTime now)
    {
        _now = now;
        return this;
    }

    /// <summary>
    /// Marks the email sent.
    /// </summary>
    /// <returns>The builder instance for chaining.</returns>
    public OutboxEmailBuilder AsSent()
    {
        _isSent = true;
        return this;
    }

    /// <summary>
    /// Registers a delivery failure.
    /// </summary>
    /// <param name="error">The failure message.</param>
    /// <param name="isTransient">Whether the failure allows a retry.</param>
    /// <returns>The builder instance for chaining.</returns>
    public OutboxEmailBuilder AsFailed(string error, bool isTransient = true)
    {
        _failure = (error, isTransient);
        return this;
    }

    /// <summary>
    /// Builds the <see cref="OutboxEmailEntity" /> instance.
    /// </summary>
    /// <returns>A configured OutboxEmailEntity instance.</returns>
    public OutboxEmailEntity Build()
    {
        OutboxEmailEntity email = OutboxEmailEntity.Enqueue(
            id: _id,
            recipientAddress: _recipientAddress,
            recipientName: _recipientName,
            subject: _subject,
            htmlBody: _htmlBody,
            textBody: _textBody,
            template: _template,
            now: _now
        );

        if (_isSent)
        {
            email.MarkSent(now: _now);
        }

        if (_failure is not null)
        {
            email.RegisterFailure(error: _failure.Value.Error, isTransient: _failure.Value.IsTransient, now: _now);
        }

        return email;
    }
}
