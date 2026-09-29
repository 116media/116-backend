using _116.Mailer.Domain.Entities;
using _116.Mailer.TestData.Builders.Entities;

namespace _116.Mailer.TestData.Factories;

/// <summary>
/// Named aliases for <see cref="NewsletterSubscriberBuilder" /> chains that three or more tests share
/// verbatim. A shape fewer tests need belongs at the call site as a builder chain, not here —
/// factory names carry the combinatorics, and combinatorics multiply.
/// </summary>
public static class NewsletterSubscriberFactory
{
    /// <summary>
    /// Creates a subscriber awaiting confirmation, the state a fresh subscription starts in.
    /// </summary>
    /// <param name="email">The subscriber email address.</param>
    /// <returns>A pending NewsletterSubscriberEntity.</returns>
    public static NewsletterSubscriberEntity Create(string email) =>
        new NewsletterSubscriberBuilder().WithEmail(email).Build();

    /// <summary>
    /// Creates a subscriber that has confirmed the subscription.
    /// </summary>
    /// <param name="email">The subscriber email address.</param>
    /// <returns>A confirmed NewsletterSubscriberEntity.</returns>
    public static NewsletterSubscriberEntity CreateConfirmed(string email) =>
        new NewsletterSubscriberBuilder().WithEmail(email).AsConfirmed().Build();

    /// <summary>
    /// Creates a subscriber that confirmed and then unsubscribed.
    /// </summary>
    /// <param name="email">The subscriber email address.</param>
    /// <returns>An unsubscribed NewsletterSubscriberEntity.</returns>
    public static NewsletterSubscriberEntity CreateUnsubscribed(string email) =>
        new NewsletterSubscriberBuilder().WithEmail(email).AsUnsubscribed().Build();
}
