namespace _116.Mailer.Application.Shared.Ports;

/// <summary>
/// Asks the outbox dispatcher to run now instead of waiting for its next scheduled run.
/// </summary>
/// <remarks>
/// Implementations never throw: the dispatcher's cron remains the guarantee, so a failed signal
/// costs one interval of latency and must not fail the caller that enqueued the email.
/// </remarks>
public interface IOutboxDispatchSignal
{
    /// <summary>
    /// Requests an immediate dispatch run.
    /// </summary>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    Task RequestDispatchAsync(CancellationToken cancellationToken);
}
