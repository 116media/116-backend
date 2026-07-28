namespace _116.Mailer.Application.Shared.Exceptions;

/// <summary>
/// Raised by an email provider adapter when a delivery attempt fails.
/// </summary>
/// <param name="message">A description of the delivery failure.</param>
/// <param name="isTransient">Whether the failure is worth retrying.</param>
public class EmailDeliveryException(string message, bool isTransient = true) : Exception(message)
{
    /// <summary>
    /// Transient failures (timeouts, 5xx responses, refused connections) are
    /// retried by the dispatcher on the backoff schedule; permanent ones
    /// (invalid recipient, rejected sender) mark the outbox row failed
    /// immediately.
    /// </summary>
    public bool IsTransient { get; } = isTransient;
}
