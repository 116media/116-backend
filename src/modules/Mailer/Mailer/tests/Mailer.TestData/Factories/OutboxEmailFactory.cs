using _116.Mailer.Domain.Entities;
using _116.Mailer.TestData.Builders.Entities;

namespace _116.Mailer.TestData.Factories;

/// <summary>
/// Named aliases for <see cref="OutboxEmailBuilder" /> chains that three or more tests share verbatim.
/// A shape fewer tests need belongs at the call site as a builder chain, not here —
/// factory names carry the combinatorics, and combinatorics multiply.
/// </summary>
public static class OutboxEmailFactory
{
    /// <summary>
    /// Creates an unsent outbox row enqueued at the given instant, the shape the dispatcher job picks up.
    /// </summary>
    /// <param name="id">The outbox row identifier the test asserts on.</param>
    /// <param name="recipientAddress">The recipient email address.</param>
    /// <param name="subject">The email subject.</param>
    /// <param name="enqueuedAt">The instant the row was enqueued.</param>
    /// <returns>A pending OutboxEmailEntity.</returns>
    public static OutboxEmailEntity CreatePendingDueAt(
        Guid id,
        string recipientAddress,
        string subject,
        DateTime enqueuedAt
    ) =>
        new OutboxEmailBuilder()
            .WithId(id)
            .WithRecipient(recipientAddress)
            .WithContent(subject, "<p>x</p>", "x")
            .WithTemplate("Welcome")
            .At(enqueuedAt)
            .Build();
}
