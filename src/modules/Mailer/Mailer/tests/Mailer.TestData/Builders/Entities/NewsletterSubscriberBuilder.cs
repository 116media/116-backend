using _116.Mailer.Domain.Entities;
using _116.Tests.TestData.Helpers;
using Bogus;

namespace _116.Mailer.TestData.Builders.Entities;

/// <summary>
/// Fluent builder for creating <see cref="NewsletterSubscriberEntity" /> instances in tests.
/// Subscribers start pending confirmation, as a real subscription does; the state helpers walk the
/// same transitions the application uses.
/// </summary>
public class NewsletterSubscriberBuilder
{
    private readonly Faker _faker = TestFaker.Create();

    private Guid _id = Guid.NewGuid();
    private string _email;
    private bool _isConfirmed;
    private bool _isUnsubscribed;
    private DateTime _now = DateTime.UtcNow;

    /// <summary>
    /// Initializes a new instance of the <see cref="NewsletterSubscriberBuilder" /> class with random default values.
    /// </summary>
    public NewsletterSubscriberBuilder()
    {
        _email = _faker.Internet.Email();
    }

    /// <summary>
    /// Sets the subscriber ID.
    /// </summary>
    /// <param name="id">The subscriber identifier.</param>
    /// <returns>The builder instance for chaining.</returns>
    public NewsletterSubscriberBuilder WithId(Guid id)
    {
        _id = id;
        return this;
    }

    /// <summary>
    /// Sets the subscriber email.
    /// </summary>
    /// <param name="email">The email address; normalized by the domain.</param>
    /// <returns>The builder instance for chaining.</returns>
    public NewsletterSubscriberBuilder WithEmail(string email)
    {
        _email = email;
        return this;
    }

    /// <summary>
    /// Sets the instant the state transitions are stamped with.
    /// </summary>
    /// <param name="now">The instant to stamp.</param>
    /// <returns>The builder instance for chaining.</returns>
    public NewsletterSubscriberBuilder At(DateTime now)
    {
        _now = now;
        return this;
    }

    /// <summary>
    /// Confirms the subscription.
    /// </summary>
    /// <returns>The builder instance for chaining.</returns>
    public NewsletterSubscriberBuilder AsConfirmed()
    {
        _isConfirmed = true;
        return this;
    }

    /// <summary>
    /// Confirms and then unsubscribes, the only path to the unsubscribed state.
    /// </summary>
    /// <returns>The builder instance for chaining.</returns>
    public NewsletterSubscriberBuilder AsUnsubscribed()
    {
        _isConfirmed = true;
        _isUnsubscribed = true;
        return this;
    }

    /// <summary>
    /// Builds the <see cref="NewsletterSubscriberEntity" /> instance.
    /// </summary>
    /// <returns>A configured NewsletterSubscriberEntity instance.</returns>
    public NewsletterSubscriberEntity Build()
    {
        NewsletterSubscriberEntity subscriber = NewsletterSubscriberEntity.Subscribe(id: _id, email: _email);

        if (_isConfirmed)
        {
            subscriber.Confirm(now: _now);
        }

        if (_isUnsubscribed)
        {
            subscriber.Unsubscribe(now: _now);
        }

        return subscriber;
    }
}
