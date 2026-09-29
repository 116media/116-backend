namespace _116.Mailer.Contracts.Application.OutboundEmails;

/// <summary>
/// Routes a message to its recipients, rendering each recipient's copy in their own locale and
/// enqueuing email through the outbox.
/// </summary>
public interface IEmailDispatcher
{
    /// <summary>
    /// Dispatches one message to every recipient it resolves.
    /// </summary>
    /// <param name="message">The message to deliver.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A task representing the dispatch.</returns>
    Task DispatchAsync(OutboundEmail message, CancellationToken cancellationToken = default);
}
